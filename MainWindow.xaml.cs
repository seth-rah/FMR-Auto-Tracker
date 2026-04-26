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
        private const int DefaultUiUpdateIntervalMs = 16;
        private const string AttachButtonText = "Attach";
        private const string DetachButtonText = "Detach";

        private const string StatusAutoDetected = "Auto-detected YGO FM";
        private const string StatusAttached = "Attached";
        private const string StatusDetached = "Detached";
        private const string StatusNotFound = "not found";
        private const string StatusNoEmulator = "No emulator";

        private const string SelectEmulatorPrompt = "Select an emulator to attach to";
        private const string NotAttachedText = "[Not attached]";
        private const string GameNotFoundText = "Game not found";
        private const string DefaultScoreText = "-- = --";
        private const string DefaultLpText = "--";
        private const string DefaultStarchipsText = "--";

        private static readonly SolidColorBrush GreenButtonBrush = new((Color)ColorConverter.ConvertFromString("#4CAF50"));
        private static readonly SolidColorBrush RedButtonBrush = new((Color)ColorConverter.ConvertFromString("#F44336"));
        private static readonly SolidColorBrush DefaultCellBackground = new((Color)ColorConverter.ConvertFromString("#2a2a3a"));
        private static readonly SolidColorBrush ActiveCellBackground = new((Color)ColorConverter.ConvertFromString("#4a4a5a"));
        private static readonly SolidColorBrush DefaultForeground = new((Color)ColorConverter.ConvertFromString("#666666"));
        private static readonly SolidColorBrush ScorePositiveForeground = new((Color)ColorConverter.ConvertFromString("#00FF00"));
        private static readonly SolidColorBrush ScoreNegativeForeground = new((Color)ColorConverter.ConvertFromString("#FF6666"));

        private ProcessMonitor? _processMonitor;
        private System.Windows.Threading.DispatcherTimer? _uiTimer;
        private bool _isAttached = false;

        public MainWindow()
        {
            InitializeComponent();

            if (App.DebugMode)
            {
                DebugExpander.Visibility = Visibility.Visible;
            }

            _processMonitor = new ProcessMonitor();
            _processMonitor.StatusChanged += OnStatusChanged;

            _uiTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DefaultUiUpdateIntervalMs)
            };
            _uiTimer.Tick += (s, e) => UpdateUI();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            EmulatorSelectionPanel.Visibility = Visibility.Visible;
            AttachDetachButton.Visibility = Visibility.Collapsed;
        }

        private void SelectDuckStation_Click(object sender, RoutedEventArgs e)
        {
            EmulatorSelectionPanel.Visibility = Visibility.Collapsed;
            AttachDetachButton.Visibility = Visibility.Visible;
            AttachDetachButton.Content = AttachButtonText;
            AttachDetachButton.Background = GreenButtonBrush;

            _processMonitor?.SetPreferredEmulator("DuckStation");
            _processMonitor?.TryAttachToProcess();
            _uiTimer?.Start();
            _isAttached = _processMonitor?.IsAttached ?? false;
            if (_isAttached)
            {
                AttachDetachButton.Content = DetachButtonText;
                AttachDetachButton.Background = RedButtonBrush;
            }
        }

        private void SelectBizhawk_Click(object sender, RoutedEventArgs e)
        {
            EmulatorSelectionPanel.Visibility = Visibility.Collapsed;
            AttachDetachButton.Visibility = Visibility.Visible;
            AttachDetachButton.Content = AttachButtonText;
            AttachDetachButton.Background = GreenButtonBrush;

            _processMonitor?.SetPreferredEmulator("Bizhawk");
            _processMonitor?.TryAttachToProcess();
            _uiTimer?.Start();
            _isAttached = _processMonitor?.IsAttached ?? false;
            if (_isAttached)
            {
                AttachDetachButton.Content = DetachButtonText;
                AttachDetachButton.Background = RedButtonBrush;
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            StatusText.Text = SelectEmulatorPrompt;
            ResetTableCells();
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _uiTimer?.Stop();
            _processMonitor?.Dispose();
        }

        private void OnStatusChanged(object? sender, string status)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = status;
                if (status.Contains(StatusAutoDetected) || status.Contains(StatusAttached))
                {
                    _isAttached = true;
                    AttachDetachButton.Content = DetachButtonText;
                    AttachDetachButton.Background = RedButtonBrush;
                }
                else if (status.Contains(StatusDetached))
                {
                    _isAttached = false;
                    AttachDetachButton.Content = AttachButtonText;
                    AttachDetachButton.Background = GreenButtonBrush;
                }
                else if (status.Contains(StatusNotFound) || status.Contains(StatusNoEmulator))
                {
                    _isAttached = false;
                    AttachDetachButton.Content = DetachButtonText;
                    AttachDetachButton.Background = RedButtonBrush;
                }
            });
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
                    cell.Background = ActiveCellBackground;
                    rangeText.Text = value.ToString();
                    rangeText.Foreground = new SolidColorBrush(Colors.White);
                    rangeText.FontWeight = FontWeights.Bold;
                    scoreText.Text = FormatScore(tier.Score);
                    scoreText.Foreground = ScorePositiveForeground;
                    scoreText.FontWeight = FontWeights.Bold;
                }
                else
                {
                    rangeText.Text = tier.GetRangeDisplay();
                    rangeText.Foreground = DefaultForeground;
                    rangeText.FontWeight = FontWeights.Normal;
                    scoreText.Text = FormatScore(tier.Score);
                    scoreText.Foreground = tier.Score >= 0 
                        ? ScorePositiveForeground 
                        : ScoreNegativeForeground;
                    scoreText.FontWeight = FontWeights.Normal;
                    cell.Background = DefaultCellBackground;
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

            if (!gameState.GameVerified)
            {
                GameIdText.Text = GameNotFoundText;
                P1LifePointsText.Text = DefaultLpText;
                P2LifePointsText.Text = DefaultLpText;
                DuelScoreText.Text = DefaultScoreText;
                StarchipsText.Text = DefaultStarchipsText;
                return;
            }

            GameIdText.Text = gameState.GameIdText;
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
            sb.AppendLine($"(Base 52 + modifiers, victory ranks shown below)");
            sb.AppendLine();
            sb.AppendLine("Stats breakdown:");
            sb.AppendLine($"  TRN: {gameState.StatTurns} -> {gameState.ScoreContributions[0]}");
            sb.AppendLine($"  ATK: {gameState.StatEffectiveAttacks} -> {gameState.ScoreContributions[2]}");
            sb.AppendLine($"  DFW: {gameState.StatDefensiveWins} -> {gameState.ScoreContributions[3]}");
            sb.AppendLine($"  FCD: {gameState.StatFaceDowns} -> {gameState.ScoreContributions[4]}");
            sb.AppendLine($"  FUS: {gameState.StatFusions} -> {gameState.ScoreContributions[5]}");
            sb.AppendLine($"  EQP: {gameState.StatEquipMagic} -> {gameState.ScoreContributions[6]}");
            sb.AppendLine($"  MAG: {gameState.StatPureMagic} -> {gameState.ScoreContributions[7]}");
            sb.AppendLine($"  TRP: {gameState.StatTrapsTriggered} -> {gameState.ScoreContributions[8]}");
            sb.AppendLine($"  CRD: {gameState.CardsUsed} -> {gameState.ScoreContributions[8]}");
            sb.AppendLine($"  LP: {gameState.DuelLifePoints} -> {gameState.ScoreContributions[9]}");
            sb.AppendLine();
            sb.AppendLine("Rank scale: S POW:99-90 | A POW:89-80 | B POW:79-70 | C POW:69-60 | D POW:59-50");
            sb.AppendLine("            D TEC:49-40 | C TEC:39-30 | B TEC:29-20 | A TEC:19-10 | S TEC:09-00");

            DebugLogText.Text = sb.ToString();
        }

        private void AttachDetachButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttachDetachButton.Content.ToString() == DetachButtonText)
            {
                _processMonitor?.Detach();
                ClearUI();
                _isAttached = false;
            }
            else
            {
                _processMonitor?.TryAttachToProcess();
                _uiTimer?.Start();
                _isAttached = _processMonitor?.IsAttached ?? false;
                if (_isAttached)
                {
                    AttachDetachButton.Content = DetachButtonText;
                    AttachDetachButton.Background = RedButtonBrush;
                }
            }
        }

        private void ClearUI()
        {
            Dispatcher.Invoke(() =>
            {
                GameIdText.Text = NotAttachedText;
                P1LifePointsText.Text = DefaultLpText;
                P2LifePointsText.Text = DefaultLpText;
                DuelScoreText.Text = DefaultScoreText;
                StarchipsText.Text = DefaultStarchipsText;
                ResetTableCells();
                _isAttached = false;
                AttachDetachButton.Content = AttachButtonText;
                AttachDetachButton.Background = GreenButtonBrush;
                AttachDetachButton.Visibility = Visibility.Collapsed;
                EmulatorSelectionPanel.Visibility = Visibility.Visible;
                StatusText.Text = SelectEmulatorPrompt;
            });
        }

        private void ResetTableCells()
        {
            var defaultBg = DefaultCellBackground;
            var defaultFg = DefaultForeground;

            void ResetCell(Border cell, TextBlock range, TextBlock score, string rangeText, string scoreText)
            {
                cell.Background = defaultBg;
                range.Text = rangeText;
                range.Foreground = defaultFg;
                range.FontWeight = FontWeights.Normal;
                score.Text = scoreText;
                score.Foreground = defaultFg;
                score.FontWeight = FontWeights.Normal;
            }

            ResetCell(CardsUsedCell1, CardsUsedRange1, CardsUsedScore1, "0-8", "+15");
            ResetCell(CardsUsedCell2, CardsUsedRange2, CardsUsedScore2, "9-12", "+12");
            ResetCell(CardsUsedCell3, CardsUsedRange3, CardsUsedScore3, "13-32", "+0");
            ResetCell(CardsUsedCell4, CardsUsedRange4, CardsUsedScore4, "33-36", "-5");
            ResetCell(CardsUsedCell5, CardsUsedRange5, CardsUsedScore5, "37+", "-7");

            ResetCell(TurnsCell1, TurnsRange1, TurnsScore1, "0-4", "+12");
            ResetCell(TurnsCell2, TurnsRange2, TurnsScore2, "5-8", "+8");
            ResetCell(TurnsCell3, TurnsRange3, TurnsScore3, "9-28", "+0");
            ResetCell(TurnsCell4, TurnsRange4, TurnsScore4, "29-32", "-8");
            ResetCell(TurnsCell5, TurnsRange5, TurnsScore5, "33+", "-12");

            ResetCell(AtkCell1, AtkRange1, AtkScore1, "0-1", "+4");
            ResetCell(AtkCell2, AtkRange2, AtkScore2, "2-3", "+2");
            ResetCell(AtkCell3, AtkRange3, AtkScore3, "4-9", "+0");
            ResetCell(AtkCell4, AtkRange4, AtkScore4, "10-19", "-2");
            ResetCell(AtkCell5, AtkRange5, AtkScore5, "20+", "-4");

            ResetCell(DefWinsCell1, DefWinsRange1, DefWinsScore1, "0-1", "+0");
            ResetCell(DefWinsCell2, DefWinsRange2, DefWinsScore2, "2-5", "-10");
            ResetCell(DefWinsCell3, DefWinsRange3, DefWinsScore3, "6-9", "-20");
            ResetCell(DefWinsCell4, DefWinsRange4, DefWinsScore4, "10-14", "-30");
            ResetCell(DefWinsCell5, DefWinsRange5, DefWinsScore5, "15+", "-40");

            ResetCell(FaceDownCell1, FaceDownRange1, FaceDownScore1, "0", "+0");
            ResetCell(FaceDownCell2, FaceDownRange2, FaceDownScore2, "1-10", "-2");
            ResetCell(FaceDownCell3, FaceDownRange3, FaceDownScore3, "11-20", "-4");
            ResetCell(FaceDownCell4, FaceDownRange4, FaceDownScore4, "21-30", "-6");
            ResetCell(FaceDownCell5, FaceDownRange5, FaceDownScore5, "31+", "-8");

            ResetCell(FusionCell1, FusionRange1, FusionScore1, "0", "+4");
            ResetCell(FusionCell2, FusionRange2, FusionScore2, "1-4", "+0");
            ResetCell(FusionCell3, FusionRange3, FusionScore3, "5-9", "-4");
            ResetCell(FusionCell4, FusionRange4, FusionScore4, "10-14", "-8");
            ResetCell(FusionCell5, FusionRange5, FusionScore5, "15+", "-12");

            ResetCell(EquipCell1, EquipRange1, EquipScore1, "0", "+4");
            ResetCell(EquipCell2, EquipRange2, EquipScore2, "1-4", "+0");
            ResetCell(EquipCell3, EquipRange3, EquipScore3, "5-9", "-4");
            ResetCell(EquipCell4, EquipRange4, EquipScore4, "10-14", "-8");
            ResetCell(EquipCell5, EquipRange5, EquipScore5, "15+", "-12");

            ResetCell(MagicCell1, MagicRange1, MagicScore1, "0", "+2");
            ResetCell(MagicCell2, MagicRange2, MagicScore2, "1-3", "-4");
            ResetCell(MagicCell3, MagicRange3, MagicScore3, "4-6", "-8");
            ResetCell(MagicCell4, MagicRange4, MagicScore4, "7-9", "-12");
            ResetCell(MagicCell5, MagicRange5, MagicScore5, "10+", "-16");

            ResetCell(TrapCell1, TrapRange1, TrapScore1, "0", "+2");
            ResetCell(TrapCell2, TrapRange2, TrapScore2, "1-2", "-8");
            ResetCell(TrapCell3, TrapRange3, TrapScore3, "3-4", "-16");
            ResetCell(TrapCell4, TrapRange4, TrapScore4, "5-6", "-24");
            ResetCell(TrapCell5, TrapRange5, TrapScore5, "7+", "-32");

            ResetCell(LPCell1, LPRange1, LPScore1, "8000+", "+6");
            ResetCell(LPCell2, LPRange2, LPScore2, "7000-7999", "+4");
            ResetCell(LPCell3, LPRange3, LPScore3, "1000-6999", "+0");
            ResetCell(LPCell4, LPRange4, LPScore4, "100-999", "-5");
            ResetCell(LPCell5, LPRange5, LPScore5, "0-99", "-7");
        }
        
        private static string FormatScore(int score)
        {
            return score >= 0 ? $"+{score}" : $"{score}";
        }
    }
}