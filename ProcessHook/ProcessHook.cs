using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace YuGiOh_Forbidden_Memories_Monitor.ProcessHook
{
    public static class ProcessHook
    {
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern void OutputDebugString(string lpOutputString);

        private static void DebugLog(string message)
        {
            Debug.WriteLine(message);
            OutputDebugString(message + Environment.NewLine);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public const uint MEM_COMMIT = 0x1000;

        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_READONLY = 0x02;

        private const string GameIdPattern = "SLUS_014.11;1";
        private const int GameIdOffsetFromRamBase = 0x9244;
        private const int MinRegionScanBytes = 0x10000;
        private const long MinRegionSizeBytes = 0x100000;
        private const long RamSizeBytes = 0x1FFFFF;
        private const int FallbackRegionSizeBytes = 0x10000;
        private const int MaxScanSizeBytes = 16 * 1024 * 1024;
        private const int ProcessAccessFlagsExtra = 0x1000;

        private static readonly ConcurrentDictionary<string, string> _scanLogs = new();
        private static int _scanCounter;
        private static readonly object _logLock = new object();

        public static string GetLastMemoryScanLog()
        {
            return _scanLogs.TryGetValue("last", out var log) ? log : string.Empty;
        }

        private static void SetLastMemoryScanLog(string log)
        {
            var id = $"_{Interlocked.Increment(ref _scanCounter)}";
            _scanLogs[id] = log;
            lock (_logLock)
            {
                _scanLogs["last"] = log;
                if (_scanLogs.Count > 20)
                {
                    var keysToRemove = _scanLogs.Keys.Where(k => k != "last" && k != id).Take(10).ToList();
                    foreach (var key in keysToRemove)
                    {
                        _scanLogs.TryRemove(key, out _);
                    }
                }
            }
        }

        public static IntPtr? OpenProcessHandle(uint processId)
        {
            var handle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION | ProcessAccessFlagsExtra, false, processId);
            if (handle == IntPtr.Zero)
            {
                return null;
            }
            return handle;
        }

        public static bool CloseProcessHandle(IntPtr handle)
        {
            return CloseHandle(handle);
        }

        public static bool ReadMemory(IntPtr processHandle, IntPtr address, byte[] buffer, int size, out int bytesRead)
        {
            return ReadProcessMemory(processHandle, address, buffer, size, out bytesRead);
        }

        public static short ReadInt16(IntPtr processHandle, IntPtr address)
        {
            var buffer = new byte[2];
            if (ReadMemory(processHandle, address, buffer, 2, out var bytesRead) && bytesRead == 2)
            {
                return BitConverter.ToInt16(buffer, 0);
            }
            return 0;
        }

        public static byte ReadByte(IntPtr processHandle, IntPtr address)
        {
            var buffer = new byte[1];
            if (ReadMemory(processHandle, address, buffer, 1, out var bytesRead) && bytesRead == 1)
            {
                return buffer[0];
            }
            return 0;
        }

        public static ulong AutoDetectPS1RAMBase(IntPtr processHandle, out bool gameVerified)
        {
            gameVerified = false;

            var log = new StringBuilder();
            log.AppendLine("=== DuckStation PS1 RAM Detection ===");
            log.AppendLine();
            DebugLog("[DuckStation] === PS1 RAM Detection Started ===");

            log.AppendLine("[INFO] Searching for PS1 game identifiers...");
            DebugLog("[DuckStation] Searching for PS1 game identifiers...");

            var ramBase = FindPS1RAMViaGameIDSearch(processHandle, log);

            if (ramBase.HasValue)
            {
                log.AppendLine($"[SUCCESS] PS1 RAM Found via Game ID: 0x{ramBase.Value:X16}");
                log.AppendLine($"[INFO] RAM End: 0x{ramBase.Value + RamSizeBytes:X16}");
                gameVerified = true;
                SetLastMemoryScanLog(log.ToString());
                DebugLog($"[DuckStation] SUCCESS via Game ID: 0x{ramBase.Value:X16}");
                return ramBase.Value;
            }

            log.AppendLine("[FAILED] All detection methods failed");
            DebugLog("[DuckStation] FAILED: All detection methods exhausted");
            SetLastMemoryScanLog(log.ToString());
            return 0;
        }

        private static ulong? FindPS1RAMViaGameIDSearch(IntPtr processHandle, StringBuilder log)
        {
            log.AppendLine($"--- Game ID Search ---");
            log.AppendLine($"[SEARCH] Searching for: \"{GameIdPattern}\"");

            byte[] gameIdPatternBytes = Encoding.ASCII.GetBytes(GameIdPattern);

            IntPtr address = IntPtr.Zero;
            int regionsScanned = 0;
            int idMatches = 0;
            ulong? foundRamBase = null;

            while (true)
            {
                var mbi = new MEMORY_BASIC_INFORMATION();
                int result = VirtualQueryEx(processHandle, address, out mbi, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>());

                if (result == 0)
                    break;

                regionsScanned++;
                long regionSize = mbi.RegionSize.ToInt64();

                bool isReadable = (mbi.Protect == PAGE_READWRITE ||
                                   mbi.Protect == PAGE_READONLY);

                if (mbi.State == MEM_COMMIT && isReadable && regionSize >= MinRegionSizeBytes)
                {
                    long scanSize = Math.Min(regionSize, MaxScanSizeBytes);
                    byte[] regionBuffer = new byte[scanSize];

                    if (ReadMemory(processHandle, mbi.BaseAddress, regionBuffer, (int)scanSize, out int bytesRead) && bytesRead > MinRegionScanBytes)
                    {
                        for (int i = 0; i <= bytesRead - gameIdPatternBytes.Length; i++)
                        {
                            bool match = true;
                            for (int j = 0; j < gameIdPatternBytes.Length; j++)
                            {
                                if (regionBuffer[i + j] != gameIdPatternBytes[j])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                ulong ramBase = (ulong)mbi.BaseAddress + (ulong)i - GameIdOffsetFromRamBase;
                                idMatches++;

                                log.AppendLine($"[MATCH] Game ID \"{GameIdPattern}\" found at region 0x{mbi.BaseAddress:X16}");
                                log.AppendLine($"  Offset in region: 0x{i:X}");
                                log.AppendLine($"  Calculated RAM base: 0x{ramBase:X16}");

                                byte[] verifyBuffer = new byte[16];
                                if (ReadMemory(processHandle, new IntPtr((long)ramBase + GameIdOffsetFromRamBase), verifyBuffer, 16, out int verifyBytes) && verifyBytes == 16)
                                {
                                    string verifyHeader = Encoding.ASCII.GetString(verifyBuffer).TrimEnd('\0');
                                    log.AppendLine($"  Verified at RAM+0x{GameIdOffsetFromRamBase:X}: {verifyHeader}");
                                }

                                DebugLog($"[DuckStation] Game ID found, RAM base: 0x{ramBase:X16}");
                                foundRamBase = ramBase;
                                break;
                            }
                        }

                        if (foundRamBase.HasValue)
                            break;
                    }
                }

                if (regionSize <= 0)
                    regionSize = FallbackRegionSizeBytes;

                address = (IntPtr)((long)mbi.BaseAddress + regionSize);

                if (foundRamBase.HasValue)
                    break;
            }

            if (foundRamBase.HasValue)
            {
                log.AppendLine($"[SUCCESS] PS1 RAM base confirmed: 0x{foundRamBase.Value:X16}");
                DebugLog($"[DuckStation] SUCCESS: RAM 0x{foundRamBase.Value:X16}");
                return foundRamBase.Value;
            }

            log.AppendLine($"[RESULT] Scanned {regionsScanned} regions, found {idMatches} ID matches");
            DebugLog($"[DuckStation] Game ID search: {idMatches} matches");
            return null;
        }
    }
}