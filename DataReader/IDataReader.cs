using YuGiOh_Forbidden_Memories_Monitor.DataModel;
using YuGiOh_Forbidden_Memories_Monitor.ProcessHook;

namespace YuGiOh_Forbidden_Memories_Monitor.DataReader
{
    public interface IDataReader
    {
        GameState ReadGameState();
        
        void SetProcessInfo(uint processId, string processName, bool gameVerified, string memoryScanLog);
    }
}