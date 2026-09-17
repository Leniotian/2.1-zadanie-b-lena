namespace SNAKE.Models;

public enum FruitType
{
    RedApple,
    GoldenAcorn,
    BlueBerry
}

public class Fruit
{
    public Position Position { get; set; }
    public FruitType Type { get; set; }
    public int Points { get; set; }
    public int TicksRemaining { get; set; }
    public int MaxTicks { get; set; }

    public Fruit(Position pos, FruitType type = FruitType.RedApple, int ticksDuration = -1)
    {
        Position = pos;
        Type = type;
        TicksRemaining = ticksDuration;
        MaxTicks = ticksDuration;

        Points = type switch
        {
            FruitType.RedApple => 10,
            FruitType.BlueBerry => 20,
            FruitType.GoldenAcorn => 35,
            _ => 10
        };
    }

    public bool IsExpiring => TicksRemaining > 0;
    public bool HasExpired => TicksRemaining == 0;

    public void Tick()
    {
        if (TicksRemaining > 0)
        {
            TicksRemaining--;
        }
    }
}
