using System;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;
using YuGiOh_Forbidden_Memories_Monitor.ProcessHook;

namespace YuGiOh_Forbidden_Memories_Monitor.DataReader
{
    public interface IDataReader : IDisposable
    {
        GameState ReadGameState();
        
        void SetProcessInfo(uint processId, string processName, bool gameVerified, string memoryScanLog);
        
        void SetP1LifePointsAddress(uint address);
        
        void SetP2LifePointsAddress(uint address);
    }
}