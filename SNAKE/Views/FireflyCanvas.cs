using System.Windows;
using System.Windows.Media;

namespace SNAKE.Views;

public class FireflyCanvas : FrameworkElement
{
    private class Firefly
    {
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Size;
        public double Phase;
        public double PulseSpeed;
        public Color Color;
    }

    private readonly List<Firefly> _fireflies = new();
    private readonly Random _rand = new();

    public FireflyCanvas()
    {
        IsHitTestVisible = false;
        Loaded += (s, e) => InitializeFireflies();
    }

    private void InitializeFireflies()
    {
        _fireflies.Clear();
        int count = 30;
        double w = ActualWidth > 0 ? ActualWidth : 800;
        double h = ActualHeight > 0 ? ActualHeight : 600;

        Color[] palette =
        {
            Color.FromRgb(255, 235, 150), // Soft warm gold
            Color.FromRgb(200, 255, 150), // Light forest lime
            Color.FromRgb(255, 215, 100), // Amber firefly
            Color.FromRgb(165, 235, 180)  // Mint glow
        };

        for (int i = 0; i < count; i++)
        {
            _fireflies.Add(new Firefly
            {
                X = _rand.NextDouble() * w,
                Y = _rand.NextDouble() * h,
                Vx = (_rand.NextDouble() - 0.5) * 0.4,
                Vy = -0.15 - _rand.NextDouble() * 0.4, // float gently upwards
                Size = 2.0 + _rand.NextDouble() * 3.5,
                Phase = _rand.NextDouble() * Math.PI * 2,
                PulseSpeed = 0.03 + _rand.NextDouble() * 0.04,
                Color = palette[_rand.Next(palette.Length)]
            });
        }
    }

    public void Advance()
    {
        double w = ActualWidth > 0 ? ActualWidth : 800;
        double h = ActualHeight > 0 ? ActualHeight : 600;

        foreach (var f in _fireflies)
        {
            f.X += f.Vx + Math.Sin(f.Phase) * 0.3;
            f.Y += f.Vy;
            f.Phase += f.PulseSpeed;

            // Wrap edges
            if (f.Y < -10) f.Y = h + 10;
            if (f.X < -10) f.X = w + 10;
            if (f.X > w + 10) f.X = -10;
        }

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        foreach (var f in _fireflies)
        {
            double alpha = 0.25 + 0.65 * (0.5 + 0.5 * Math.Sin(f.Phase));
            Color c = Color.FromArgb((byte)(alpha * 255), f.Color.R, f.Color.G, f.Color.B);

            // Subtle glow halo
            var glowBrush = new RadialGradientBrush(
                Color.FromArgb((byte)(alpha * 90), f.Color.R, f.Color.G, f.Color.B),
                Colors.Transparent);
            dc.DrawEllipse(glowBrush, null, new Point(f.X, f.Y), f.Size * 3.0, f.Size * 3.0);

            // Sharp center mote
            var coreBrush = new SolidColorBrush(c);
            dc.DrawEllipse(coreBrush, null, new Point(f.X, f.Y), f.Size * 0.6, f.Size * 0.6);
        }
    }
}
