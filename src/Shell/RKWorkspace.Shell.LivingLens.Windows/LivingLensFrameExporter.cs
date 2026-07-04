using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace RKWorkspace.Shell.LivingLens.Windows;

public static class LivingLensFrameExporter
{
    private static readonly Size FrameSize = new(720, 420);

    public static string DefaultOutputDirectory => Path.Combine(FindRepositoryRoot(), "Docs", "VisualTargets", "MA00612");

    public static bool ExportDefaultFrames()
    {
        Directory.CreateDirectory(DefaultOutputDirectory);

        ExportVariant("extreme_glass_bubble.png", LivingLensVariantKind.RealBubble);
        ExportVariant("extreme_water_lens.png", LivingLensVariantKind.Glass);
        ExportVariant("extreme_wormhole_tunnel.png", LivingLensVariantKind.WaterSurface);
        ExportVariant("extreme_gravity_well.png", LivingLensVariantKind.Wormhole);
        ExportVariant("extreme_portal_absorption.png", LivingLensVariantKind.Gravity);
        ExportSingle("thing_in_hand.png", LivingLensVariantKind.RealBubble, 1f, 0f, 0f, 0f, drawThing: true, drawGhost: false, intensity: 4);

        ExportSequence(
            "lens_appearance_sequence.png",
            [
                State(LivingLensVariantKind.RealBubble, 0.18f, 0f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.RealBubble, 0.42f, 0f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.RealBubble, 0.72f, 0f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.RealBubble, 1f, 0.08f, 0f, 0f, 4, 1200)
            ]);

        ExportSequence(
            "lens_opening_sequence.png",
            [
                State(LivingLensVariantKind.Glass, 1f, 0.16f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.Glass, 1f, 0.42f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.Glass, 1f, 0.72f, 0f, 0f, 4, 1200),
                State(LivingLensVariantKind.Glass, 1f, 1f, 0f, 0f, 4, 1200)
            ]);

        ExportAbsorptionSequence("absorption_sequence_600ms.png", 600);
        ExportAbsorptionSequence("absorption_sequence_1200ms.png", 1200);
        ExportAbsorptionSequence("absorption_sequence_1800ms.png", 1800);
        ExportAbsorptionSequence("absorption_sequence_2400ms.png", 2400);

        ExportSequence(
            "target_emergence_sequence.png",
            [
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.54f, 0.08f, 5, 1800),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.68f, 0.34f, 5, 1800),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.82f, 0.66f, 5, 1800),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.96f, 0.92f, 5, 1800)
            ]);

        ExportSingle("target_ghost_emergence.png", LivingLensVariantKind.Gravity, 1f, 1f, 0.72f, 0.66f, drawThing: true, drawGhost: true, intensity: 5);
        ExportMobileFrame("mobile_spatial_surface.png", active: false, open: false);
        ExportMobileFrame("mobile_lens_gesture_active.png", active: true, open: false);
        ExportMobileFrame("mobile_tunnel_open.png", active: true, open: true);

        var required = new[]
        {
            "extreme_glass_bubble.png",
            "extreme_water_lens.png",
            "extreme_wormhole_tunnel.png",
            "extreme_gravity_well.png",
            "extreme_portal_absorption.png",
            "absorption_sequence_600ms.png",
            "absorption_sequence_1200ms.png",
            "absorption_sequence_1800ms.png",
            "absorption_sequence_2400ms.png",
            "target_emergence_sequence.png",
            "target_ghost_emergence.png",
            "mobile_spatial_surface.png",
            "mobile_lens_gesture_active.png",
            "mobile_tunnel_open.png",
            "thing_in_hand.png"
        };

        return required.All(file =>
        {
            var path = Path.Combine(DefaultOutputDirectory, file);
            return File.Exists(path) && new FileInfo(path).Length > 0;
        });
    }

    private static void ExportVariant(string fileName, LivingLensVariantKind variant)
    {
        ExportSingle(fileName, variant, 1f, 0.92f, variant == LivingLensVariantKind.Gravity ? 0.48f : 0f, 0f, drawThing: false, drawGhost: false, intensity: 5);
    }

    private static void ExportSingle(
        string fileName,
        LivingLensVariantKind variant,
        float emergence,
        float open,
        float absorption,
        float ghost,
        bool drawThing,
        bool drawGhost,
        int intensity)
    {
        using var bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
        using var graphics = Graphics.FromImage(bitmap);
        LivingLensRenderer.DrawScene(graphics, FrameSize, State(variant, emergence, open, absorption, ghost, intensity, 1200) with
        {
            DrawThing = drawThing,
            DrawGhost = drawGhost
        });
        bitmap.Save(Path.Combine(DefaultOutputDirectory, fileName), ImageFormat.Png);
    }

    private static void ExportAbsorptionSequence(string fileName, int timingMs)
    {
        ExportSequence(
            fileName,
            [
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.08f, 0f, 5, timingMs),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.32f, 0.02f, 5, timingMs),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.58f, 0.26f, 5, timingMs),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.84f, 0.74f, 5, timingMs)
            ]);
    }

    private static void ExportSequence(string fileName, IReadOnlyList<LivingLensRenderState> states)
    {
        using var bitmap = new Bitmap(FrameSize.Width * states.Count, FrameSize.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        for (var index = 0; index < states.Count; index++)
        {
            using var frame = new Bitmap(FrameSize.Width, FrameSize.Height);
            using (var frameGraphics = Graphics.FromImage(frame))
            {
                LivingLensRenderer.DrawScene(frameGraphics, FrameSize, states[index]);
            }

            graphics.DrawImageUnscaled(frame, index * FrameSize.Width, 0);
        }

        bitmap.Save(Path.Combine(DefaultOutputDirectory, fileName), ImageFormat.Png);
    }

    private static LivingLensRenderState State(
        LivingLensVariantKind variant,
        float emergence,
        float open,
        float absorption,
        float ghost,
        int intensity,
        int timingMs)
    {
        return new LivingLensRenderState
        {
            Variant = variant,
            ExtremeFxMode = true,
            EffectIntensity = intensity,
            AbsorptionDurationMs = timingMs,
            Phase = 1.2f + absorption,
            EmergenceProgress = emergence,
            OpenProgress = open,
            AbsorptionProgress = absorption,
            TargetEmergenceProgress = ghost,
            TiltX = 1.4f,
            TiltY = -1.1f,
            ShadowX = -2.2f,
            ShadowY = 20f,
            ThingCompact = true,
            ThingPartiallyOccluded = true,
            GripShadowVisible = true,
            DrawTestBackground = true,
            DrawThing = true,
            DrawLens = true,
            DrawGhost = ghost > 0,
            ThingCenter = new PointF(FrameSize.Width * 0.34f, FrameSize.Height * 0.55f),
            LensCenter = new PointF(FrameSize.Width * 0.76f, FrameSize.Height * 0.48f),
            LensLabel = "Ablage",
            NameVisibility = open > 0.78f ? LivingLensNameVisibility.ActionReadable : LivingLensNameVisibility.Hidden
        };
    }

    private static void ExportMobileFrame(string fileName, bool active, bool open)
    {
        using var bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var background = new LinearGradientBrush(
            new Rectangle(Point.Empty, FrameSize),
            Color.FromArgb(232, 232, 224),
            Color.FromArgb(210, 226, 222),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(background, new Rectangle(Point.Empty, FrameSize));

        var card = new RectangleF(FrameSize.Width * 0.36f, FrameSize.Height * 0.39f, 190, 86);
        using var cardPath = RoundedRectangle(card, 8f);
        using var cardFill = new SolidBrush(Color.FromArgb(230, 252, 252, 246));
        graphics.FillPath(cardFill, cardPath);

        if (!active)
        {
            bitmap.Save(Path.Combine(DefaultOutputDirectory, fileName), ImageFormat.Png);
            return;
        }

        DrawMobileLens(graphics, new PointF(FrameSize.Width * 0.84f, FrameSize.Height * 0.46f), 108f, open, "Ablage");
        DrawMobileLens(graphics, new PointF(FrameSize.Width * 0.16f, FrameSize.Height * 0.66f), 62f, false, string.Empty);
        DrawMobileLens(graphics, new PointF(FrameSize.Width * 0.68f, FrameSize.Height * 0.17f), 78f, false, "Ablage");
        bitmap.Save(Path.Combine(DefaultOutputDirectory, fileName), ImageFormat.Png);
    }

    private static void DrawMobileLens(Graphics graphics, PointF center, float size, bool open, string label)
    {
        var bounds = new RectangleF(center.X - (size / 2f), center.Y - (size / 2f), size, size * 0.78f);
        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var material = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(open ? 150 : 70, 246, 252, 250),
            SurroundColors = [Color.FromArgb(open ? 96 : 38, 92, 112, 118)],
            FocusScales = new PointF(0.34f, 0.22f)
        };
        graphics.FillPath(material, path);
        using var rim = new Pen(Color.FromArgb(open ? 170 : 90, 250, 255, 255), open ? 2.2f : 1.1f);
        graphics.DrawPath(rim, path);
        if (open)
        {
            using var throat = new SolidBrush(Color.FromArgb(128, 8, 12, 16));
            graphics.FillEllipse(throat, bounds.X + (bounds.Width * 0.34f), bounds.Y + (bounds.Height * 0.42f), bounds.Width * 0.32f, bounds.Height * 0.16f);
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            using var font = new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Regular);
            TextRenderer.DrawText(graphics, label, font, Rectangle.Round(new RectangleF(bounds.X - 20, bounds.Bottom + 4, bounds.Width + 40, 20)), Color.FromArgb(160, 32, 42, 44), TextFormatFlags.HorizontalCenter);
        }
    }

    private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2f;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "Docs")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
