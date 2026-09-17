using System.IO;
using System.Text.Json;

namespace SNAKE.Engine;

public class GameStats
{
    public int HighScore { get; set; } = 0;
    public int TotalApplesEaten { get; set; } = 0;
    public int TotalGamesPlayed { get; set; } = 0;
    public int VictoriesCount { get; set; } = 0;
    public Dictionary<string, int> ModeHighScores { get; set; } = new();
}

public class ScoreManager
{
    private static ScoreManager? _instance;
    public static ScoreManager Instance => _instance ??= new ScoreManager();

    private readonly string _filePath;
    public GameStats Stats { get; private set; }

    private ScoreManager()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscore.json");
        Stats = LoadStats();
    }

    private GameStats LoadStats()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<GameStats>(json);
                if (loaded != null) return loaded;
            }
        }
        catch
        {
            // Fallback to fresh stats
        }

        return new GameStats();
    }

    public void SaveStats()
    {
        try
        {
            string json = JsonSerializer.Serialize(Stats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Silently ignore write failures
        }
    }

    public bool RecordGameEnd(int finalScore, int applesEaten, string modeKey, bool isVictory)
    {
        Stats.TotalGamesPlayed++;
        Stats.TotalApplesEaten += applesEaten;
        if (isVictory)
        {
            Stats.VictoriesCount++;
        }

        bool isNewRecord = false;
        if (finalScore > Stats.HighScore)
        {
            Stats.HighScore = finalScore;
            isNewRecord = true;
        }

        if (!Stats.ModeHighScores.TryGetValue(modeKey, out int currentModeScore) || finalScore > currentModeScore)
        {
            Stats.ModeHighScores[modeKey] = finalScore;
        }

        SaveStats();
        return isNewRecord;
    }
}
