using System.Drawing.Imaging;

namespace RKWorkspace.Shell.LivingLens.Windows;

public static class LivingLensFrameExporter
{
    private static readonly Size FrameSize = new(720, 420);

    public static string DefaultOutputDirectory => Path.Combine(FindRepositoryRoot(), "Docs", "VisualTargets", "MA00610R");

    public static bool ExportDefaultFrames()
    {
        Directory.CreateDirectory(DefaultOutputDirectory);

        ExportVariant("real_bubble_lens.png", LivingLensVariantKind.RealBubble);
        ExportVariant("glass_lens.png", LivingLensVariantKind.Glass);
        ExportVariant("water_surface_lens.png", LivingLensVariantKind.WaterSurface);
        ExportVariant("wormhole_lens.png", LivingLensVariantKind.Wormhole);
        ExportVariant("gravity_lens.png", LivingLensVariantKind.Gravity);
        ExportSingle("thing_in_hand.png", LivingLensVariantKind.RealBubble, 1f, 0f, 0f, 0f, drawThing: true, drawGhost: false);

        ExportSequence(
            "lens_appearance_sequence.png",
            [
                State(LivingLensVariantKind.RealBubble, 0.18f, 0f, 0f, 0f),
                State(LivingLensVariantKind.RealBubble, 0.42f, 0f, 0f, 0f),
                State(LivingLensVariantKind.RealBubble, 0.72f, 0f, 0f, 0f),
                State(LivingLensVariantKind.RealBubble, 1f, 0.08f, 0f, 0f)
            ]);

        ExportSequence(
            "lens_opening_sequence.png",
            [
                State(LivingLensVariantKind.Glass, 1f, 0.16f, 0f, 0f),
                State(LivingLensVariantKind.Glass, 1f, 0.42f, 0f, 0f),
                State(LivingLensVariantKind.Glass, 1f, 0.72f, 0f, 0f),
                State(LivingLensVariantKind.Glass, 1f, 1f, 0f, 0f)
            ]);

        ExportSequence(
            "lens_absorption_sequence.png",
            [
                State(LivingLensVariantKind.Wormhole, 1f, 1f, 0.08f, 0f),
                State(LivingLensVariantKind.Wormhole, 1f, 1f, 0.32f, 0f),
                State(LivingLensVariantKind.Wormhole, 1f, 1f, 0.58f, 0.22f),
                State(LivingLensVariantKind.Wormhole, 1f, 1f, 0.84f, 0.66f)
            ]);

        ExportSequence(
            "target_emergence_sequence.png",
            [
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.54f, 0.08f),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.68f, 0.34f),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.82f, 0.66f),
                State(LivingLensVariantKind.Gravity, 1f, 1f, 0.96f, 0.92f)
            ]);

        var required = new[]
        {
            "real_bubble_lens.png",
            "glass_lens.png",
            "water_surface_lens.png",
            "wormhole_lens.png",
            "gravity_lens.png",
            "lens_appearance_sequence.png",
            "lens_opening_sequence.png",
            "lens_absorption_sequence.png",
            "target_emergence_sequence.png",
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
        ExportSingle(fileName, variant, 1f, 0.82f, 0f, 0f, drawThing: false, drawGhost: false);
    }

    private static void ExportSingle(
        string fileName,
        LivingLensVariantKind variant,
        float emergence,
        float open,
        float absorption,
        float ghost,
        bool drawThing,
        bool drawGhost)
    {
        using var bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
        using var graphics = Graphics.FromImage(bitmap);
        LivingLensRenderer.DrawScene(graphics, FrameSize, State(variant, emergence, open, absorption, ghost) with
        {
            DrawThing = drawThing,
            DrawGhost = drawGhost
        });
        bitmap.Save(Path.Combine(DefaultOutputDirectory, fileName), ImageFormat.Png);
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
        float ghost)
    {
        return new LivingLensRenderState
        {
            Variant = variant,
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
