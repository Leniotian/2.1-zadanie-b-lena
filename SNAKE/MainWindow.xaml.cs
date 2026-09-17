using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SNAKE.Engine;
using SNAKE.Models;

namespace SNAKE;

public partial class MainWindow : Window
{
    private readonly GameEngine _engine = new();
    private readonly Stopwatch _gameStopwatch = new();
    private double _tickAccumulatorMs = 0;

    private SnakeSkin _selectedSkin = SnakeSkin.AllSkins[0];

    public MainWindow()
    {
        InitializeComponent();

        BoardCanvas.Engine = _engine;
        SkinPreview.Skin = _selectedSkin;
        TxtSkinDescription.Text = _selectedSkin.Description;

        // Initialize Skin buttons
        BuildSkinButtons();

        // Wire up engine events
        _engine.StateChanged += Engine_StateChanged;
        _engine.BoardChanged += Engine_BoardChanged;
        _engine.FruitEaten += Engine_FruitEaten;
        _engine.GameOverEvent += Engine_GameOver;
        _engine.VictoryEvent += Engine_Victory;

        // Hook VSync CompositionTarget rendering loop for buttery-smooth 60+ FPS gliding
        CompositionTarget.Rendering += OnCompositionRendering;
        _gameStopwatch.Start();

        // Update initial stats and records
        RefreshStatsDisplay();
        UpdateUI();
    }

    private void BuildSkinButtons()
    {
        PanelSkinsList.Children.Clear();
        foreach (var skin in SnakeSkin.AllSkins)
        {
            var rb = new RadioButton
            {
                Content = skin.Name,
                GroupName = "SkinGroup",
                Style = (Style)FindResource("ForestPillRadioStyle"),
                Tag = skin,
                IsChecked = skin.Id == _selectedSkin.Id
            };

            rb.Checked += (s, e) =>
            {
                if (s is RadioButton btn && btn.Tag is SnakeSkin chosen)
                {
                    _selectedSkin = chosen;
                    _engine.SelectedSkin = chosen;
                    SkinPreview.Skin = chosen;
                    TxtSkinDescription.Text = chosen.Description;
                    SoundManager.Instance.PlayClick();
                    BoardCanvas.RequestRedraw();
                }
            };

            PanelSkinsList.Children.Add(rb);
        }
    }

    private void OnCompositionRendering(object? sender, EventArgs e)
    {
        double elapsed = _gameStopwatch.Elapsed.TotalMilliseconds;
        _gameStopwatch.Restart();

        // Guard against huge spikes (e.g. window dragged or minimized)
        if (elapsed > 200) elapsed = 200;

        if (_engine.State == GameState.Running)
        {
            _tickAccumulatorMs += elapsed;
            int interval = _engine.GetCurrentTickIntervalMs();

            while (_tickAccumulatorMs >= interval)
            {
                _engine.Tick();
                _tickAccumulatorMs -= interval;
                interval = _engine.GetCurrentTickIntervalMs();

                if (_engine.State != GameState.Running)
                {
                    _tickAccumulatorMs = 0;
                    break;
                }
            }

            if (_engine.State == GameState.Running)
            {
                _engine.InterpolationProgress = Math.Clamp(_tickAccumulatorMs / interval, 0.0, 1.0);
            }
            else
            {
                _engine.InterpolationProgress = 1.0;
            }
        }
        else
        {
            _engine.InterpolationProgress = 1.0;
            _tickAccumulatorMs = 0;
        }

        FireflyLayer.Advance();
        SkinPreview.AdvanceAnimation();
        BoardCanvas.RequestRedraw();
    }

    private void Engine_BoardChanged()
    {
        UpdateUI();
    }

    private void Engine_StateChanged()
    {
        UpdateUI();
    }

    private void Engine_FruitEaten(Fruit fruit, int points, int combo)
    {
        UpdateUI();
    }

    private void Engine_GameOver()
    {
        TxtGameOverReason.Text = _engine.DeathReason;
        TxtGameOverScore.Text = _engine.Score.ToString();
        TxtGameOverApples.Text = _engine.FruitsCollected.ToString();

        bool isRecord = _engine.Score > 0 && _engine.Score >= ScoreManager.Instance.Stats.HighScore;
        BannerNewRecord.Visibility = isRecord ? Visibility.Visible : Visibility.Collapsed;

        RefreshStatsDisplay();
        UpdateUI();
    }

