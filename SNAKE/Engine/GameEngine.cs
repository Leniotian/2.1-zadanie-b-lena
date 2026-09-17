using System.Diagnostics;
using SNAKE.Models;

namespace SNAKE.Engine;

public class GameEngine
{
    private readonly Random _random = new();
    private readonly Queue<Direction> _inputBuffer = new(2);

    public int GridWidth { get; private set; } = 16;
    public int GridHeight { get; private set; } = 16;

    public List<Position> SnakeBody { get; } = new();
    public Position Head => SnakeBody.Count > 0 ? SnakeBody[0] : new Position(0, 0);

    public List<Position> PreviousBody { get; private set; } = new();
    public double InterpolationProgress { get; set; } = 1.0;

    public Direction CurrentDirection { get; private set; } = Direction.Right;
    public GameState State { get; private set; } = GameState.Ready;

    public BoardSize SelectedBoardSize { get; set; } = BoardSize.Normal;
    public GameSpeed SelectedSpeed { get; set; } = GameSpeed.Deer;
    public WallMode SelectedWallMode { get; set; } = WallMode.Solid;
    public SpecialMode SelectedSpecialMode { get; set; } = SpecialMode.Classic;
    public SnakeSkin SelectedSkin { get; set; } = SnakeSkin.AllSkins[0];

    public List<Fruit> ActiveFruits { get; } = new();
    public List<Obstacle> Obstacles { get; } = new();
    public List<FloatingScore> FloatingScores { get; } = new();

    public int Score { get; private set; } = 0;
    public int FruitsCollected { get; private set; } = 0;
    public int MaxFruitsPossible => (GridWidth * GridHeight) - 3 - Obstacles.Count;

    public int ComboStreak { get; private set; } = 0;
    private readonly Stopwatch _comboStopwatch = new();
    private const double ComboTimeWindowSeconds = 5.0;

    private int _ticksSinceSpecialFruit = 0;
    public string DeathReason { get; private set; } = string.Empty;

    // Events
    public event Action? StateChanged;
    public event Action? BoardChanged;
    public event Action<Fruit, int, int>? FruitEaten; // fruit, points, combo
    public event Action? GameOverEvent;
    public event Action? VictoryEvent;

    public GameEngine()
    {
        InitializeGame();
        State = GameState.Menu;
    }

    public void OpenMenu()
    {
        State = GameState.Menu;
        StateChanged?.Invoke();
    }

    public void ApplySettings(BoardSize size, GameSpeed speed, WallMode wallMode, SpecialMode specialMode, SnakeSkin skin)
    {
        SelectedBoardSize = size;
        SelectedSpeed = speed;
        SelectedWallMode = wallMode;
        SelectedSpecialMode = specialMode;
        SelectedSkin = skin;

        (GridWidth, GridHeight) = size switch
        {
            BoardSize.Small => (10, 10),
            BoardSize.Normal => (16, 16),
            BoardSize.Large => (22, 22),
            _ => (16, 16)
        };

        InitializeGame();
    }

    public void InitializeGame()
    {
        (GridWidth, GridHeight) = SelectedBoardSize switch
        {
            BoardSize.Small => (10, 10),
            BoardSize.Normal => (16, 16),
            BoardSize.Large => (22, 22),
            _ => (16, 16)
        };

        State = GameState.Ready;
        DeathReason = string.Empty;
        Score = 0;
        FruitsCollected = 0;
        ComboStreak = 0;
        _comboStopwatch.Reset();
        _ticksSinceSpecialFruit = 0;

        _inputBuffer.Clear();
        CurrentDirection = Direction.Right;

        SnakeBody.Clear();
        int startX = GridWidth / 2;
        int startY = GridHeight / 2;

        // Start with length 3 facing right
        SnakeBody.Add(new Position(startX, startY));
        SnakeBody.Add(new Position(startX - 1, startY));
        SnakeBody.Add(new Position(startX - 2, startY));
        PreviousBody = SnakeBody.ToList();
        InterpolationProgress = 1.0;

        Obstacles.Clear();
        if (SelectedSpecialMode == SpecialMode.ForestObstacles)
        {
            GenerateObstacles();
        }

        ActiveFruits.Clear();
        FloatingScores.Clear();

        SpawnFruit(FruitType.RedApple);
        if (SelectedSpecialMode == SpecialMode.TwinApples)
        {
            SpawnFruit(FruitType.RedApple);
        }

        BoardChanged?.Invoke();
        StateChanged?.Invoke();
    }

    public void StartGame()
    {
        if (State == GameState.Ready || State == GameState.Paused)
        {
            State = GameState.Running;
            StateChanged?.Invoke();
        }
    }

    public void PauseGame()
    {
        if (State == GameState.Running)
        {
            State = GameState.Paused;
            StateChanged?.Invoke();
        }
        else if (State == GameState.Paused)
        {
            State = GameState.Running;
            StateChanged?.Invoke();
        }
    }

