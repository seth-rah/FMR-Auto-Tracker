using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private const uint DefaultRAMBase = 0x80000000;

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

        public event EventHandler<GameState>? GameStateUpdated;
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ProcessNameChanged;

        public void SetPreferredEmulator(string emulatorType)
        {
            _preferredEmulator = emulatorType;
        }

        public bool TryAttachToProcess()
        {
            var (duckstationProcess, bizhawkProcess) = GetAvailableEmulators();

            if (!string.IsNullOrEmpty(_preferredEmulator))
            {
                if (_preferredEmulator == "DuckStation" && duckstationProcess != null)
                {
                    _currentEmulatorType = "DuckStation";
                    bool success = AttachToProcess((uint)duckstationProcess.Id, duckstationProcess.ProcessName, "DuckStation");
                    if (success)
                    {
                        StartPolling(16);
                    }
                    return success;
                }

                if (_preferredEmulator == "Bizhawk" && bizhawkProcess != null)
                {
                    _currentEmulatorType = "Bizhawk";
                    bool success = AttachToProcess((uint)bizhawkProcess.Id, bizhawkProcess.ProcessName, "Bizhawk");
                    if (success)
                    {
                        StartPolling(16);
                    }
                    return success;
                }
            }

            string emulatorName = !string.IsNullOrEmpty(_preferredEmulator) ? _preferredEmulator : "Selected emulator";
            StatusChanged?.Invoke(this, $"{emulatorName} not found. Waiting...");
            return false;
        }

        private (Process? duckstation, Process? bizhawk) GetAvailableEmulators()
        {
            var processes = Process.GetProcesses().ToList();
            Process? duckstationProcess = null;
            Process? bizhawkProcess = null;

            foreach (var process in processes)
            {
                if (process.ProcessName.StartsWith(DuckStationProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    duckstationProcess = process;
                }
                else if (process.ProcessName.Equals(BizhawkProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    bizhawkProcess = process;
                }

                if (duckstationProcess != null && bizhawkProcess != null)
                {
                    break;
                }
            }

            return (duckstationProcess, bizhawkProcess);
        }

        public string GetCurrentEmulatorType() => _currentEmulatorType;

        public (bool duckstationAvailable, bool bizhawkAvailable) GetAvailableEmulatorsStatus()
        {
            var (duckstation, bizhawk) = GetAvailableEmulators();
            return (duckstation != null, bizhawk != null);
        }

        public bool TrySwitchToOtherEmulator()
        {
            if (!_isAttached)
            {
                return TryAttachToProcess();
            }

            var (duckstation, bizhawk) = GetAvailableEmulators();

            if (_currentEmulatorType == "DuckStation" && bizhawk != null)
            {
                DetachNoStatus();
                return AttachToProcess((uint)bizhawk.Id, bizhawk.ProcessName, "Bizhawk");
            }

            if (_currentEmulatorType == "Bizhawk" && duckstation != null)
            {
                DetachNoStatus();
                return AttachToProcess((uint)duckstation.Id, duckstation.ProcessName, "DuckStation");
            }

            Detach();
            return false;
        }

        private void DetachNoStatus()
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
            _currentProcessName = string.Empty;
            _currentEmulatorType = string.Empty;
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

            ProcessNameChanged?.Invoke(this, $"{emulatorType}: {processName}");
            return true;
        }

        public bool AttachToProcess(uint processId, string processName)
        {
            return AttachToProcess(processId, processName, "Unknown");
        }

        public void DiscoverMemoryAddresses()
        {
            if (!_isAttached || _processHandle == IntPtr.Zero)
            {
                return;
            }

            // Note: The actual addresses will be discovered at runtime
            // For now, we'll use the DataReader's read methods which use the MemoryMap addresses
            // Users can use the Memory Search tab to find dynamic addresses
            StatusChanged?.Invoke(this, "Memory addresses ready. Use Memory Search tab to find dynamic values.");
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
                GameStateUpdated?.Invoke(this, CurrentGameState);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("ProcessMonitor.PollGameState", ex);
            }
        }

        public GameState? ReadCurrentState()
        {
            if (!_isAttached || _dataReader == null)
            {
                return null;
            }

            try
            {
                CurrentGameState = _dataReader.ReadGameState();
                return CurrentGameState;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("ProcessMonitor.ReadCurrentState", ex);
                return null;
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

        public void UpdateP1LifePointsAddress(uint address)
        {
            if (_dataReader != null)
            {
                _dataReader.SetP1LifePointsAddress(address);
            }
        }

        public void UpdateP2LifePointsAddress(uint address)
        {
            if (_dataReader != null)
            {
                _dataReader.SetP2LifePointsAddress(address);
            }
        }

        ~ProcessMonitor()
        {
            Dispose();
        }
    }
}