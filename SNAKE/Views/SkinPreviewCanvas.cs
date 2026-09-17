using System.Windows;
using System.Windows.Media;
using SNAKE.Models;

namespace SNAKE.Views;

public class SkinPreviewCanvas : FrameworkElement
{
    public static readonly DependencyProperty SkinProperty =
        DependencyProperty.Register(
            nameof(Skin),
            typeof(SnakeSkin),
            typeof(SkinPreviewCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public SnakeSkin? Skin
    {
        get => (SnakeSkin?)GetValue(SkinProperty);
        set => SetValue(SkinProperty, value);
    }

    private double _animPhase = 0;

    public void AdvanceAnimation()
    {
        _animPhase += 0.12;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double w = ActualWidth;
        double h = ActualHeight;
        if (w <= 0 || h <= 0 || Skin == null) return;

        // Draw soft dark woodland background card
        var bgBrush = new SolidColorBrush(Color.FromRgb(30, 48, 26));
        dc.DrawRoundedRectangle(bgBrush, null, new Rect(0, 0, w, h), 8, 8);

        // Calculate positions for a slithering S-curve preview
        int segmentCount = 6;
        double segRadius = Math.Min(w, h) * 0.14;
        double cy = h / 2;

        var primaryBrush = new SolidColorBrush(Skin.BodyColorA);
        var secondaryBrush = new SolidColorBrush(Skin.BodyColorB);
        var patternBrush = new SolidColorBrush(Skin.PatternColor);
        var headBrush = new SolidColorBrush(Skin.HeadColor);
        var eyeBrush = new SolidColorBrush(Skin.EyeColor);
        var pupilBrush = new SolidColorBrush(Skin.PupilColor);
        var tongueBrush = new SolidColorBrush(Skin.TongueColor);

        // Draw body segments (from tail to neck)
        for (int i = segmentCount - 1; i >= 1; i--)
        {
            double t = (double)i / (segmentCount - 1);
            double x = w * 0.22 + (1.0 - t) * (w * 0.52);
            double waveY = cy + Math.Sin(_animPhase + i * 0.8) * (h * 0.18);

            double r = (i == segmentCount - 1) ? segRadius * 0.75 : segRadius;
            var brush = (i % 2 == 0) ? primaryBrush : secondaryBrush;

            dc.DrawEllipse(brush, null, new Point(x, waveY), r, r);

            // Patterns
            if (Skin.Pattern == SkinPattern.Spotted)
            {
                dc.DrawEllipse(patternBrush, null, new Point(x, waveY), r * 0.4, r * 0.4);
            }
            else if (Skin.Pattern == SkinPattern.Diamond)
            {
                dc.DrawRectangle(patternBrush, null, new Rect(x - r * 0.3, waveY - r * 0.3, r * 0.6, r * 0.6));
            }
        }

        // Draw Head
        double headX = w * 0.75;
        double headY = cy + Math.Sin(_animPhase + 0.3) * (h * 0.18);
        double headR = segRadius * 1.15;

        // Tongue
        double tLen = segRadius * 0.8;
        var pen = new Pen(tongueBrush, 2.5);
        dc.DrawLine(pen, new Point(headX + headR * 0.8, headY), new Point(headX + headR + tLen, headY));
        dc.DrawLine(pen, new Point(headX + headR + tLen, headY), new Point(headX + headR + tLen + 4, headY - 3));
        dc.DrawLine(pen, new Point(headX + headR + tLen, headY), new Point(headX + headR + tLen + 4, headY + 3));

        // Head ellipse
        dc.DrawEllipse(headBrush, null, new Point(headX, headY), headR, headR);

        // Eyes
        double eyeR = headR * 0.35;
        double pupilR = eyeR * 0.55;
        Point eye1 = new(headX + headR * 0.3, headY - headR * 0.4);
        Point eye2 = new(headX + headR * 0.3, headY + headR * 0.4);

        dc.DrawEllipse(eyeBrush, null, eye1, eyeR, eyeR);
        dc.DrawEllipse(eyeBrush, null, eye2, eyeR, eyeR);

        // Pupils looking right
        dc.DrawEllipse(pupilBrush, null, new Point(eye1.X + eyeR * 0.3, eye1.Y), pupilR, pupilR);
        dc.DrawEllipse(pupilBrush, null, new Point(eye2.X + eyeR * 0.3, eye2.Y), pupilR, pupilR);
    }
}
