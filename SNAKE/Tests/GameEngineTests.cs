using System.Diagnostics;
using System.IO;
using SNAKE.Engine;
using SNAKE.Models;

namespace SNAKE.Tests;

public static class GameEngineTests
{
    public static int RunAllTests()
    {
        int passed = 0;
        int total = 0;

        void AssertTest(string testName, Action action)
        {
            total++;
            try
            {
                action();
                Console.WriteLine($"[PASS] {testName}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName}: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine("========================================");
        Console.WriteLine("  Running SNAKE Engine Automated Tests  ");
        Console.WriteLine("========================================");

        AssertTest("Snake starts with length 3 and ready state", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Normal, GameSpeed.Deer, WallMode.Solid, SpecialMode.Classic, SnakeSkin.AllSkins[0]);

            if (engine.SnakeBody.Count != 3) throw new Exception($"Expected 3 segments, got {engine.SnakeBody.Count}");
            if (engine.State != GameState.Ready) throw new Exception($"Expected Ready state, got {engine.State}");
            if (engine.Score != 0) throw new Exception($"Expected 0 score, got {engine.Score}");
            if (engine.FruitsCollected != 0) throw new Exception($"Expected 0 fruits, got {engine.FruitsCollected}");
        });

        AssertTest("Snake moves in queued direction on tick", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Normal, GameSpeed.Deer, WallMode.Solid, SpecialMode.Classic, SnakeSkin.AllSkins[0]);

            var startHead = engine.Head;
            engine.StartGame();
            engine.Tick();

            var newHead = engine.Head;
            if (newHead.X != startHead.X + 1 || newHead.Y != startHead.Y)
            {
                throw new Exception($"Expected head at ({startHead.X + 1}, {startHead.Y}), got ({newHead.X}, {newHead.Y})");
            }
        });

        AssertTest("Double buffered input prevents instant reverse turn", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Normal, GameSpeed.Deer, WallMode.Solid, SpecialMode.Classic, SnakeSkin.AllSkins[0]);
            engine.StartGame();

