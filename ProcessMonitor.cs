using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;
using YuGiOh_Forbidden_Memories_Monitor.ProcessHook;
using DataReaderLibrary = YuGiOh_Forbidden_Memories_Monitor.DataReader;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public sealed class ProcessMonitor : IProcessMonitor
    {
        private const string DuckStationProcessName = "duckstation";
        private const string BizhawkProcessName = "EmuHawk";
        private const string DuckStationDisplayName = "DuckStation";
        private const string BizhawkDisplayName = "Bizhawk";
        private const int DefaultPollingIntervalMs = 16;

        private string _preferredEmulator = string.Empty;
        private string _currentEmulatorType = string.Empty;

        private IntPtr _processHandle;
        private ulong _ramBaseAddress;
        private DataReaderLibrary.DataReader? _dataReader;
        private Timer? _pollingTimer;
        private bool _isAttached;
        private bool _disposed;
        private string _currentProcessName = string.Empty;

        public bool IsAttached => _isAttached;
        public ulong RamBaseAddress => _ramBaseAddress;
        public DataReaderLibrary.DataReader? Reader => _dataReader;
        public GameState? CurrentGameState { get; private set; }

        public event EventHandler<string>? StatusChanged;

        public void SetPreferredEmulator(string emulatorType)
        {
            _preferredEmulator = emulatorType;
        }

        public bool TryAttachToProcess()
        {
            var (duckstationId, bizhawkId, duckstationName, bizhawkName) = GetAvailableEmulators();

            if (!string.IsNullOrEmpty(_preferredEmulator))
            {
                if (_preferredEmulator == DuckStationDisplayName && duckstationId.HasValue)
                {
                    _currentEmulatorType = DuckStationDisplayName;
                    bool success = AttachToProcess(duckstationId.Value, duckstationName!, DuckStationDisplayName);
                    if (success)
                    {
                        StartPolling(DefaultPollingIntervalMs);
                    }
                    return success;
                }

                if (_preferredEmulator == BizhawkDisplayName && bizhawkId.HasValue)
                {
                    _currentEmulatorType = BizhawkDisplayName;
                    bool success = AttachToProcess(bizhawkId.Value, bizhawkName!, BizhawkDisplayName);
                    if (success)
                    {
                        StartPolling(DefaultPollingIntervalMs);
                    }
                    return success;
                }
            }

            string emulatorName = !string.IsNullOrEmpty(_preferredEmulator) ? _preferredEmulator : "Selected emulator";
            StatusChanged?.Invoke(this, $"{emulatorName} not found. Waiting...");
            return false;
        }

        private (uint? duckstationId, uint? bizhawkId, string? duckstationName, string? bizhawkName) GetAvailableEmulators()
        {
            uint? duckstationId = null;
            uint? bizhawkId = null;
            string? duckstationName = null;
            string? bizhawkName = null;

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName.StartsWith(DuckStationProcessName, StringComparison.OrdinalIgnoreCase))
                    {
                        duckstationId = (uint)process.Id;
                        duckstationName = process.ProcessName;
                    }
                    else if (process.ProcessName.Equals(BizhawkProcessName, StringComparison.OrdinalIgnoreCase))
                    {
                        bizhawkId = (uint)process.Id;
                        bizhawkName = process.ProcessName;
                    }

                    if (duckstationId.HasValue && bizhawkId.HasValue)
                    {
                        break;
                    }
                }
                catch
                {
                }
            }

            return (duckstationId, bizhawkId, duckstationName, bizhawkName);
        }

        public bool AttachToProcess(uint processId, string processName, string emulatorType)
        {
            var handle = ProcessHook.ProcessHook.OpenProcessHandle(processId);
            if (handle == null)
            {
                StatusChanged?.Invoke(this, $"Failed to open {emulatorType} process.");
                return false;
            }

            _processHandle = handle.Value;
            
            bool gameVerified;
            _ramBaseAddress = ProcessHook.ProcessHook.AutoDetectPS1RAMBase(_processHandle, out gameVerified);
            var scanLog = ProcessHook.ProcessHook.GetLastMemoryScanLog();
            
            if (gameVerified)
            {
                StatusChanged?.Invoke(this, $"Auto-detected YGO FM! RAM: 0x{_ramBaseAddress:X16}");
            }
            else
            {
                StatusChanged?.Invoke(this, $"Attached: {processName} | RAM: 0x{_ramBaseAddress:X16}");
            }
            
            _dataReader = new DataReaderLibrary.DataReader(_processHandle, _ramBaseAddress);
            _dataReader.SetProcessInfo(processId, processName, gameVerified, scanLog);
            _isAttached = true;
            _currentProcessName = processName;
            _currentEmulatorType = emulatorType;

            return true;
        }

        public void StartPolling(int intervalMs = 16)
        {
            _pollingTimer?.Dispose();
            _pollingTimer = new Timer(PollGameState, null, 0, intervalMs);
        }

        public void StopPolling()
        {
            _pollingTimer?.Dispose();
            _pollingTimer = null;
            StatusChanged?.Invoke(this, "Memory polling stopped.");
        }

        private void PollGameState(object? state)
        {
            if (!_isAttached || _dataReader == null)
            {
                return;
            }

            try
            {
                CurrentGameState = _dataReader.ReadGameState();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("ProcessMonitor.PollGameState", ex);
            }
        }

        public void Detach()
        {
            StopPolling();

            if (_processHandle != IntPtr.Zero)
            {
                ProcessHook.ProcessHook.CloseProcessHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }

            _dataReader = null;
            _isAttached = false;
            CurrentGameState = null;
            string emulatorType = _currentEmulatorType;
            _currentProcessName = string.Empty;
            _currentEmulatorType = string.Empty;

            StatusChanged?.Invoke(this, string.IsNullOrEmpty(emulatorType) 
                ? "Detached." 
                : $"Detached from {emulatorType}.");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Detach();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~ProcessMonitor()
        {
            Dispose();
        }
    }
}