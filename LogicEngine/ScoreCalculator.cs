using System;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOh_Forbidden_Memories_Monitor.LogicEngine
{
    public class ScoreCalculator : IScoreCalculator
    {
        private const int BaseScore = 52;
        
        private static readonly IReadOnlyList<RankThreshold> _rankThresholds = new List<RankThreshold>
        {
            new(90, 150, "S", "POW"),
            new(80, 89, "A", "POW"),
            new(70, 79, "B", "POW"),
            new(60, 69, "C", "POW"),
            new(50, 59, "D", "POW"),
            new(40, 49, "D", "TEC"),
            new(30, 39, "C", "TEC"),
            new(20, 29, "B", "TEC"),
            new(10, 19, "A", "TEC"),
            new(-50, 9, "S", "TEC")
        };

        public int CalculateRankScore(int[] stats, ushort lp, ushort cardsUsed, out int[] contributions)
        {
            contributions = new int[10];
            
            if (stats == null || stats.Length < 9)
            {
                return BaseScore;
            }
            
            int score = BaseScore;
            
            contributions[0] = EvaluateStat(ScoreTierRegistry.Turns, stats.Length > 1 ? stats[1] : 0);
            score += contributions[0];
            
            contributions[1] = EvaluateStat(ScoreTierRegistry.EffectiveAttacks, stats.Length > 2 ? stats[2] : 0);
            score += contributions[1];
            
            contributions[2] = EvaluateStat(ScoreTierRegistry.DefensiveWins, stats.Length > 3 ? stats[3] : 0);
            score += contributions[2];
            
            contributions[3] = EvaluateStat(ScoreTierRegistry.FaceDowns, stats.Length > 4 ? stats[4] : 0);
            score += contributions[3];
            
            contributions[4] = EvaluateStat(ScoreTierRegistry.Fusions, stats.Length > 5 ? stats[5] : 0);
            score += contributions[4];
            
            contributions[5] = EvaluateStat(ScoreTierRegistry.EquipMagic, stats.Length > 6 ? stats[6] : 0);
            score += contributions[5];
            
            contributions[6] = EvaluateStat(ScoreTierRegistry.PureMagic, stats.Length > 7 ? stats[7] : 0);
            score += contributions[6];
            
            contributions[7] = EvaluateStat(ScoreTierRegistry.TrapsTriggered, stats.Length > 8 ? stats[8] : 0);
            score += contributions[7];
            
            contributions[8] = EvaluateStat(ScoreTierRegistry.CardsUsed, cardsUsed);
            score += contributions[8];
            
            contributions[9] = EvaluateStat(ScoreTierRegistry.LifePoints, lp);
            score += contributions[9];
            
            return score;
        }

        public string GetDuelRankFromScore(int score)
        {
            foreach (var threshold in _rankThresholds)
            {
                if (score >= threshold.LowerBound && score <= threshold.UpperBound)
                {
                    return $"{threshold.Letter} {threshold.Category}";
                }
            }
            return "S TEC";
        }

        private static int EvaluateStat(IReadOnlyList<ScoreTier> tiers, int value)
        {
            return ScoreTierRegistry.Evaluate(tiers, value);
        }

        private readonly struct RankThreshold
        {
            public int LowerBound { get; }
            public int UpperBound { get; }
            public string Letter { get; }
            public string Category { get; }

            public RankThreshold(int lowerBound, int upperBound, string letter, string category)
            {
                LowerBound = lowerBound;
                UpperBound = upperBound;
                Letter = letter;
                Category = category;
            }
        }
    }
}