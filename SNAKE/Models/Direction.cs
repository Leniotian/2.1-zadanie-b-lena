namespace SNAKE.Models;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public static class DirectionExtensions
{
    public static bool IsOpposite(this Direction dir, Direction other)
    {
        return (dir == Direction.Up && other == Direction.Down) ||
               (dir == Direction.Down && other == Direction.Up) ||
               (dir == Direction.Left && other == Direction.Right) ||
               (dir == Direction.Right && other == Direction.Left);
    }

    public static (int Dx, int Dy) ToOffset(this Direction dir)
    {
        return dir switch
        {
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            _ => (0, 0)
        };
    }
}
