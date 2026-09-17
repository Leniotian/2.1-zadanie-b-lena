using System.Globalization;
using System.Windows;
using System.Windows.Media;
using SNAKE.Engine;
using SNAKE.Models;

namespace SNAKE.Views;

public class GameCanvas : FrameworkElement
{
    public static readonly DependencyProperty EngineProperty =
        DependencyProperty.Register(
            nameof(Engine),
            typeof(GameEngine),
            typeof(GameCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public GameEngine? Engine
    {
        get => (GameEngine?)GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    private readonly SolidColorBrush _tileDarkBrush = new(Color.FromRgb(46, 74, 38));   // Deep forest green
    private readonly SolidColorBrush _tileLightBrush = new(Color.FromRgb(58, 92, 48));  // Lush moss green
    private readonly SolidColorBrush _borderBrush = new(Color.FromRgb(34, 49, 31));
    private readonly Pen _borderPen;

    private int _tongueAnimTick = 0;

    public GameCanvas()
    {
        _tileDarkBrush.Freeze();
        _tileLightBrush.Freeze();
        _borderBrush.Freeze();
        _borderPen = new Pen(new SolidColorBrush(Color.FromRgb(28, 43, 25)), 6);
        _borderPen.Freeze();

        ClipToBounds = true;
    }

    public void RequestRedraw()
    {
        _tongueAnimTick++;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0 || Engine == null) return;

        int gridW = Engine.GridWidth;
        int gridH = Engine.GridHeight;

        double tileSize = Math.Floor(Math.Min((width - 16) / gridW, (height - 16) / gridH));
        if (tileSize < 8) tileSize = 8;

        double boardPixelW = tileSize * gridW;
        double boardPixelH = tileSize * gridH;
        double startX = Math.Round((width - boardPixelW) / 2);
        double startY = Math.Round((height - boardPixelH) / 2);

        // Draw outer border / frame
        Rect boardRect = new(startX, startY, boardPixelW, boardPixelH);
        dc.DrawRoundedRectangle(null, _borderPen, new Rect(startX - 3, startY - 3, boardPixelW + 6, boardPixelH + 6), 10, 10);

        // Draw checkered meadow tiles
        for (int x = 0; x < gridW; x++)
        {
            for (int y = 0; y < gridH; y++)
            {
                var brush = ((x + y) % 2 == 0) ? _tileLightBrush : _tileDarkBrush;
                Rect tileRect = new(startX + x * tileSize, startY + y * tileSize, tileSize, tileSize);
                dc.DrawRectangle(brush, null, tileRect);
            }
        }

        // Draw Obstacles (Rocks, Stumps)
        foreach (var obs in Engine.Obstacles)
        {
            DrawObstacle(dc, obs, startX, startY, tileSize);
        }

        // Draw Fruits
        foreach (var fruit in Engine.ActiveFruits)
        {
            DrawFruit(dc, fruit, startX, startY, tileSize);
        }

        // Draw Snake
        DrawSnake(dc, Engine, startX, startY, tileSize);

        // Draw Floating Scores
        DrawFloatingScores(dc, Engine, startX, startY, tileSize);
    }

    private void DrawFruit(DrawingContext dc, Fruit fruit, double startX, double startY, double tileSize)
    {
        double cx = startX + fruit.Position.X * tileSize + tileSize / 2;
        double cy = startY + fruit.Position.Y * tileSize + tileSize / 2;
        double radius = tileSize * 0.42;

        if (fruit.Type == FruitType.GoldenAcorn)
        {
            // Golden Acorn with pulsing halo
            double pulse = 1.0 + 0.08 * Math.Sin(_tongueAnimTick * 0.25);
            var glowBrush = new RadialGradientBrush(Color.FromArgb(120, 255, 215, 0), Colors.Transparent);
            dc.DrawEllipse(glowBrush, null, new Point(cx, cy), radius * 1.5 * pulse, radius * 1.5 * pulse);

            // Nut body
            var acornNutBrush = new LinearGradientBrush(
                Color.FromRgb(255, 215, 0),
                Color.FromRgb(218, 165, 32),
                new Point(0.3, 0),
                new Point(0.7, 1));
            dc.DrawEllipse(acornNutBrush, null, new Point(cx, cy + radius * 0.2), radius * 0.8, radius * 0.9);

            // Acorn cap
            var capBrush = new SolidColorBrush(Color.FromRgb(121, 85, 72));
            dc.DrawRoundedRectangle(capBrush, null, new Rect(cx - radius * 0.85, cy - radius * 0.7, radius * 1.7, radius * 0.75), 4, 4);

            // Little stem
            var stemPen = new Pen(new SolidColorBrush(Color.FromRgb(93, 64, 55)), Math.Max(2, tileSize * 0.08));
            dc.DrawLine(stemPen, new Point(cx, cy - radius * 0.6), new Point(cx + radius * 0.2, cy - radius * 1.1));

            // Highlight sparkle
            var shineBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            dc.DrawEllipse(shineBrush, null, new Point(cx - radius * 0.3, cy + radius * 0.1), radius * 0.2, radius * 0.2);
        }
        else if (fruit.Type == FruitType.BlueBerry)
        {
            // Forest Berry Cluster
            var berryBrush = new RadialGradientBrush(Color.FromRgb(129, 212, 250), Color.FromRgb(2, 119, 189));
            dc.DrawEllipse(berryBrush, null, new Point(cx - radius * 0.3, cy + radius * 0.2), radius * 0.6, radius * 0.6);
            dc.DrawEllipse(berryBrush, null, new Point(cx + radius * 0.3, cy + radius * 0.2), radius * 0.6, radius * 0.6);
            dc.DrawEllipse(berryBrush, null, new Point(cx, cy - radius * 0.3), radius * 0.6, radius * 0.6);

            // Green leaf
            var leafBrush = new SolidColorBrush(Color.FromRgb(139, 195, 74));
            dc.DrawEllipse(leafBrush, null, new Point(cx, cy - radius * 0.75), radius * 0.3, radius * 0.18);
        }
        else
        {
            // Classic Google Snake Red Apple
            // Apple body shadow
            var shadowBrush = new SolidColorBrush(Color.FromArgb(80, 20, 30, 15));
            dc.DrawEllipse(shadowBrush, null, new Point(cx, cy + radius * 0.7), radius * 0.7, radius * 0.3);

            // Plump Red Apple body
            var appleGradient = new RadialGradientBrush(
                Color.FromRgb(255, 82, 82),
                Color.FromRgb(211, 47, 47));
            appleGradient.GradientOrigin = new Point(0.35, 0.35);

            dc.DrawEllipse(appleGradient, null, new Point(cx - radius * 0.25, cy), radius * 0.72, radius * 0.75);
            dc.DrawEllipse(appleGradient, null, new Point(cx + radius * 0.25, cy), radius * 0.72, radius * 0.75);

            // Apple stem
            var stemPen = new Pen(new SolidColorBrush(Color.FromRgb(109, 76, 65)), Math.Max(2, tileSize * 0.09));
            stemPen.StartLineCap = PenLineCap.Round;
            stemPen.EndLineCap = PenLineCap.Round;
            dc.DrawLine(stemPen, new Point(cx, cy - radius * 0.4), new Point(cx + radius * 0.25, cy - radius * 1.05));

            // Shiny Green Leaf
            var leafBrush = new SolidColorBrush(Color.FromRgb(124, 179, 66));
            var leafPath = new PathGeometry();
            var leafFig = new PathFigure { StartPoint = new Point(cx + radius * 0.1, cy - radius * 0.75), IsClosed = true, IsFilled = true };
            leafFig.Segments.Add(new QuadraticBezierSegment(new Point(cx + radius * 0.8, cy - radius * 1.1), new Point(cx + radius * 0.9, cy - radius * 0.6), true));
            leafFig.Segments.Add(new QuadraticBezierSegment(new Point(cx + radius * 0.5, cy - radius * 0.55), new Point(cx + radius * 0.1, cy - radius * 0.75), true));
            leafPath.Figures.Add(leafFig);
            dc.DrawGeometry(leafBrush, null, leafPath);

            // Specular sheen
            var sheenBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
            dc.DrawEllipse(sheenBrush, null, new Point(cx - radius * 0.35, cy - radius * 0.25), radius * 0.2, radius * 0.14);
        }
    }

    private void DrawObstacle(DrawingContext dc, Obstacle obs, double startX, double startY, double tileSize)
    {
        double x = startX + obs.Position.X * tileSize;
        double y = startY + obs.Position.Y * tileSize;
        double margin = tileSize * 0.1;
        double w = tileSize - 2 * margin;

        if (obs.Type == ObstacleType.MossyRock)
        {
            // Gray rock
            var rockBrush = new LinearGradientBrush(
                Color.FromRgb(120, 144, 156),
                Color.FromRgb(55, 71, 79),
                new Point(0, 0),
                new Point(1, 1));
            dc.DrawRoundedRectangle(rockBrush, null, new Rect(x + margin, y + margin, w, w), w * 0.45, w * 0.45);

            // Moss cap
            var mossBrush = new SolidColorBrush(Color.FromRgb(104, 159, 56));
            dc.DrawEllipse(mossBrush, null, new Point(x + margin + w * 0.4, y + margin + w * 0.25), w * 0.35, w * 0.2);
        }
        else
        {
            // Tree stump
            var stumpBrush = new SolidColorBrush(Color.FromRgb(93, 64, 55));
            dc.DrawRoundedRectangle(stumpBrush, null, new Rect(x + margin, y + margin, w, w), 6, 6);

            // Wood rings
            var ringBrush = new SolidColorBrush(Color.FromRgb(141, 110, 99));
            dc.DrawEllipse(ringBrush, null, new Point(x + tileSize / 2, y + tileSize / 2), w * 0.35, w * 0.35);

            var centerBrush = new SolidColorBrush(Color.FromRgb(109, 76, 65));
            dc.DrawEllipse(centerBrush, null, new Point(x + tileSize / 2, y + tileSize / 2), w * 0.15, w * 0.15);
        }
    }

    private void DrawSnake(DrawingContext dc, GameEngine engine, double startX, double startY, double tileSize)
    {
        var body = engine.SnakeBody;
        if (body.Count == 0) return;

        double t = Math.Clamp(engine.InterpolationProgress, 0.0, 1.0);
        var prevBody = engine.PreviousBody;

        var skin = engine.SelectedSkin;
        var primaryBrush = new SolidColorBrush(skin.BodyColorA);
        var secondaryBrush = new SolidColorBrush(skin.BodyColorB);
        var patternBrush = new SolidColorBrush(skin.PatternColor);
        var headBrush = new SolidColorBrush(skin.HeadColor);
        var eyeBrush = new SolidColorBrush(skin.EyeColor);
        var pupilBrush = new SolidColorBrush(skin.PupilColor);
        var tongueBrush = new SolidColorBrush(skin.TongueColor);

        // Precompute interpolated centers
        Point[] centers = new Point[body.Count];
        for (int i = 0; i < body.Count; i++)
        {
            Position curr = body[i];
            Position prev = (i < prevBody.Count) ? prevBody[i] : curr;
            Point coord = GetInterpolatedCoord(curr, prev, t, engine.GridWidth, engine.GridHeight);
            centers[i] = new Point(
                startX + coord.X * tileSize + tileSize / 2,
                startY + coord.Y * tileSize + tileSize / 2);
        }

        double baseSegSize = tileSize * 0.86;

        // Draw body segments and connecting smooth capsules from tail to neck
        for (int i = body.Count - 1; i >= 1; i--)
        {
            Point pCurrent = centers[i];
            Point pNext = centers[i - 1];

            var segBrush = (i % 2 == 0) ? primaryBrush : secondaryBrush;
            double segSize = (i == body.Count - 1) ? baseSegSize * 0.80 : baseSegSize;

            // Connect to previous segment if not wrapping across opposite borders
            double dist = (pCurrent - pNext).Length;
            if (dist < tileSize * 2.2)
            {
                var connectorPen = new Pen(segBrush, segSize)
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };
                dc.DrawLine(connectorPen, pCurrent, pNext);
            }

            // Draw segment joint circle
            dc.DrawEllipse(segBrush, null, pCurrent, segSize / 2, segSize / 2);

            // Skin patterns
            DrawBodyPattern(dc, skin.Pattern, patternBrush, pCurrent.X, pCurrent.Y, tileSize * 0.22);
        }

        // Draw Head at centers[0]
        Point headCenter = centers[0];
        double headRadius = baseSegSize * 0.52;
        dc.DrawEllipse(headBrush, null, headCenter, headRadius, headRadius);

        // Tongue flick animation
        bool showTongue = (_tongueAnimTick / 5) % 3 == 0 && engine.State == GameState.Running;
        if (showTongue)
        {
            DrawTongue(dc, engine.CurrentDirection, tongueBrush, headCenter.X, headCenter.Y, tileSize);
        }

        // Animated Google-style Eyes
        double hx = headCenter.X - tileSize / 2;
        double hy = headCenter.Y - tileSize / 2;
        DrawEyes(dc, engine, eyeBrush, pupilBrush, hx, hy, tileSize);
    }

    private static Point GetInterpolatedCoord(Position curr, Position prev, double t, int gridW, int gridH)
    {
        int dx = curr.X - prev.X;
        int dy = curr.Y - prev.Y;

        // Portal wrapping horizontally
        if (Math.Abs(dx) > 1)
        {
            if (curr.X < prev.X)
            {
                double x = t < 0.5 ? prev.X + t : curr.X - (1.0 - t);
                return new Point(x, curr.Y);
            }
            else
            {
                double x = t < 0.5 ? prev.X - t : curr.X + (1.0 - t);
                return new Point(x, curr.Y);
            }
        }

        // Portal wrapping vertically
        if (Math.Abs(dy) > 1)
        {
            if (curr.Y < prev.Y)
            {
                double y = t < 0.5 ? prev.Y + t : curr.Y - (1.0 - t);
                return new Point(curr.X, y);
            }
            else
            {
                double y = t < 0.5 ? prev.Y - t : curr.Y + (1.0 - t);
                return new Point(curr.X, y);
            }
        }

        return new Point(prev.X + dx * t, prev.Y + dy * t);
    }

    private void DrawBodyPattern(DrawingContext dc, SkinPattern pattern, Brush brush, double cx, double cy, double size)
    {
        switch (pattern)
        {
            case SkinPattern.Spotted:
                dc.DrawEllipse(brush, null, new Point(cx, cy), size * 0.75, size * 0.75);
                break;
            case SkinPattern.Diamond:
                var diamond = new PathGeometry();
                var fig = new PathFigure { StartPoint = new Point(cx, cy - size), IsClosed = true, IsFilled = true };
                fig.Segments.Add(new LineSegment(new Point(cx + size * 0.8, cy), true));
                fig.Segments.Add(new LineSegment(new Point(cx, cy + size), true));
                fig.Segments.Add(new LineSegment(new Point(cx - size * 0.8, cy), true));
                diamond.Figures.Add(fig);
                dc.DrawGeometry(brush, null, diamond);
                break;
            case SkinPattern.Striped:
                dc.DrawRoundedRectangle(brush, null, new Rect(cx - size * 0.9, cy - size * 0.35, size * 1.8, size * 0.7), 2, 2);
                break;
        }
    }

    private void DrawTongue(DrawingContext dc, Direction dir, Brush tongueBrush, double cx, double cy, double tileSize)
    {
        double tLen = tileSize * 0.45;
        var (dx, dy) = dir.ToOffset();
        double startX = cx + dx * (tileSize * 0.45);
        double startY = cy + dy * (tileSize * 0.45);
        double endX = startX + dx * tLen;
        double endY = startY + dy * tLen;

        var pen = new Pen(tongueBrush, Math.Max(2, tileSize * 0.08));
        pen.StartLineCap = PenLineCap.Round;
        pen.EndLineCap = PenLineCap.Round;

        dc.DrawLine(pen, new Point(startX, startY), new Point(endX, endY));

        // Fork tip
        double forkSize = tileSize * 0.16;
        if (dir == Direction.Up || dir == Direction.Down)
        {
            dc.DrawLine(pen, new Point(endX, endY), new Point(endX - forkSize, endY + dy * forkSize));
            dc.DrawLine(pen, new Point(endX, endY), new Point(endX + forkSize, endY + dy * forkSize));
        }
        else
        {
            dc.DrawLine(pen, new Point(endX, endY), new Point(endX + dx * forkSize, endY - forkSize));
            dc.DrawLine(pen, new Point(endX, endY), new Point(endX + dx * forkSize, endY + forkSize));
        }
    }

    private void DrawEyes(DrawingContext dc, GameEngine engine, Brush eyeBrush, Brush pupilBrush, double hx, double hy, double tileSize)
    {
        Direction dir = engine.CurrentDirection;
        bool isDead = engine.State == GameState.GameOver;

        double eyeRadius = tileSize * 0.16;
        double pupilRadius = eyeRadius * 0.55;

        // Position eyes based on direction
        Point eye1, eye2;
        double pupilOffX = 0, pupilOffY = 0;

        switch (dir)
        {
            case Direction.Up:
                eye1 = new Point(hx + tileSize * 0.28, hy + tileSize * 0.28);
                eye2 = new Point(hx + tileSize * 0.72, hy + tileSize * 0.28);
                pupilOffY = -eyeRadius * 0.4;
                break;
            case Direction.Down:
                eye1 = new Point(hx + tileSize * 0.28, hy + tileSize * 0.72);
                eye2 = new Point(hx + tileSize * 0.72, hy + tileSize * 0.72);
                pupilOffY = eyeRadius * 0.4;
                break;
            case Direction.Left:
                eye1 = new Point(hx + tileSize * 0.28, hy + tileSize * 0.28);
                eye2 = new Point(hx + tileSize * 0.28, hy + tileSize * 0.72);
                pupilOffX = -eyeRadius * 0.4;
                break;
            case Direction.Right:
            default:
                eye1 = new Point(hx + tileSize * 0.72, hy + tileSize * 0.28);
                eye2 = new Point(hx + tileSize * 0.72, hy + tileSize * 0.72);
                pupilOffX = eyeRadius * 0.4;
                break;
        }

        // Draw Eye Whites
        dc.DrawEllipse(eyeBrush, null, eye1, eyeRadius, eyeRadius);
        dc.DrawEllipse(eyeBrush, null, eye2, eyeRadius, eyeRadius);

        if (isDead)
        {
            // Cute Google Snake X-eyes on Game Over!
            var xPen = new Pen(pupilBrush, Math.Max(2, tileSize * 0.08));
            DrawCross(dc, eye1, eyeRadius * 0.75, xPen);
            DrawCross(dc, eye2, eyeRadius * 0.75, xPen);
        }
        else
        {
            // Pupils looking forward
            Point p1 = new(eye1.X + pupilOffX, eye1.Y + pupilOffY);
            Point p2 = new(eye2.X + pupilOffX, eye2.Y + pupilOffY);
            dc.DrawEllipse(pupilBrush, null, p1, pupilRadius, pupilRadius);
            dc.DrawEllipse(pupilBrush, null, p2, pupilRadius, pupilRadius);

            // Little cute white reflection dot
            var shineBrush = new SolidColorBrush(Colors.White);
            dc.DrawEllipse(shineBrush, null, new Point(p1.X - pupilRadius * 0.3, p1.Y - pupilRadius * 0.3), pupilRadius * 0.35, pupilRadius * 0.35);
            dc.DrawEllipse(shineBrush, null, new Point(p2.X - pupilRadius * 0.3, p2.Y - pupilRadius * 0.3), pupilRadius * 0.35, pupilRadius * 0.35);
        }
    }

    private void DrawCross(DrawingContext dc, Point center, double size, Pen pen)
    {
        dc.DrawLine(pen, new Point(center.X - size, center.Y - size), new Point(center.X + size, center.Y + size));
        dc.DrawLine(pen, new Point(center.X + size, center.Y - size), new Point(center.X - size, center.Y + size));
    }

    private void DrawFloatingScores(DrawingContext dc, GameEngine engine, double startX, double startY, double tileSize)
    {
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal);

        foreach (var score in engine.FloatingScores)
        {
            double x = startX + score.X * tileSize + tileSize / 2;
            double y = startY + score.Y * tileSize + tileSize * 0.2;

            var textBrush = new SolidColorBrush(score.Color) { Opacity = score.Opacity };
            var formatted = new FormattedText(
                score.Text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                Math.Max(12, tileSize * 0.55),
                textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // Center text
            dc.DrawText(formatted, new Point(x - formatted.Width / 2, y - formatted.Height / 2));
        }
    }
}
