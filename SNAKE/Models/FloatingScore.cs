using System.Windows.Media;

namespace SNAKE.Models;

public class FloatingScore
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Text { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Gold;
    public double Opacity { get; set; } = 1.0;
    public double Scale { get; set; } = 1.0;
    public int LifetimeTicks { get; set; } = 20;

    public bool IsDead => LifetimeTicks <= 0;

    public void Update()
    {
        Y -= 1.2;
        LifetimeTicks--;
        Opacity = Math.Max(0.0, LifetimeTicks / 20.0);
    }
}
