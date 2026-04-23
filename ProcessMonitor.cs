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
        private const string TargetProcessName = "duckstation-qt";
        private const uint DefaultRAMBase = 0x80000000;

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

        public bool TryAttachToProcess()
        {
            var processes = Process.GetProcesses();
            Process? duckstationProcess = null;
            
            foreach (var process in processes)
            {
                if (process.ProcessName.StartsWith("duckstation", StringComparison.OrdinalIgnoreCase))
                {
                    duckstationProcess = process;
                    break;
                }
            }
            
            if (duckstationProcess == null)
            {
                StatusChanged?.Invoke(this, "DuckStation not found. Waiting...");
                return false;
            }

            bool success = AttachToProcess((uint)duckstationProcess.Id, duckstationProcess.ProcessName);
            if (success)
            {
                StartPolling(16);
            }
            return success;
        }

        public bool AttachToProcess(uint processId, string processName)
        {
            var handle = ProcessHook.ProcessHook.OpenProcessHandle(processId);
            if (handle == null)
            {
                StatusChanged?.Invoke(this, "Failed to open DuckStation process.");
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

            ProcessNameChanged?.Invoke(this, processName);
            return true;
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
            _currentProcessName = string.Empty;

            StatusChanged?.Invoke(this, "Detached from DuckStation.");
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