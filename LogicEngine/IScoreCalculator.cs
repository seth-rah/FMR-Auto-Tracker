namespace YuGiOh_Forbidden_Memories_Monitor.LogicEngine
{
    public interface IScoreCalculator
    {
        int CalculateRankScore(int[] stats, ushort lp, ushort cardsUsed, out int[] contributions);
        
        string GetDuelRankFromScore(int score);
    }
}