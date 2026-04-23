using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuGiOh_Forbidden_Memories_Monitor.DataModel;
using YuGiOh_Forbidden_Memories_Monitor.LogicEngine;

namespace YuGiOh_Forbidden_Memories_Monitor
{
    public partial class MainWindow : Window
    {
        private ProcessMonitor? _processMonitor;
        private System.Windows.Threading.DispatcherTimer? _uiTimer;

        public MainWindow()
        {
            InitializeComponent();

            if (App.DebugMode)
            {
                DebugExpander.Visibility = Visibility.Visible;
            }

            _processMonitor = new ProcessMonitor();
            _processMonitor.GameStateUpdated += OnGameStateUpdated;
            _processMonitor.StatusChanged += OnStatusChanged;
            _processMonitor.ProcessNameChanged += OnProcessNameChanged;

            _uiTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _uiTimer.Tick += (s, e) => UpdateUI();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_processMonitor != null)
            {
                _processMonitor.TryAttachToProcess();
                _processMonitor.StartPolling(16);
                _uiTimer?.Start();
                UpdateUI();
            }
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _uiTimer?.Stop();
            _processMonitor?.Dispose();
        }

        private void OnGameStateUpdated(object? sender, GameState gameState)
        {
        }

        private void OnStatusChanged(object? sender, string status)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = status;
                if (status.Contains("Auto-detected YGO FM") || status.Contains("Attached"))
                {
                    _isAttached = true;
                    AttachDetachButton.Content = "Detach";
                    AttachDetachButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                }
                else if (status.Contains("DuckStation not found"))
                {
                    _isAttached = false;
                    AttachDetachButton.Content = "Attach";
                    AttachDetachButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                }
            });
        }

        private void OnProcessNameChanged(object? sender, string processName)
        {
        }

        private void UpdateRankVisual(GameState gameState)
        {
            UpdateStatTier(ScoreTierRegistry.CardsUsed, gameState.CardsUsed,
                CardsUsedCell1, CardsUsedRange1, CardsUsedScore1,
                CardsUsedCell2, CardsUsedRange2, CardsUsedScore2,
                CardsUsedCell3, CardsUsedRange3, CardsUsedScore3,
                CardsUsedCell4, CardsUsedRange4, CardsUsedScore4,
                CardsUsedCell5, CardsUsedRange5, CardsUsedScore5);

            UpdateStatTier(ScoreTierRegistry.Turns, gameState.StatValues[1],
                TurnsCell1, TurnsRange1, TurnsScore1,
                TurnsCell2, TurnsRange2, TurnsScore2,
                TurnsCell3, TurnsRange3, TurnsScore3,
                TurnsCell4, TurnsRange4, TurnsScore4,
                TurnsCell5, TurnsRange5, TurnsScore5);

            UpdateStatTier(ScoreTierRegistry.EffectiveAttacks, gameState.StatValues[2],
                AtkCell1, AtkRange1, AtkScore1,
                AtkCell2, AtkRange2, AtkScore2,
                AtkCell3, AtkRange3, AtkScore3,
                AtkCell4, AtkRange4, AtkScore4,
                AtkCell5, AtkRange5, AtkScore5);

            UpdateStatTier(ScoreTierRegistry.DefensiveWins, gameState.StatValues[3],
                DefWinsCell1, DefWinsRange1, DefWinsScore1,
                DefWinsCell2, DefWinsRange2, DefWinsScore2,
                DefWinsCell3, DefWinsRange3, DefWinsScore3,
                DefWinsCell4, DefWinsRange4, DefWinsScore4,
                DefWinsCell5, DefWinsRange5, DefWinsScore5);

            UpdateStatTier(ScoreTierRegistry.FaceDowns, gameState.StatValues[4],
                FaceDownCell1, FaceDownRange1, FaceDownScore1,
                FaceDownCell2, FaceDownRange2, FaceDownScore2,
                FaceDownCell3, FaceDownRange3, FaceDownScore3,
                FaceDownCell4, FaceDownRange4, FaceDownScore4,
                FaceDownCell5, FaceDownRange5, FaceDownScore5);

            UpdateStatTier(ScoreTierRegistry.Fusions, gameState.StatValues[5],
                FusionCell1, FusionRange1, FusionScore1,
                FusionCell2, FusionRange2, FusionScore2,
                FusionCell3, FusionRange3, FusionScore3,
                FusionCell4, FusionRange4, FusionScore4,
                FusionCell5, FusionRange5, FusionScore5);

            UpdateStatTier(ScoreTierRegistry.EquipMagic, gameState.StatValues[6],
                EquipCell1, EquipRange1, EquipScore1,
                EquipCell2, EquipRange2, EquipScore2,
                EquipCell3, EquipRange3, EquipScore3,
                EquipCell4, EquipRange4, EquipScore4,
                EquipCell5, EquipRange5, EquipScore5);

            UpdateStatTier(ScoreTierRegistry.PureMagic, gameState.StatValues[7],
                MagicCell1, MagicRange1, MagicScore1,
                MagicCell2, MagicRange2, MagicScore2,
                MagicCell3, MagicRange3, MagicScore3,
                MagicCell4, MagicRange4, MagicScore4,
                MagicCell5, MagicRange5, MagicScore5);

            UpdateStatTier(ScoreTierRegistry.TrapsTriggered, gameState.StatValues[8],
                TrapCell1, TrapRange1, TrapScore1,
                TrapCell2, TrapRange2, TrapScore2,
                TrapCell3, TrapRange3, TrapScore3,
                TrapCell4, TrapRange4, TrapScore4,
                TrapCell5, TrapRange5, TrapScore5);

            UpdateStatTier(ScoreTierRegistry.LifePoints, gameState.DuelLifePoints,
                LPCell1, LPRange1, LPScore1,
                LPCell2, LPRange2, LPScore2,
                LPCell3, LPRange3, LPScore3,
                LPCell4, LPRange4, LPScore4,
                LPCell5, LPRange5, LPScore5);
        }

        private void UpdateStatTier(
            System.Collections.Generic.IReadOnlyList<ScoreTier> tiers, int value,
            Border cell1, TextBlock range1, TextBlock score1,
            Border cell2, TextBlock range2, TextBlock score2,
            Border cell3, TextBlock range3, TextBlock score3,
            Border cell4, TextBlock range4, TextBlock score4,
            Border cell5, TextBlock range5, TextBlock score5)
        {
            var cells = new[] { (cell1, range1, score1), (cell2, range2, score2), (cell3, range3, score3), (cell4, range4, score4), (cell5, range5, score5) };
            
            for (int i = 0; i < tiers.Count && i < cells.Length; i++)
            {
                var (cell, rangeText, scoreText) = cells[i];
                var tier = tiers[i];
                bool isActive = tier.Contains(value);
                
                if (isActive)
                {
                    cell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a5a"));
                    rangeText.Text = value.ToString();
                    rangeText.Foreground = new SolidColorBrush(Colors.White);
                    rangeText.FontWeight = FontWeights.Bold;
                    scoreText.Text = FormatScore(tier.Score);
                    scoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
                    scoreText.FontWeight = FontWeights.Bold;
                }
                else
                {
                    rangeText.Text = tier.GetRangeDisplay();
                    rangeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
                    rangeText.FontWeight = FontWeights.Normal;
                    scoreText.Text = FormatScore(tier.Score);
                    scoreText.Foreground = tier.Score >= 0 
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00")) 
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6666"));
                    scoreText.FontWeight = FontWeights.Normal;
                    cell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a3a"));
                }
            }
        }

        private void UpdateUI()
        {
            var gameState = _processMonitor?.CurrentGameState;
            if (gameState == null)
            {
                return;
            }

            GameIdText.Text = gameState.IsProcessAttached ? gameState.GameIdText : "[Not attached]";
            P1LifePointsText.Text = gameState.P1LifePoints.ToString();
            P2LifePointsText.Text = gameState.P2LifePoints.ToString();
            
            DuelScoreText.Text = $"{gameState.TotalScore} = {gameState.ScoreRank}";
            StarchipsText.Text = gameState.Starchips.ToString();
            
            UpdateRankVisual(gameState);
            
            if (App.DebugMode)
            {
                UpdateDebugValues(gameState);
            }
        }

        private void UpdateDebugValues(DataModel.GameState gameState)
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(gameState.MemoryScanLog))
            {
                sb.AppendLine(gameState.MemoryScanLog);
                sb.AppendLine();
            }
            
            sb.AppendLine("=== PROCESS ===");
            sb.AppendLine($"Attached: {gameState.IsProcessAttached}");
            sb.AppendLine($"Process: {gameState.ProcessName}");
            sb.AppendLine($"PID: {gameState.ProcessId}");
            sb.AppendLine($"RAM Base: 0x{gameState.RamBaseAddress:X8}");
            sb.AppendLine($"Verified: {gameState.GameVerified}");
            sb.AppendLine();
            
            sb.AppendLine("=== GAME ID (0x00009244-0x00009250) ===");
            sb.AppendLine(gameState.GameIdText);
            sb.AppendLine();
            
            sb.AppendLine("=== LIFE POINTS ===");
            sb.AppendLine($"P1 LP: {gameState.P1LifePoints}");
            sb.AppendLine($"P2 LP: {gameState.P2LifePoints}");
            sb.AppendLine();
            
            sb.AppendLine($"=== DUEL SCORE: {gameState.TotalScore} = {gameState.ScoreRank} ===");
            sb.AppendLine($"(Base 50 + modifiers, victory ranks shown below)");
            sb.AppendLine();
            sb.AppendLine("Stats breakdown:");
            sb.AppendLine($"  TRN: {gameState.StatTurns} -> {gameState.ScoreContributions[1]}");
            sb.AppendLine($"  ATK: {gameState.StatEffectiveAttacks} -> {gameState.ScoreContributions[2]}");
            sb.AppendLine($"  DFW: {gameState.StatDefensiveWins} -> {gameState.ScoreContributions[3]}");
            sb.AppendLine($"  FCD: {gameState.StatFaceDowns} -> {gameState.ScoreContributions[4]}");
            sb.AppendLine($"  FUS: {gameState.StatFusions} -> {gameState.ScoreContributions[5]}");
            sb.AppendLine($"  EQP: {gameState.StatEquipMagic} -> {gameState.ScoreContributions[6]}");
            sb.AppendLine($"  MAG: {gameState.StatPureMagic} -> {gameState.ScoreContributions[7]}");
            sb.AppendLine($"  TRP: {gameState.StatTrapsTriggered} -> {gameState.ScoreContributions[8]}");
            sb.AppendLine($"  CMB: {gameState.StatComboPlays} -> {gameState.ScoreContributions[9]}");
            sb.AppendLine($"  LP: {gameState.DuelLifePoints} -> {gameState.ScoreContributions[10]}");
            sb.AppendLine();
            sb.AppendLine("Victory ranks (if applicable at duel end):");
            sb.AppendLine($"  Exodia Win (+40): {gameState.ScoreRankExodia}");
            sb.AppendLine($"  Total Annihilation (+2): {gameState.ScoreRankTotalAnnihilation}");
            sb.AppendLine($"  Attrition (-40): {gameState.ScoreRankAttrition}");
            sb.AppendLine();
            sb.AppendLine("Rank scale: S POW:99-90 | A POW:89-80 | B POW:79-70 | C POW:69-60 | D POW:59-50");
            sb.AppendLine("            D TEC:49-40 | C TEC:39-30 | B TEC:29-20 | A TEC:19-10 | S TEC:09-00");

            DebugLogText.Text = sb.ToString();
        }

        private bool _isAttached = false;

        private void AttachDetachButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAttached)
            {
                _processMonitor?.Detach();
                ClearUI();
                _isAttached = false;
                AttachDetachButton.Content = "Attach";
                AttachDetachButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            }
            else
            {
                _processMonitor?.TryAttachToProcess();
                _uiTimer?.Start();
                _isAttached = _processMonitor?.IsAttached ?? false;
                if (_isAttached)
                {
                    AttachDetachButton.Content = "Detach";
                    AttachDetachButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                }
            }
        }

        private void ClearUI()
        {
            Dispatcher.Invoke(() =>
            {
                GameIdText.Text = "[Not attached]";
                P1LifePointsText.Text = "--";
                P2LifePointsText.Text = "--";
                DuelScoreText.Text = "-- = --";
                _isAttached = false;
                AttachDetachButton.Content = "Attach";
                AttachDetachButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            });
        }
        
        private static string FormatScore(int score)
        {
            return score >= 0 ? $"+{score}" : $"{score}";
        }
    }
}