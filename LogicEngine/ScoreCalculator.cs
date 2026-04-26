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
            contributions = new int[11];
            
            int score = BaseScore;
            
            contributions[1] = EvaluateStat(ScoreTierRegistry.Turns, stats[1]);
            score += contributions[1];
            
            contributions[2] = EvaluateStat(ScoreTierRegistry.EffectiveAttacks, stats[2]);
            score += contributions[2];
            
            contributions[3] = EvaluateStat(ScoreTierRegistry.DefensiveWins, stats[3]);
            score += contributions[3];
            
            contributions[4] = EvaluateStat(ScoreTierRegistry.FaceDowns, stats[4]);
            score += contributions[4];
            
            contributions[5] = EvaluateStat(ScoreTierRegistry.Fusions, stats[5]);
            score += contributions[5];
            
            contributions[6] = EvaluateStat(ScoreTierRegistry.EquipMagic, stats[6]);
            score += contributions[6];
            
            contributions[7] = EvaluateStat(ScoreTierRegistry.PureMagic, stats[7]);
            score += contributions[7];
            
            contributions[8] = EvaluateStat(ScoreTierRegistry.TrapsTriggered, stats[8]);
            score += contributions[8];
            
            contributions[9] = EvaluateStat(ScoreTierRegistry.CardsUsed, cardsUsed);
            score += contributions[9];
            
            contributions[10] = EvaluateStat(ScoreTierRegistry.LifePoints, lp);
            score += contributions[10];
            
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