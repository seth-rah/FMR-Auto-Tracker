namespace YuGiOh_Forbidden_Memories_Monitor.DataModel
{
    public static class MemoryMap
    {
        public const uint KSEG0_BASE = 0x80000000;

        // Game ID - At PS1 RAM offset 0x00009244-0x00009250 = "SLUS_014.11;1"
        public const uint GameIdAddress = 0x80009244;

        // Display resources
        public const uint P1LifePointsAddress = 0x800EA004;
        public const uint P2LifePointsAddress = 0x800EA024;
        public const uint DuelLifePointsAddress = 0x800EA004;
        public const uint CardsUsedAddress = 0x800EA008;
        public const uint StarchipsAddress = 0x801D07E0;

        // Duel Score Calculation Stats - Base: 0x800e9ff0 (13 bytes)
        public const uint VictoryConditions = 0x800e9ff0;
        public const uint StatTurns = 0x800e9ff1;
        public const uint StatEffectiveAttacks = 0x800e9ff2;
        public const uint StatDefensiveWins = 0x800e9ff3;
        public const uint StatFaceDowns = 0x800e9ff4;
        public const uint StatPureMagic = 0x800e9ff5;
        public const uint StatTrapsTriggered = 0x800e9ff6;
        public const uint StatComboPlays = 0x800e9ff7;
        public const uint StatFusions = 0x800e9ff8;
        public const uint StatEquipMagic = 0x800e9ff9;
        public const uint StatChangeField = 0x800e9ffa;
        public const uint StatCardDestruction = 0x800e9ffb;
        public const uint StatDefensiveLoses = 0x800e9ffc;
    }
}