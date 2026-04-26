using System;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public interface IProcessMonitor : IDisposable
    {
        bool IsAttached { get; }
        
        ulong RamBaseAddress { get; }
        
        GameState? CurrentGameState { get; }
        
        event EventHandler<GameState>? GameStateUpdated;
        
        event EventHandler<string>? StatusChanged;
        
        event EventHandler<string>? ProcessNameChanged;
        
        bool TryAttachToProcess();
        
        bool AttachToProcess(uint processId, string processName);
        
        void StartPolling(int intervalMs = 16);
        
        void StopPolling();
        
        GameState? ReadCurrentState();
        
        void Detach();
        
        void UpdateP1LifePointsAddress(uint address);
        
        void UpdateP2LifePointsAddress(uint address);

        string GetCurrentEmulatorType();

        void SetPreferredEmulator(string emulatorType);

        (bool duckstationAvailable, bool bizhawkAvailable) GetAvailableEmulatorsStatus();

        bool TrySwitchToOtherEmulator();
    }
}