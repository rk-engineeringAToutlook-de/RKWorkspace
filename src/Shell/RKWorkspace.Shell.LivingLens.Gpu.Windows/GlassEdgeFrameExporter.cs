using RKWorkspace.Shell;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WBrush = System.Windows.Media.Brush;
using WColor = System.Windows.Media.Color;
using WPen = System.Windows.Media.Pen;
using WPoint = System.Windows.Point;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public static class GlassEdgeFrameExporter
{
    private const int Width = 960;
    private const int Height = 540;

    public static IReadOnlyList<string> Export(string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        var files = new[]
        {
            ("glass_edge_right.png", AblageDirection.Right, AblageDistanceKind.Near, 0.35, 0.0),
            ("glass_edge_left.png", AblageDirection.Left, AblageDistanceKind.Near, 0.35, 0.0),
            ("glass_edge_top.png", AblageDirection.Up, AblageDistanceKind.Near, 0.35, 0.0),
            ("glass_edge_bottom.png", AblageDirection.Down, AblageDistanceKind.Near, 0.35, 0.0),
            ("glass_edge_near.png", AblageDirection.Right, AblageDistanceKind.VeryNear, 0.78, 0.0),
            ("glass_edge_far.png", AblageDirection.Right, AblageDistanceKind.Far, 0.26, 0.0),
            ("edge_absorption_sequence.png", AblageDirection.Right, AblageDistanceKind.Near, 1.0, 0.72),
            ("counter_edge_emergence.png", AblageDirection.Left, AblageDistanceKind.Near, 0.82, 0.58),
            ("mobile_glass_edge.png", AblageDirection.Up, AblageDistanceKind.Near, 0.64, 0.18),
            ("nearest_ablage_selection.png", AblageDirection.Right, AblageDistanceKind.Near, 0.52, 0.0)
        };

        var exported = new List<string>();
        foreach (var (name, direction, distance, activation, absorption) in files)
        {
            var path = Path.Combine(targetDirectory, name);
            Render(path, direction, distance, activation, absorption);
            exported.Add(path);
        }

        return exported;
    }

    private static void Render(string path, AblageDirection direction, AblageDistanceKind distance, double activation, double absorption)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            DrawSurface(context);
            DrawEdge(context, direction, distance, activation);
            DrawThing(context, direction, absorption);
        }

        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void DrawSurface(DrawingContext context)
    {
        var background = new LinearGradientBrush(
            WColor.FromRgb(232, 237, 235),
            WColor.FromRgb(216, 226, 222),
            new WPoint(0, 0),
            new WPoint(1, 1));
        context.DrawRectangle(background, null, new Rect(0, 0, Width, Height));

        var gridPen = new WPen(new SolidColorBrush(WColor.FromArgb(18, 42, 56, 52)), 1);
        for (var x = 0; x <= Width; x += 80)
        {
            context.DrawLine(gridPen, new WPoint(x, 0), new WPoint(x, Height));
        }

        for (var y = 0; y <= Height; y += 72)
        {
            context.DrawLine(gridPen, new WPoint(0, y), new WPoint(Width, y));
        }
    }

    private static void DrawEdge(DrawingContext context, AblageDirection direction, AblageDistanceKind distance, double activation)
    {
        var profile = GlassEdgeVisualProfile.FromDistance(distance);
        var thickness = profile.Thickness * (0.76 + activation * 0.54);
        var opacity = profile.Opacity * (0.36 + activation * 0.64);
        var rect = EdgeRect(direction, thickness);
        var brush = EdgeBrush(direction, opacity);
        context.DrawRectangle(brush, null, rect);

        var rim = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(72 + activation * 78), 232, 255, 255)), 1.15);
        var dark = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(58 + activation * 64), 12, 32, 36)), 1.35);
        DrawEdgeLine(context, direction, rim, 0);
        DrawEdgeLine(context, direction, dark, direction is AblageDirection.Left or AblageDirection.Up ? thickness * 0.82 : -thickness * 0.82);

        var glint = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(42 + activation * 84), 255, 255, 255)), 0.72);
        DrawEdgeLine(context, direction, glint, direction is AblageDirection.Left or AblageDirection.Up ? thickness * 0.24 : -thickness * 0.24);
    }

    private static void DrawThing(DrawingContext context, AblageDirection direction, double absorption)
    {
        if (absorption <= 0.0)
        {
            return;
        }

        var center = direction switch
        {
            AblageDirection.Left => new WPoint(140, Height * 0.52),
            AblageDirection.Right => new WPoint(Width - 140, Height * 0.52),
            AblageDirection.Up => new WPoint(Width * 0.50, 120),
            AblageDirection.Down => new WPoint(Width * 0.50, Height - 120),
            _ => new WPoint(Width * 0.50, Height * 0.50)
        };

        var width = 190 * (1.0 - absorption * 0.42);
        var height = 104 * (1.0 - absorption * 0.36);
        var rect = new Rect(center.X - width / 2, center.Y - height / 2, width, height);
        var shadow = new RadialGradientBrush(WColor.FromArgb(74, 18, 26, 24), WColor.FromArgb(0, 18, 26, 24));
        context.DrawEllipse(shadow, null, center + new Vector(0, height * 0.62), width * 0.48, height * 0.22);
        context.DrawRectangle(new SolidColorBrush(WColor.FromArgb(202, 248, 250, 247)), new WPen(new SolidColorBrush(WColor.FromArgb(70, 32, 42, 40)), 1), rect);
    }

    private static Rect EdgeRect(AblageDirection direction, double thickness)
    {
        return direction switch
        {
            AblageDirection.Left => new Rect(0, 0, thickness, Height),
            AblageDirection.Right => new Rect(Width - thickness, 0, thickness, Height),
            AblageDirection.Up => new Rect(0, 0, Width, thickness),
            AblageDirection.Down => new Rect(0, Height - thickness, Width, thickness),
            _ => new Rect(Width - thickness, 0, thickness, Height)
        };
    }

    private static WBrush EdgeBrush(AblageDirection direction, double opacity)
    {
        var points = direction switch
        {
            AblageDirection.Left => (Start: new WPoint(0, 0), End: new WPoint(1, 0)),
            AblageDirection.Right => (Start: new WPoint(1, 0), End: new WPoint(0, 0)),
            AblageDirection.Up => (Start: new WPoint(0, 0), End: new WPoint(0, 1)),
            AblageDirection.Down => (Start: new WPoint(0, 1), End: new WPoint(0, 0)),
            _ => (Start: new WPoint(1, 0), End: new WPoint(0, 0))
        };
        var brush = new LinearGradientBrush
        {
            StartPoint = points.Start,
            EndPoint = points.End,
            Opacity = opacity
        };
        brush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
        brush.GradientStops.Add(new GradientStop(WColor.FromArgb(32, 235, 252, 252), 0.32));
        brush.GradientStops.Add(new GradientStop(WColor.FromArgb(92, 170, 232, 238), 0.72));
        brush.GradientStops.Add(new GradientStop(WColor.FromArgb(42, 8, 24, 28), 1.0));
        return brush;
    }

    private static void DrawEdgeLine(DrawingContext context, AblageDirection direction, WPen pen, double offset)
    {
        switch (direction)
        {
            case AblageDirection.Left:
                context.DrawLine(pen, new WPoint(offset, 0), new WPoint(offset, Height));
                break;
            case AblageDirection.Right:
                context.DrawLine(pen, new WPoint(Width + offset, 0), new WPoint(Width + offset, Height));
                break;
            case AblageDirection.Up:
                context.DrawLine(pen, new WPoint(0, offset), new WPoint(Width, offset));
                break;
            case AblageDirection.Down:
                context.DrawLine(pen, new WPoint(0, Height + offset), new WPoint(Width, Height + offset));
                break;
            default:
                context.DrawLine(pen, new WPoint(Width + offset, 0), new WPoint(Width + offset, Height));
                break;
        }
    }
}
