using System;
using System.Collections.Generic;

namespace YuGiOh_Forbidden_Memories_Monitor.DataModel
{
    public sealed class GameState
    {
        public bool IsProcessAttached { get; }
        public string ProcessName { get; }
        public uint ProcessId { get; }
        public uint RamBaseAddress { get; }
        public bool GameVerified { get; }
        public string MemoryScanLog { get; }
        public string GameIdText { get; }

        public ushort P1LifePoints { get; }
        public ushort P2LifePoints { get; }

        public IReadOnlyList<int> StatValues { get; }
        public IReadOnlyList<int> ScoreContributions { get; }
        public int TotalScore { get; }
        public string ScoreRank { get; }

        public byte VictoryConditions { get; }
        public byte StatTurns { get; }
        public byte StatEffectiveAttacks { get; }
        public byte StatDefensiveWins { get; }
        public byte StatFaceDowns { get; }
        public byte StatPureMagic { get; }
        public byte StatTrapsTriggered { get; }
        public byte StatFusions { get; }
        public byte StatEquipMagic { get; }
        public byte StatChangeField { get; }
        public byte StatCardDestruction { get; }
        public byte StatDefensiveLoses { get; }

        public ushort DuelLifePoints { get; }
        public ushort CardsUsed { get; }
        public ushort Starchips { get; }

        private GameState(Builder builder)
        {
            IsProcessAttached = builder.IsProcessAttached;
            ProcessName = builder.ProcessName;
            ProcessId = builder.ProcessId;
            RamBaseAddress = builder.RamBaseAddress;
            GameVerified = builder.GameVerified;
            MemoryScanLog = builder.MemoryScanLog;
            GameIdText = builder.GameIdText;
            P1LifePoints = builder.P1LifePoints;
            P2LifePoints = builder.P2LifePoints;
            StatValues = builder.StatValues;
            ScoreContributions = builder.ScoreContributions;
            TotalScore = builder.TotalScore;
            ScoreRank = builder.ScoreRank;
            VictoryConditions = builder.VictoryConditions;
            StatTurns = builder.StatTurns;
            StatEffectiveAttacks = builder.StatEffectiveAttacks;
            StatDefensiveWins = builder.StatDefensiveWins;
            StatFaceDowns = builder.StatFaceDowns;
            StatPureMagic = builder.StatPureMagic;
            StatTrapsTriggered = builder.StatTrapsTriggered;
            StatFusions = builder.StatFusions;
            StatEquipMagic = builder.StatEquipMagic;
            StatChangeField = builder.StatChangeField;
            StatCardDestruction = builder.StatCardDestruction;
            StatDefensiveLoses = builder.StatDefensiveLoses;
            DuelLifePoints = builder.DuelLifePoints;
            CardsUsed = builder.CardsUsed;
            Starchips = builder.Starchips;
        }

        public static GameState Empty => new(new Builder
        {
            IsProcessAttached = false,
            ProcessName = string.Empty,
            RamBaseAddress = MemoryMap.KSEG0_BASE,
            ScoreRank = "--"
        });

        public sealed class Builder
        {
            public bool IsProcessAttached { get; set; }
            public string ProcessName { get; set; } = string.Empty;
            public uint ProcessId { get; set; }
            public uint RamBaseAddress { get; set; } = MemoryMap.KSEG0_BASE;
            public bool GameVerified { get; set; }
            public string MemoryScanLog { get; set; } = string.Empty;
            public string GameIdText { get; set; } = string.Empty;

            public ushort P1LifePoints { get; set; }
            public ushort P2LifePoints { get; set; }

            public int[] StatValues { get; set; } = Array.Empty<int>();
            public int[] ScoreContributions { get; set; } = Array.Empty<int>();
            public int TotalScore { get; set; }
            public string ScoreRank { get; set; } = "--";

            public byte VictoryConditions { get; set; }
            public byte StatTurns { get; set; }
            public byte StatEffectiveAttacks { get; set; }
            public byte StatDefensiveWins { get; set; }
            public byte StatFaceDowns { get; set; }
            public byte StatPureMagic { get; set; }
            public byte StatTrapsTriggered { get; set; }
            public byte StatFusions { get; set; }
            public byte StatEquipMagic { get; set; }
            public byte StatChangeField { get; set; }
            public byte StatCardDestruction { get; set; }
            public byte StatDefensiveLoses { get; set; }

            public ushort DuelLifePoints { get; set; }
            public ushort CardsUsed { get; set; }
            public ushort Starchips { get; set; }

            internal Builder() { }

            public GameState Build() => new(this);
        }
    }
}