    public void QueueDirection(Direction newDir)
    {
        if (State == GameState.Ready)
        {
            StartGame();
        }

        if (State != GameState.Running) return;

        Direction lastQueued = _inputBuffer.Count > 0 ? _inputBuffer.Peek() : CurrentDirection;

        // Disallow opposite movement or duplicate
        if (!newDir.IsOpposite(lastQueued) && newDir != lastQueued)
        {
            if (_inputBuffer.Count < 2)
            {
                _inputBuffer.Enqueue(newDir);
            }
        }
    }

    public int GetCurrentTickIntervalMs()
    {
        return SelectedSpeed switch
        {
            GameSpeed.Sloth => 165,
            GameSpeed.Deer => 115,
            GameSpeed.Falcon => 75,
            GameSpeed.Viper => 48,
            GameSpeed.Dynamic => Math.Max(45, 130 - (FruitsCollected / 3) * 4),
            _ => 115
        };
    }

    public void Tick()
    {
        if (State != GameState.Running) return;

        PreviousBody = SnakeBody.ToList();

        // Process buffered direction
        if (_inputBuffer.Count > 0)
        {
            CurrentDirection = _inputBuffer.Dequeue();
        }

        // Calculate next head position
        var (dx, dy) = CurrentDirection.ToOffset();
        int nextX = Head.X + dx;
        int nextY = Head.Y + dy;

        // Handle walls
        if (nextX < 0 || nextX >= GridWidth || nextY < 0 || nextY >= GridHeight)
        {
            if (SelectedWallMode == WallMode.Portal || SelectedSpecialMode == SpecialMode.Peaceful)
            {
                // Wrap around edges
                if (nextX < 0) nextX = GridWidth - 1;
                else if (nextX >= GridWidth) nextX = 0;

                if (nextY < 0) nextY = GridHeight - 1;
                else if (nextY >= GridHeight) nextY = 0;
            }
            else
            {
                TriggerGameOver("Zderzenie ze ścianą lasu!");
                return;
            }
        }

        Position nextPos = new(nextX, nextY);

        // Check Obstacles
        if (Obstacles.Any(o => o.Position == nextPos))
        {
            TriggerGameOver("Uderzenie w omszały głaz!");
            return;
        }

        // Check Self Collision (ignoring tail tip since it will move, unless eating)
        bool willEat = ActiveFruits.Any(f => f.Position == nextPos);
        int checkLimit = willEat ? SnakeBody.Count : SnakeBody.Count - 1;

        if (SelectedSpecialMode != SpecialMode.Peaceful)
        {
            for (int i = 0; i < checkLimit; i++)
            {
                if (SnakeBody[i] == nextPos)
                {
                    TriggerGameOver("Wąż zaplątał się we własne łuski!");
                    return;
                }
            }
        }

        // Move Snake
        SnakeBody.Insert(0, nextPos);

        if (willEat)
        {
            Fruit eatenFruit = ActiveFruits.First(f => f.Position == nextPos);
            ActiveFruits.Remove(eatenFruit);

            HandleFruitEaten(eatenFruit);

            // Spawn replacement fruit
            SpawnFruit(FruitType.RedApple);

            // In TwinApples mode, maintain 2 fruits
            if (SelectedSpecialMode == SpecialMode.TwinApples && ActiveFruits.Count(f => f.Type == FruitType.RedApple) < 2)
            {
                SpawnFruit(FruitType.RedApple);
            }
        }
        else
        {
            // Remove tail if didn't eat
            SnakeBody.RemoveAt(SnakeBody.Count - 1);
        }

        // Handle timed bonus fruits (Golden Acorn)
        UpdateTimedFruits();

        // Update floating score notifications
        for (int i = FloatingScores.Count - 1; i >= 0; i--)
        {
            FloatingScores[i].Update();
            if (FloatingScores[i].IsDead)
            {
                FloatingScores.RemoveAt(i);
            }
        }

        // Check win condition
        if (FruitsCollected >= MaxFruitsPossible || SnakeBody.Count >= GridWidth * GridHeight)
        {
            TriggerVictory();
            return;
        }

        BoardChanged?.Invoke();
    }