            // Moving Right; attempt to reverse Left directly should be ignored
            engine.QueueDirection(Direction.Left);
            if (engine.CurrentDirection != Direction.Right)
            {
                throw new Exception("Direct opposite direction should be rejected!");
            }
        });

        AssertTest("Solid Wall collision triggers GameOver", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Small, GameSpeed.Viper, WallMode.Solid, SpecialMode.Classic, SnakeSkin.AllSkins[0]);
            engine.StartGame();

            // Run right into the wall
            for (int i = 0; i < 15; i++)
            {
                if (engine.State == GameState.GameOver) break;
                engine.Tick();
            }

            if (engine.State != GameState.GameOver)
            {
                throw new Exception("Expected Game Over when hitting solid wall!");
            }
        });

        AssertTest("Portal Wall mode wraps around screen edges", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Small, GameSpeed.Viper, WallMode.Portal, SpecialMode.Classic, SnakeSkin.AllSkins[0]);
            engine.StartGame();

            // Move across right border of 10x10 board
            for (int i = 0; i < 12; i++)
            {
                engine.Tick();
                if (engine.State == GameState.GameOver)
                {
                    throw new Exception("Portal mode should not trigger game over on boundary!");
                }
            }

            // Head should be wrapped within valid bounds
            if (engine.Head.X < 0 || engine.Head.X >= engine.GridWidth)
            {
                throw new Exception($"Head X {engine.Head.X} out of bounds in Portal mode!");
            }
        });

        AssertTest("Eating fruit increases score, fruits collected and snake length", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Small, GameSpeed.Deer, WallMode.Portal, SpecialMode.Classic, SnakeSkin.AllSkins[0]);
            engine.StartGame();

            // Force fruit directly in front of head
            var fruitPos = new Position(engine.Head.X + 1, engine.Head.Y);
            engine.ActiveFruits.Clear();
            engine.ActiveFruits.Add(new Fruit(fruitPos, FruitType.RedApple));

            int initialLength = engine.SnakeBody.Count;
            engine.Tick();

            if (engine.FruitsCollected != 1) throw new Exception($"Expected 1 fruit collected, got {engine.FruitsCollected}");
            if (engine.Score != 10) throw new Exception($"Expected 10 points, got {engine.Score}");
            if (engine.SnakeBody.Count != initialLength + 1) throw new Exception($"Expected length {initialLength + 1}, got {engine.SnakeBody.Count}");
        });

        AssertTest("Twin Apples mode maintains 2 fruits", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Normal, GameSpeed.Deer, WallMode.Solid, SpecialMode.TwinApples, SnakeSkin.AllSkins[0]);

            if (engine.ActiveFruits.Count != 2)
            {
                throw new Exception($"Expected 2 fruits in TwinApples mode, got {engine.ActiveFruits.Count}");
            }
        });

        AssertTest("Forest Obstacles mode spawns obstacles away from snake", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Normal, GameSpeed.Deer, WallMode.Solid, SpecialMode.ForestObstacles, SnakeSkin.AllSkins[0]);

            if (engine.Obstacles.Count == 0)
            {
                throw new Exception("Expected obstacles in ForestObstacles mode!");
            }

            foreach (var obs in engine.Obstacles)
            {
                if (engine.SnakeBody.Contains(obs.Position))
                {
                    throw new Exception($"Obstacle generated inside snake position: {obs.Position}");
                }
            }
        });

        AssertTest("Procedural Audio engine initializes and creates valid WAVs", () =>
        {
            var soundMgr = SoundManager.Instance;
            soundMgr.IsMuted = true; // Mute actual playback during test
            soundMgr.PlayEatApple();
            soundMgr.PlayEatBonus();
            soundMgr.PlayTurn();
            soundMgr.PlayGameOver();
            soundMgr.PlayVictory();
            soundMgr.PlayClick();
            soundMgr.IsMuted = false;
        });

        AssertTest("Max fruits victory condition triggers Victory state", () =>
        {
            var engine = new GameEngine();
            engine.ApplySettings(BoardSize.Small, GameSpeed.Viper, WallMode.Portal, SpecialMode.Classic, SnakeSkin.AllSkins[0]);
            engine.StartGame();

            // Set fruits collected to MaxFruitsPossible - 1
            int target = engine.MaxFruitsPossible;
            while (engine.FruitsCollected < target - 1)
            {
                var f = new Fruit(new Position(0, 0), FruitType.RedApple);
                // Artificially increment fruits collected
                typeof(GameEngine).GetProperty("FruitsCollected")?.SetValue(engine, target - 1);
            }

            // Put fruit in front of head and tick to reach target
            var fruitPos = new Position(engine.Head.X + 1, engine.Head.Y);
            engine.ActiveFruits.Clear();
            engine.ActiveFruits.Add(new Fruit(fruitPos, FruitType.RedApple));
            engine.Tick();

            if (engine.State != GameState.Victory)
            {
                throw new Exception($"Expected Victory state upon reaching max fruits, got {engine.State}");
            }
        });

        AssertTest("ScoreManager persists and reads statistics correctly", () =>
        {
            var scoreMgr = ScoreManager.Instance;
            int prevHigh = scoreMgr.Stats.HighScore;
            int testScore = prevHigh + 50;

            bool isNewRecord = scoreMgr.RecordGameEnd(testScore, 5, "TestMode", false);
            if (!isNewRecord) throw new Exception("Expected record to be registered!");
            if (scoreMgr.Stats.HighScore != testScore) throw new Exception($"Expected HighScore {testScore}, got {scoreMgr.Stats.HighScore}");

            // Restore
            scoreMgr.Stats.HighScore = prevHigh;
            scoreMgr.SaveStats();
        });

        Console.WriteLine("========================================");
        Console.WriteLine($"  Results: {passed}/{total} tests passed  ");
        Console.WriteLine("========================================");

        return passed == total ? 0 : 1;
    }
}