    private void Engine_Victory()
    {
        TxtVictoryScore.Text = _engine.Score.ToString();
        TxtVictoryApples.Text = _engine.FruitsCollected.ToString();

        RefreshStatsDisplay();
        UpdateUI();
    }

    private void UpdateUI()
    {
        TxtCurrentScore.Text = _engine.Score.ToString();
        TxtFruitsProgress.Text = $"{_engine.FruitsCollected} / {_engine.MaxFruitsPossible}";
        TxtHighScore.Text = ScoreManager.Instance.Stats.HighScore.ToString();

        // Mode summary
        string sizeName = _engine.SelectedBoardSize switch
        {
            BoardSize.Small => "Mała 10x10",
            BoardSize.Normal => "Klasyczna 16x16",
            BoardSize.Large => "Przestronna 22x22",
            _ => "16x16"
        };

        string speedName = _engine.SelectedSpeed switch
        {
            GameSpeed.Sloth => "Leniwiec",
            GameSpeed.Deer => "Jeleń",
            GameSpeed.Falcon => "Sokół",
            GameSpeed.Viper => "Żmija",
            GameSpeed.Dynamic => "Dynamiczna",
            _ => "Normalna"
        };

        string wallName = _engine.SelectedWallMode == WallMode.Solid ? "Ściany" : "Portal";
        TxtModeSummary.Text = $"{sizeName} • {speedName} • {wallName}";

        // Combo badge
        if (_engine.ComboStreak > 1 && _engine.State == GameState.Running)
        {
            TxtCombo.Text = $"COMBO x{(_engine.ComboStreak >= 4 ? 3 : 2)}!";
            BadgeCombo.Visibility = Visibility.Visible;
        }
        else
        {
            BadgeCombo.Visibility = Visibility.Collapsed;
        }

        // Overlays
        OverlayMenu.Visibility = _engine.State == GameState.Menu ? Visibility.Visible : Visibility.Collapsed;
        OverlayPause.Visibility = _engine.State == GameState.Paused ? Visibility.Visible : Visibility.Collapsed;
        OverlayGameOver.Visibility = _engine.State == GameState.GameOver ? Visibility.Visible : Visibility.Collapsed;
        OverlayVictory.Visibility = _engine.State == GameState.Victory ? Visibility.Visible : Visibility.Collapsed;

        BannerReadyHint.Visibility = _engine.State == GameState.Ready ? Visibility.Visible : Visibility.Collapsed;

        // Button state
        TxtPauseIcon.Text = _engine.State == GameState.Paused ? "▶ Wznów" : "⏸ Pauza";
        TxtMuteIcon.Text = SoundManager.Instance.IsMuted ? "🔇 Wyciszony" : "🔊 Dźwięk";
    }

