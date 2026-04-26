using System.Collections.Generic;

namespace YuGiOh_Forbidden_Memories_Monitor.LogicEngine
{
    public static class ScoreTierRegistry
    {
        public static IReadOnlyList<ScoreTier> Turns => _turns;
        public static IReadOnlyList<ScoreTier> EffectiveAttacks => _effectiveAttacks;
        public static IReadOnlyList<ScoreTier> DefensiveWins => _defensiveWins;
        public static IReadOnlyList<ScoreTier> FaceDowns => _faceDowns;
        public static IReadOnlyList<ScoreTier> Fusions => _fusions;
        public static IReadOnlyList<ScoreTier> EquipMagic => _equipMagic;
        public static IReadOnlyList<ScoreTier> PureMagic => _pureMagic;
        public static IReadOnlyList<ScoreTier> TrapsTriggered => _trapsTriggered;
        public static IReadOnlyList<ScoreTier> CardsUsed => _cardsUsed;
        public static IReadOnlyList<ScoreTier> LifePoints => _lifePoints;
        public static IReadOnlyList<ScoreTier> ComboPlays => _comboPlays;

        public static int Evaluate(IReadOnlyList<ScoreTier> tiers, int value)
        {
            foreach (var tier in tiers)
            {
                if (value >= tier.MinValue && value <= tier.MaxValue)
                {
                    return tier.Score;
                }
            }
            return 0;
        }

        private static readonly List<ScoreTier> _turns = new()
        {
            new(0, 4, 12),
            new(5, 8, 8),
            new(9, 28, 0),
            new(29, 32, -8),
            new(33, int.MaxValue, -12)
        };

        private static readonly List<ScoreTier> _effectiveAttacks = new()
        {
            new(0, 1, 4),
            new(2, 3, 2),
            new(4, 9, 0),
            new(10, 19, -2),
            new(20, int.MaxValue, -4)
        };

        private static readonly List<ScoreTier> _defensiveWins = new()
        {
            new(0, 1, 0),
            new(2, 5, -10),
            new(6, 9, -20),
            new(10, 14, -30),
            new(15, int.MaxValue, -40)
        };

        private static readonly List<ScoreTier> _faceDowns = new()
        {
            new(0, 0, 0),
            new(1, 10, -2),
            new(11, 20, -4),
            new(21, 30, -6),
            new(31, int.MaxValue, -8)
        };

        private static readonly List<ScoreTier> _fusions = new()
        {
            new(0, 0, 4),
            new(1, 4, 0),
            new(5, 9, -4),
            new(10, 14, -8),
            new(15, int.MaxValue, -12)
        };

        private static readonly List<ScoreTier> _equipMagic = new()
        {
            new(0, 0, 4),
            new(1, 4, 0),
            new(5, 9, -4),
            new(10, 14, -8),
            new(15, int.MaxValue, -12)
        };

        private static readonly List<ScoreTier> _pureMagic = new()
        {
            new(0, 0, 2),
            new(1, 3, -4),
            new(4, 6, -8),
            new(7, 9, -12),
            new(10, int.MaxValue, -16)
        };

        private static readonly List<ScoreTier> _trapsTriggered = new()
        {
            new(0, 0, 2),
            new(1, 2, -8),
            new(3, 4, -16),
            new(5, 6, -24),
            new(7, int.MaxValue, -32)
        };

        private static readonly List<ScoreTier> _cardsUsed = new()
        {
            new(0, 8, 15),
            new(9, 12, 12),
            new(13, 32, 0),
            new(33, 36, -5),
            new(37, int.MaxValue, -7)
        };

        private static readonly List<ScoreTier> _lifePoints = new()
        {
            new(8000, int.MaxValue, 6),
            new(7000, 7999, 4),
            new(1000, 6999, 0),
            new(100, 999, -5),
            new(0, 99, -7)
        };

        private static readonly List<ScoreTier> _comboPlays = new()
        {
            new(0, 0, 2),
            new(1, 2, 0),
            new(3, 5, -2),
            new(6, 9, -4),
            new(10, int.MaxValue, -6)
        };
    }

    public readonly struct ScoreTier
    {
        public int MinValue { get; }
        public int MaxValue { get; }
        public int Score { get; }

        public ScoreTier(int minValue, int maxValue, int score)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Score = score;
        }

        public bool Contains(int value) => value >= MinValue && value <= MaxValue;

        public string GetRangeDisplay() => MaxValue == int.MaxValue
            ? $"{MinValue}+"
            : (MinValue == MaxValue ? $"{MinValue}" : $"{MinValue}-{MaxValue}");
    }
}