    private void HandleFruitEaten(Fruit fruit)
    {
        FruitsCollected++;

        // Combo check
        if (_comboStopwatch.IsRunning && _comboStopwatch.Elapsed.TotalSeconds <= ComboTimeWindowSeconds)
        {
            ComboStreak++;
        }
        else
        {
            ComboStreak = 1;
        }
        _comboStopwatch.Restart();

        int multiplier = ComboStreak switch
        {
            >= 4 => 3,
            >= 2 => 2,
            _ => 1
        };

        int pointsEarned = fruit.Points * multiplier;
        Score += pointsEarned;

        // Floating score text
        string label = multiplier > 1 ? $"+{pointsEarned} (x{multiplier}!)" : $"+{pointsEarned}";
        FloatingScores.Add(new FloatingScore
        {
            X = fruit.Position.X,
            Y = fruit.Position.Y,
            Text = label,
            Color = fruit.Type switch
            {
                FruitType.GoldenAcorn => System.Windows.Media.Color.FromRgb(255, 215, 0),
                FruitType.BlueBerry => System.Windows.Media.Color.FromRgb(129, 212, 250),
                _ => System.Windows.Media.Color.FromRgb(255, 100, 100)
            }
        });

        FruitEaten?.Invoke(fruit, pointsEarned, multiplier);

        // Sound effect
        if (fruit.Type == FruitType.GoldenAcorn)
        {
            SoundManager.Instance.PlayEatBonus();
        }
        else
        {
            SoundManager.Instance.PlayEatApple();
        }
    }

    private void UpdateTimedFruits()
    {
        _ticksSinceSpecialFruit++;

        // In GoldenAcorn mode or occasionally in other modes, spawn a bonus Golden Acorn
        bool shouldSpawnAcorn = (SelectedSpecialMode == SpecialMode.GoldenAcorn && _ticksSinceSpecialFruit >= 35) ||
                                 (_ticksSinceSpecialFruit >= 70 && _random.Next(100) < 30);

        if (shouldSpawnAcorn && !ActiveFruits.Any(f => f.Type == FruitType.GoldenAcorn))
        {
            _ticksSinceSpecialFruit = 0;
            SpawnFruit(FruitType.GoldenAcorn, ticksDuration: 60);
        }

        // Decrement ticks on expiring fruits
        for (int i = ActiveFruits.Count - 1; i >= 0; i--)
        {
            var fruit = ActiveFruits[i];
            if (fruit.IsExpiring)
            {
                fruit.Tick();
                if (fruit.HasExpired)
                {
                    ActiveFruits.RemoveAt(i);
                }
            }
        }
    }

    private void SpawnFruit(FruitType type, int ticksDuration = -1)
    {
        var freePositions = GetFreePositions();
        if (freePositions.Count == 0) return;

        Position pos = freePositions[_random.Next(freePositions.Count)];
        ActiveFruits.Add(new Fruit(pos, type, ticksDuration));
    }

    private void GenerateObstacles()
    {
        int count = SelectedBoardSize switch
        {
            BoardSize.Small => 3,
            BoardSize.Normal => 7,
            BoardSize.Large => 14,
            _ => 6
        };

        var safeZone = new HashSet<Position>
        {
            Head,
            new(Head.X - 1, Head.Y),
            new(Head.X - 2, Head.Y),
            new(Head.X + 1, Head.Y),
            new(Head.X + 2, Head.Y),
            new(Head.X, Head.Y - 1),
            new(Head.X, Head.Y + 1)
        };

        var freePositions = GetFreePositions().Where(p => !safeZone.Contains(p)).ToList();

        for (int i = 0; i < count && freePositions.Count > 0; i++)
        {
            int idx = _random.Next(freePositions.Count);
            Position pos = freePositions[idx];
            freePositions.RemoveAt(idx);

            ObstacleType oType = _random.Next(2) == 0 ? ObstacleType.MossyRock : ObstacleType.TreeStump;
            Obstacles.Add(new Obstacle(pos, oType));
        }
    }

    private List<Position> GetFreePositions()
    {
        var occupied = new HashSet<Position>(SnakeBody);
        foreach (var f in ActiveFruits) occupied.Add(f.Position);
        foreach (var o in Obstacles) occupied.Add(o.Position);

        var list = new List<Position>();
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Position p = new(x, y);
                if (!occupied.Contains(p))
                {
                    list.Add(p);
                }
            }
        }

        return list;
    }

    private void TriggerGameOver(string reason)
    {
        State = GameState.GameOver;
        DeathReason = reason;
        SoundManager.Instance.PlayGameOver();

        string modeKey = $"{SelectedBoardSize}_{SelectedSpeed}_{SelectedWallMode}_{SelectedSpecialMode}";
        ScoreManager.Instance.RecordGameEnd(Score, FruitsCollected, modeKey, isVictory: false);

        StateChanged?.Invoke();
        GameOverEvent?.Invoke();
    }

    private void TriggerVictory()
    {
        State = GameState.Victory;
        DeathReason = "Niewiarygodne! Cały las należy do Ciebie!";
        SoundManager.Instance.PlayVictory();

        string modeKey = $"{SelectedBoardSize}_{SelectedSpeed}_{SelectedWallMode}_{SelectedSpecialMode}";
        ScoreManager.Instance.RecordGameEnd(Score, FruitsCollected, modeKey, isVictory: true);

        StateChanged?.Invoke();
        VictoryEvent?.Invoke();
    }
}
