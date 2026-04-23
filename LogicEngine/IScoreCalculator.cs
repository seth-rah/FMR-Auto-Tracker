namespace YuGiOh_Forbidden_Memories_Monitor.LogicEngine
{
    public interface IScoreCalculator
    {
        int CalculateRankScore(int[] stats, ushort lp, ushort cardsUsed, out int[] contributions, out int[] victoryScores);
        
        string GetDuelRankFromScore(int score);
        
        ScoreRankInfo GetRankInfo(int score);
    }

    public struct ScoreRankInfo
    {
        public string Rank { get; init; }
        public string Category { get; init; }
        public int ScoreValue { get; init; }
        public int LowerBound { get; init; }
        public int UpperBound { get; init; }
    }
}