    private void RefreshStatsDisplay()
    {
        var stats = ScoreManager.Instance.Stats;
        TxtStatsHighScore.Text = stats.HighScore.ToString();
        TxtStatsTotalApples.Text = stats.TotalApplesEaten.ToString();
        TxtStatsGamesPlayed.Text = stats.TotalGamesPlayed.ToString();
        TxtStatsVictories.Text = stats.VictoriesCount.ToString();
        TxtHighScore.Text = stats.HighScore.ToString();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (_engine.State == GameState.Menu)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                StartGameFromMenu();
                e.Handled = true;
            }
            return;
        }

        switch (e.Key)
        {
            case Key.Up:
            case Key.W:
                HandleDirectionKey(Direction.Up);
                e.Handled = true;
                break;

            case Key.Down:
            case Key.S:
                HandleDirectionKey(Direction.Down);
                e.Handled = true;
                break;

            case Key.Left:
            case Key.A:
                HandleDirectionKey(Direction.Left);
                e.Handled = true;
                break;

            case Key.Right:
            case Key.D:
                HandleDirectionKey(Direction.Right);
                e.Handled = true;
                break;

            case Key.Space:
                TogglePause();
                e.Handled = true;
                break;

            case Key.R:
                RestartGame();
                e.Handled = true;
                break;

            case Key.M:
                ToggleMute();
                e.Handled = true;
                break;

            case Key.Escape:
                ToggleMenu();
                e.Handled = true;
                break;
        }
    }

    private void HandleDirectionKey(Direction dir)
    {
        if (_engine.State == GameState.Ready)
        {
            _engine.QueueDirection(dir);
            _engine.StartGame();
            _tickAccumulatorMs = 0;
            _gameStopwatch.Restart();
            SoundManager.Instance.PlayTurn();
        }
        else if (_engine.State == GameState.Running)
        {
            _engine.QueueDirection(dir);
            SoundManager.Instance.PlayTurn();
        }
    }

    private void StartGameFromMenu()
    {
        ReadMenuSettings(out var size, out var speed, out var wall, out var special);
        _engine.ApplySettings(size, speed, wall, special, _selectedSkin);

        _tickAccumulatorMs = 0;
        _gameStopwatch.Restart();
        UpdateUI();
        SoundManager.Instance.PlayClick();
    }

    private void ReadMenuSettings(out BoardSize size, out GameSpeed speed, out WallMode wall, out SpecialMode special)
    {
        size = BoardSize.Normal;
        if (RadSizeSmall.IsChecked == true) size = BoardSize.Small;
        else if (RadSizeLarge.IsChecked == true) size = BoardSize.Large;

        speed = GameSpeed.Deer;
        if (RadSpeedSloth.IsChecked == true) speed = GameSpeed.Sloth;
        else if (RadSpeedFalcon.IsChecked == true) speed = GameSpeed.Falcon;
        else if (RadSpeedViper.IsChecked == true) speed = GameSpeed.Viper;
        else if (RadSpeedDynamic.IsChecked == true) speed = GameSpeed.Dynamic;

        wall = RadWallSolid.IsChecked == true ? WallMode.Solid : WallMode.Portal;

        special = SpecialMode.Classic;
        if (RadSpecialTwin.IsChecked == true) special = SpecialMode.TwinApples;
        else if (RadSpecialGolden.IsChecked == true) special = SpecialMode.GoldenAcorn;
        else if (RadSpecialObstacles.IsChecked == true) special = SpecialMode.ForestObstacles;
        else if (RadSpecialPeaceful.IsChecked == true) special = SpecialMode.Peaceful;
    }

    private void TogglePause()
    {
        if (_engine.State == GameState.Running)
        {
            _engine.PauseGame();
        }
        else if (_engine.State == GameState.Paused)
        {
            _engine.PauseGame();
            _gameStopwatch.Restart();
        }
        else if (_engine.State == GameState.Ready)
        {
            _engine.StartGame();
            _tickAccumulatorMs = 0;
            _gameStopwatch.Restart();
        }
    }

    private void RestartGame()
    {
        _tickAccumulatorMs = 0;
        _engine.InitializeGame();
        _gameStopwatch.Restart();
        UpdateUI();
        SoundManager.Instance.PlayClick();
    }

    private void ToggleMute()
    {
        SoundManager.Instance.IsMuted = !SoundManager.Instance.IsMuted;
        UpdateUI();
    }

    private void ToggleMenu()
    {
        if (_engine.State == GameState.Menu)
        {
            StartGameFromMenu();
        }
        else
        {
            _engine.OpenMenu();
            _tickAccumulatorMs = 0;
            RefreshStatsDisplay();
            UpdateUI();
        }
    }

    // UI Click Handlers
    private void BtnStartGame_Click(object sender, RoutedEventArgs e) => StartGameFromMenu();
    private void BtnResume_Click(object sender, RoutedEventArgs e) => TogglePause();
    private void BtnPause_Click(object sender, RoutedEventArgs e) => TogglePause();
    private void BtnRestart_Click(object sender, RoutedEventArgs e) => RestartGame();
    private void BtnMute_Click(object sender, RoutedEventArgs e) => ToggleMute();
    private void BtnMenu_Click(object sender, RoutedEventArgs e) => ToggleMenu();

    private void ModeSelection_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        SoundManager.Instance.PlayClick();
    }
}