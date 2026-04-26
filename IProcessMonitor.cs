using System;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public interface IProcessMonitor : IDisposable
    {
        bool IsAttached { get; }
        
        GameState? CurrentGameState { get; }
        
        event EventHandler<string>? StatusChanged;
        
        bool TryAttachToProcess();
        
        void StartPolling(int intervalMs = 16);
        
        void StopPolling();
        
        void Detach();

        void SetPreferredEmulator(string emulatorType);
    }
}