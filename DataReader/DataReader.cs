using System;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;
using YuGiOh_Forbidden_Memories_Monitor.LogicEngine;
using YuGiOh_Forbidden_Memories_Monitor.ProcessHook;

namespace YuGiOh_Forbidden_Memories_Monitor.DataReader
{
    public sealed class DataReader : IDataReader
    {
        private readonly IntPtr _processHandle;
        private readonly ulong _ramBaseAddress;
        private long _baseOffset;
        private uint _p1LifePointsAddress;
        private uint _p2LifePointsAddress;
        private uint _processId;
        private string _processName = string.Empty;
        private bool _gameVerified;
        private string _memoryScanLog = string.Empty;
        
        private readonly IScoreCalculator _scoreCalculator;

        public DataReader(IntPtr processHandle, ulong ramBaseAddress)
            : this(processHandle, ramBaseAddress, new ScoreCalculator())
        {
        }

        public DataReader(IntPtr processHandle, ulong ramBaseAddress, IScoreCalculator scoreCalculator)
        {
            _processHandle = processHandle;
            _ramBaseAddress = ramBaseAddress;
            _scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
            _baseOffset = (long)(ramBaseAddress - MemoryMap.KSEG0_BASE);
            _p1LifePointsAddress = MemoryMap.P1LifePointsAddress;
            _p2LifePointsAddress = MemoryMap.P2LifePointsAddress;
        }

        public void SetProcessInfo(uint processId, string processName, bool gameVerified, string memoryScanLog)
        {
            _processId = processId;
            _processName = processName;
            _gameVerified = gameVerified;
            _memoryScanLog = memoryScanLog;
        }

        public void SetP1LifePointsAddress(uint address)
        {
            _p1LifePointsAddress = address;
        }

        public void SetP2LifePointsAddress(uint address)
        {
            _p2LifePointsAddress = address;
        }

        private IntPtr ResolveAddress(uint absoluteAddress)
        {
            return new IntPtr((long)(absoluteAddress + _baseOffset));
        }

        public GameState ReadGameState()
        {
            var builder = new GameState.Builder
            {
                IsProcessAttached = _processHandle != IntPtr.Zero,
                RamBaseAddress = (uint)_ramBaseAddress,
                ProcessId = _processId,
                ProcessName = _processName,
                GameVerified = _gameVerified,
                MemoryScanLog = _memoryScanLog,
                GameIdText = TryReadString(ResolveAddress(MemoryMap.GameIdAddress), 13)
            };

            builder.P1LifePoints = TryReadUInt16(ResolveAddress(_p1LifePointsAddress));
            builder.P2LifePoints = TryReadUInt16(ResolveAddress(_p2LifePointsAddress));

            int[] stats = ReadStatValues();
            builder.StatValues = stats;
            builder.VictoryConditions = (byte)stats[0];
            builder.StatTurns = (byte)stats[1];
            builder.StatEffectiveAttacks = (byte)stats[2];
            builder.StatDefensiveWins = (byte)stats[3];
            builder.StatFaceDowns = (byte)stats[4];
            builder.StatFusions = (byte)stats[5];
            builder.StatEquipMagic = (byte)stats[6];
            builder.StatPureMagic = (byte)stats[7];
            builder.StatTrapsTriggered = (byte)stats[8];
            
            ReadResources(builder);

            CalculateScore(builder, stats);

            return builder.Build();
        }

        private int[] ReadStatValues()
        {
            return new int[]
            {
                ReadByte(ResolveAddress(MemoryMap.VictoryConditions)),
                ReadByte(ResolveAddress(MemoryMap.StatTurns)),
                ReadByte(ResolveAddress(MemoryMap.StatEffectiveAttacks)),
                ReadByte(ResolveAddress(MemoryMap.StatDefensiveWins)),
                ReadByte(ResolveAddress(MemoryMap.StatFaceDowns)),
                ReadByte(ResolveAddress(MemoryMap.StatFusions)),
                ReadByte(ResolveAddress(MemoryMap.StatEquipMagic)),
                ReadByte(ResolveAddress(MemoryMap.StatPureMagic)),
                ReadByte(ResolveAddress(MemoryMap.StatTrapsTriggered))
            };
        }

        private void ReadResources(GameState.Builder builder)
        {
            try
            {
                builder.DuelLifePoints = ReadUInt16(ResolveAddress(MemoryMap.DuelLifePointsAddress));
                builder.CardsUsed = ReadUInt16(ResolveAddress(MemoryMap.CardsUsedAddress));
                builder.Starchips = ReadUInt16(ResolveAddress(MemoryMap.StarchipsAddress));
            }
            catch
            {
                builder.DuelLifePoints = 0;
                builder.CardsUsed = 0;
                builder.Starchips = 0;
            }

            builder.StatChangeField = ReadByte(ResolveAddress(MemoryMap.StatChangeField));
            builder.StatCardDestruction = ReadByte(ResolveAddress(MemoryMap.StatCardDestruction));
            builder.StatDefensiveLoses = ReadByte(ResolveAddress(MemoryMap.StatDefensiveLoses));
        }

        private void CalculateScore(GameState.Builder builder, int[] stats)
        {
            int score = _scoreCalculator.CalculateRankScore(
                stats, 
                builder.DuelLifePoints, 
                builder.CardsUsed, 
                out int[] contributions, 
                out int[] victoryScores);

            builder.TotalScore = score;
            builder.ScoreContributions = contributions;
            builder.VictoryScores = victoryScores;
            builder.ScoreRank = _scoreCalculator.GetDuelRankFromScore(score);
            builder.ScoreRankExodia = _scoreCalculator.GetDuelRankFromScore(victoryScores[0]);
            builder.ScoreRankTotalAnnihilation = _scoreCalculator.GetDuelRankFromScore(victoryScores[1]);
            builder.ScoreRankAttrition = _scoreCalculator.GetDuelRankFromScore(victoryScores[2]);
        }

        private string TryReadString(IntPtr address, int length)
        {
            try
            {
                return ReadString(address, length);
            }
            catch
            {
                return string.Empty;
            }
        }

        private ushort TryReadUInt16(IntPtr address)
        {
            try
            {
                return ReadUInt16(address);
            }
            catch
            {
                return 0;
            }
        }

        private string ReadString(IntPtr address, int length)
        {
            byte[] buffer = new byte[length];
            ProcessHook.ProcessHook.ReadMemory(_processHandle, address, buffer, length, out int bytesRead);
            return System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
        }

        private ushort ReadUInt16(IntPtr address)
        {
            return (ushort)ProcessHook.ProcessHook.ReadInt16(_processHandle, address);
        }

        private byte ReadByte(IntPtr address)
        {
            return ProcessHook.ProcessHook.ReadByte(_processHandle, address);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}