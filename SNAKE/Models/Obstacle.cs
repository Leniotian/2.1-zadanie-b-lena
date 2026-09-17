namespace SNAKE.Models;

public enum ObstacleType
{
    MossyRock,
    TreeStump
}

public class Obstacle
{
    public Position Position { get; }
    public ObstacleType Type { get; }

    public Obstacle(Position pos, ObstacleType type)
    {
        Position = pos;
        Type = type;
    }
}
