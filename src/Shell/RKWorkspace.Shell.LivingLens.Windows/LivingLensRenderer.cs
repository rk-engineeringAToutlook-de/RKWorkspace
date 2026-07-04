using System.Drawing.Drawing2D;

namespace RKWorkspace.Shell.LivingLens.Windows;

public static class LivingLensRenderer
{
    private static readonly SizeF BaseThingSize = new(178, 94);

    public static RectangleF EstimateThingBounds(PointF center, float absorptionProgress)
    {
        var scale = Math.Clamp(1f - (absorptionProgress * 0.54f), 0.38f, 1f);
        var width = BaseThingSize.Width * scale;
        var height = BaseThingSize.Height * scale;
        return new RectangleF(center.X - (width / 2f), center.Y - (height / 2f), width, height);
    }

    public static void DrawScene(Graphics graphics, Size canvasSize, LivingLensRenderState state)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        if (state.DrawTestBackground)
        {
            DrawTestBackground(graphics, canvasSize);
        }

        if (state.DrawLens && state.EmergenceProgress > 0)
        {
            DrawLivingLens(graphics, state);
        }

        if (state.DrawGhost || state.TargetEmergenceProgress > 0)
        {
            DrawTargetGhost(graphics, state);
        }

        if (state.DrawThing)
        {
            DrawDigitalThing(graphics, state);
        }
    }

    private static void DrawTestBackground(Graphics graphics, Size canvasSize)
    {
        using var background = new LinearGradientBrush(
            new Rectangle(Point.Empty, canvasSize),
            Color.FromArgb(242, 246, 245),
            Color.FromArgb(214, 224, 226),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(background, new Rectangle(Point.Empty, canvasSize));

        using var surfaceLine = new Pen(Color.FromArgb(32, 52, 64, 70), 1f);
        for (var y = 60; y < canvasSize.Height; y += 72)
        {
            graphics.DrawLine(surfaceLine, 0, y, canvasSize.Width, y + 16);
        }

        for (var x = 80; x < canvasSize.Width; x += 96)
        {
            graphics.DrawLine(surfaceLine, x, 0, x - 22, canvasSize.Height);
        }
    }

    private static void DrawLivingLens(Graphics graphics, LivingLensRenderState state)
    {
        var emergence = EaseOut(state.EmergenceProgress);
        var open = EaseOut(state.OpenProgress);
        var absorption = EaseOut(state.AbsorptionProgress);
        var pulse = MathF.Sin(state.Phase * 0.72f) * 0.018f;
        var width = 164f * (0.42f + (0.58f * emergence)) * (1f + pulse + (open * 0.08f));
        var height = 118f * (0.42f + (0.58f * emergence)) * (1f - (pulse * 0.6f) + (open * 0.04f));
        if (state.Variant == LivingLensVariantKind.Gravity)
        {
            width *= 1.18f;
            height *= 0.86f;
        }

        var lensBounds = new RectangleF(
            state.LensCenter.X - (width / 2f),
            state.LensCenter.Y - (height / 2f),
            width,
            height);

        DrawLensAmbient(graphics, lensBounds, state, emergence, open);

        switch (state.Variant)
        {
            case LivingLensVariantKind.RealBubble:
                DrawRealBubbleLens(graphics, lensBounds, state, emergence, open, absorption);
                break;
            case LivingLensVariantKind.Glass:
                DrawGlassLens(graphics, lensBounds, state, emergence, open, absorption);
                break;
            case LivingLensVariantKind.WaterSurface:
                DrawWaterSurfaceLens(graphics, lensBounds, state, emergence, open, absorption);
                break;
            case LivingLensVariantKind.Wormhole:
                DrawWormholeLens(graphics, lensBounds, state, emergence, open, absorption);
                break;
            case LivingLensVariantKind.Gravity:
                DrawGravityLens(graphics, lensBounds, state, emergence, open, absorption);
                break;
        }

        if (open > 0.45f)
        {
            DrawMiniAblage(graphics, lensBounds, open, state.TargetEmergenceProgress);
        }

        DrawLensText(graphics, lensBounds, state);
    }

    private static void DrawLensAmbient(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open)
    {
        var ambientBounds = Inflate(bounds, 42f + (open * 24f), 30f + (open * 18f));
        using var path = CreateOrganicOval(ambientBounds, state.Phase, 0.018f);
        using var glow = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(10 + (open * 12), emergence), 248, 250, 248),
            SurroundColors = [Color.FromArgb(0, 248, 250, 248)]
        };
        graphics.FillPath(glow, path);

        var ground = new RectangleF(
            bounds.X + (bounds.Width * 0.12f),
            bounds.Bottom - (bounds.Height * 0.10f),
            bounds.Width * 0.76f,
            bounds.Height * 0.18f);
        using var shadow = new LinearGradientBrush(
            Rectangle.Round(ground),
            Color.FromArgb(Alpha(58 + (open * 24), emergence), 8, 12, 14),
            Color.FromArgb(0, 8, 12, 14),
            LinearGradientMode.Vertical);
        graphics.FillEllipse(shadow, ground);
    }

    private static void DrawRealBubbleLens(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open, float absorption)
    {
        using var path = CreateOrganicOval(bounds, state.Phase, 0.026f);
        using var material = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(24 + (open * 18), emergence), 250, 252, 250),
            SurroundColors = [Color.FromArgb(Alpha(72 + (open * 28), emergence), 132, 142, 142)],
            FocusScales = new PointF(0.44f, 0.32f)
        };
        graphics.FillPath(material, path);
        DrawRefraction(graphics, bounds, path, state.Phase, emergence, 0.62f);
        DrawFineLightEdge(graphics, bounds, path, emergence, open, Color.FromArgb(250, 255, 255));
        DrawBubbleHighlights(graphics, bounds, emergence, open);
        DrawSoftAperture(graphics, bounds, emergence, open, absorption, 0.34f);
    }

    private static void DrawGlassLens(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open, float absorption)
    {
        using var path = CreateOrganicOval(bounds, state.Phase, 0.012f);
        using var material = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(42 + (open * 18), emergence), 246, 248, 246),
            SurroundColors = [Color.FromArgb(Alpha(96 + (open * 28), emergence), 104, 112, 112)],
            FocusScales = new PointF(0.34f, 0.24f)
        };
        graphics.FillPath(material, path);
        DrawRefraction(graphics, bounds, path, state.Phase, emergence, 0.9f);
        DrawFineLightEdge(graphics, bounds, path, emergence, open, Color.FromArgb(246, 252, 255));
        DrawGlassHighlights(graphics, bounds, emergence, open);
        DrawSoftAperture(graphics, bounds, emergence, open, absorption, 0.44f);
    }

    private static void DrawWaterSurfaceLens(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open, float absorption)
    {
        using var path = CreateOrganicOval(bounds, state.Phase, 0.02f);
        using var material = new LinearGradientBrush(
            Rectangle.Round(bounds),
            Color.FromArgb(Alpha(76, emergence), 216, 236, 244),
            Color.FromArgb(Alpha(32 + (open * 30), emergence), 244, 250, 252),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillPath(material, path);
        DrawRefraction(graphics, bounds, path, state.Phase, emergence, 1.1f);
        DrawWaterWaves(graphics, bounds, path, state.Phase, emergence, open);
        DrawFineLightEdge(graphics, bounds, path, emergence, open, Color.FromArgb(238, 250, 255));
        DrawSoftAperture(graphics, bounds, emergence, open, absorption, 0.38f);
    }

    private static void DrawWormholeLens(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open, float absorption)
    {
        using var path = CreateOrganicOval(bounds, state.Phase, 0.014f);
        using var material = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(102 + (open * 70), emergence), 8, 12, 18),
            SurroundColors = [Color.FromArgb(Alpha(96, emergence), 118, 150, 162)],
            FocusScales = new PointF(0.30f, 0.22f)
        };
        graphics.FillPath(material, path);
        DrawDepthRings(graphics, bounds, emergence, open, absorption, 0.92f);
        DrawFineLightEdge(graphics, bounds, path, emergence, open, Color.FromArgb(222, 246, 255));
        DrawSoftAperture(graphics, bounds, emergence, open, absorption, 0.76f);
    }

    private static void DrawGravityLens(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float emergence, float open, float absorption)
    {
        using var path = CreateOrganicOval(bounds, state.Phase, 0.008f);
        using var material = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(20 + (open * 24), emergence), 220, 236, 242),
            SurroundColors = [Color.FromArgb(Alpha(44 + (open * 28), emergence), 112, 132, 140)]
        };
        graphics.FillPath(material, path);
        DrawDepthRings(graphics, bounds, emergence, open, absorption, 0.52f);
        DrawRefraction(graphics, bounds, path, state.Phase, emergence, 0.72f);
        DrawFineLightEdge(graphics, bounds, path, emergence, open, Color.FromArgb(244, 252, 255));
        DrawSoftAperture(graphics, bounds, emergence, open, absorption, 0.24f);
    }

    private static void DrawRefraction(Graphics graphics, RectangleF bounds, GraphicsPath clipPath, float phase, float emergence, float intensity)
    {
        var previousClip = graphics.Clip;
        graphics.SetClip(clipPath, CombineMode.Intersect);
        using var light = new Pen(Color.FromArgb(Alpha(42 * intensity, emergence), 250, 255, 255), 0.9f);
        using var shade = new Pen(Color.FromArgb(Alpha(24 * intensity, emergence), 24, 36, 42), 0.8f);

        for (var index = -2; index <= 2; index++)
        {
            var y = bounds.Y + (bounds.Height * 0.50f) + (index * bounds.Height * 0.10f) + (MathF.Sin(phase + index) * 2.2f * intensity);
            graphics.DrawBezier(
                light,
                bounds.Left + 8,
                y,
                bounds.Left + (bounds.Width * 0.34f),
                y - (10 * intensity),
                bounds.Right - (bounds.Width * 0.30f),
                y + (9 * intensity),
                bounds.Right - 8,
                y - 2);
        }

        graphics.DrawBezier(
            shade,
            bounds.Left + (bounds.Width * 0.16f),
            bounds.Top + (bounds.Height * 0.26f),
            bounds.Left + (bounds.Width * 0.44f),
            bounds.Top + (bounds.Height * 0.12f),
            bounds.Right - (bounds.Width * 0.28f),
            bounds.Bottom - (bounds.Height * 0.18f),
            bounds.Right - (bounds.Width * 0.10f),
            bounds.Bottom - (bounds.Height * 0.25f));

        graphics.Clip = previousClip;
        previousClip.Dispose();
    }

    private static void DrawWaterWaves(Graphics graphics, RectangleF bounds, GraphicsPath clipPath, float phase, float emergence, float open)
    {
        var previousClip = graphics.Clip;
        graphics.SetClip(clipPath, CombineMode.Intersect);
        using var wave = new Pen(Color.FromArgb(Alpha(44 + (open * 30), emergence), 250, 255, 255), 1.0f);
        for (var index = -1; index <= 1; index++)
        {
            var y = bounds.Y + (bounds.Height * (0.44f + (index * 0.12f))) + (MathF.Sin(phase * 0.7f + index) * 2.0f);
            graphics.DrawBezier(
                wave,
                bounds.Left + 10,
                y,
                bounds.Left + (bounds.Width * 0.30f),
                y - 8,
                bounds.Right - (bounds.Width * 0.32f),
                y + 8,
                bounds.Right - 10,
                y);
        }

        graphics.Clip = previousClip;
        previousClip.Dispose();
    }

    private static void DrawDepthRings(Graphics graphics, RectangleF bounds, float emergence, float open, float absorption, float intensity)
    {
        var center = new PointF(bounds.X + (bounds.Width * 0.52f), bounds.Y + (bounds.Height * 0.54f));
        var count = 6;
        for (var index = 0; index < count; index++)
        {
            var factor = 0.78f - (index * 0.098f) - (absorption * 0.035f);
            var width = bounds.Width * factor;
            var height = bounds.Height * factor * 0.58f;
            var ring = new RectangleF(center.X - (width / 2f), center.Y - (height / 2f) + (index * 2.2f), width, height);
            using var pen = new Pen(Color.FromArgb(Alpha((44 + (open * 42)) * intensity * (1f - (index * 0.08f)), emergence), 224, 244, 252), index == 0 ? 1.1f : 0.8f);
            graphics.DrawEllipse(pen, ring);
        }
    }

    private static void DrawFineLightEdge(Graphics graphics, RectangleF bounds, GraphicsPath path, float emergence, float open, Color color)
    {
        using var upper = new Pen(Color.FromArgb(Alpha(170 + (open * 56), emergence), color), 1.4f + open);
        using var lower = new Pen(Color.FromArgb(Alpha(86 + (open * 34), emergence), 20, 28, 32), 1.0f);
        graphics.DrawArc(upper, bounds, 198, 148);
        graphics.DrawArc(lower, bounds, 24, 154);
        using var inner = new Pen(Color.FromArgb(Alpha(54 + (open * 32), emergence), 250, 255, 255), 0.8f);
        graphics.DrawPath(inner, path);
    }

    private static void DrawBubbleHighlights(Graphics graphics, RectangleF bounds, float emergence, float open)
    {
        using var main = new LinearGradientBrush(
            Rectangle.Round(new RectangleF(bounds.X + (bounds.Width * 0.53f), bounds.Y + (bounds.Height * 0.14f), bounds.Width * 0.28f, bounds.Height * 0.30f)),
            Color.FromArgb(Alpha(116 + (open * 24), emergence), 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillEllipse(main, bounds.X + (bounds.Width * 0.53f), bounds.Y + (bounds.Height * 0.14f), bounds.Width * 0.28f, bounds.Height * 0.30f);

        using var small = new SolidBrush(Color.FromArgb(Alpha(48, emergence), 255, 255, 255));
        graphics.FillEllipse(small, bounds.X + (bounds.Width * 0.18f), bounds.Y + (bounds.Height * 0.34f), bounds.Width * 0.16f, bounds.Height * 0.10f);
    }

    private static void DrawGlassHighlights(Graphics graphics, RectangleF bounds, float emergence, float open)
    {
        using var main = new LinearGradientBrush(
            Rectangle.Round(new RectangleF(bounds.X + (bounds.Width * 0.50f), bounds.Y + (bounds.Height * 0.12f), bounds.Width * 0.34f, bounds.Height * 0.36f)),
            Color.FromArgb(Alpha(132 + (open * 24), emergence), 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillEllipse(main, bounds.X + (bounds.Width * 0.50f), bounds.Y + (bounds.Height * 0.12f), bounds.Width * 0.34f, bounds.Height * 0.36f);
    }

    private static void DrawSoftAperture(Graphics graphics, RectangleF bounds, float emergence, float open, float absorption, float baseStrength)
    {
        var strength = Math.Clamp(baseStrength + (open * 0.58f) + (absorption * 0.44f), 0f, 1.35f);
        if (strength <= 0.08f)
        {
            return;
        }

        var width = bounds.Width * (0.18f + (0.25f * Math.Min(strength, 1f)));
        var height = bounds.Height * (0.14f + (0.22f * Math.Min(strength, 1f)));
        var aperture = new RectangleF(
            bounds.X + ((bounds.Width - width) / 2f),
            bounds.Y + ((bounds.Height - height) / 2f) + (bounds.Height * 0.06f),
            width,
            height);
        using var path = new GraphicsPath();
        path.AddEllipse(aperture);
        using var brush = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(154 * strength, emergence), 6, 10, 14),
            SurroundColors = [Color.FromArgb(Alpha(70 * strength, emergence), 80, 108, 120)],
            FocusScales = new PointF(0.42f, 0.28f)
        };
        graphics.FillPath(brush, path);
        using var edge = new Pen(Color.FromArgb(Alpha(160 * strength, emergence), 220, 244, 252), Math.Max(0.9f, 1.2f * strength));
        graphics.DrawArc(edge, aperture, 202, 136);
    }

    private static void DrawMiniAblage(Graphics graphics, RectangleF lensBounds, float open, float targetProgress)
    {
        var topY = lensBounds.Y + (lensBounds.Height * 0.55f);
        var bottomY = lensBounds.Y + (lensBounds.Height * 0.74f);
        var leftTop = lensBounds.X + (lensBounds.Width * 0.38f);
        var rightTop = lensBounds.X + (lensBounds.Width * 0.62f);
        var leftBottom = lensBounds.X + (lensBounds.Width * 0.25f);
        var rightBottom = lensBounds.X + (lensBounds.Width * 0.75f);
        var points = new[]
        {
            new PointF(leftTop, topY),
            new PointF(rightTop, topY),
            new PointF(rightBottom, bottomY),
            new PointF(leftBottom, bottomY)
        };
        using var path = new GraphicsPath();
        path.AddPolygon(points);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(Alpha(80 + (open * 42), 1f), 222, 236, 232),
            SurroundColors = [Color.FromArgb(Alpha(34 + (open * 22), 1f), 88, 116, 124)]
        };
        using var border = new Pen(Color.FromArgb(Alpha(126, open), 230, 246, 242), 0.9f);
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);

        if (targetProgress > 0)
        {
            var ghostWidth = (rightTop - leftTop) * (0.34f + (targetProgress * 0.36f));
            var ghostHeight = (bottomY - topY) * (0.26f + (targetProgress * 0.24f));
            var ghost = new RectangleF(
                leftTop + ((rightTop - leftTop - ghostWidth) / 2f),
                topY + ((bottomY - topY - ghostHeight) / 2f),
                ghostWidth,
                ghostHeight);
            using var ghostPath = RoundedRectangle(ghost, 5f);
            using var ghostBrush = new SolidBrush(Color.FromArgb(Alpha(42 + (targetProgress * 128), 1f), 255, 255, 255));
            using var ghostBorder = new Pen(Color.FromArgb(Alpha(70 + (targetProgress * 70), 1f), 178, 214, 226), 0.8f);
            graphics.FillPath(ghostBrush, ghostPath);
            graphics.DrawPath(ghostBorder, ghostPath);
        }
    }

    private static void DrawLensText(Graphics graphics, RectangleF lensBounds, LivingLensRenderState state)
    {
        if (state.NameVisibility == LivingLensNameVisibility.Hidden)
        {
            return;
        }

        var alpha = state.NameVisibility == LivingLensNameVisibility.MicroText ? 58 : 190;
        var text = state.NameVisibility == LivingLensNameVisibility.ActionReadable
            ? $"{state.LensLabel}\nHier ablegen"
            : state.LensLabel;
        using var font = new Font(FontFamily.GenericSansSerif, state.NameVisibility == LivingLensNameVisibility.MicroText ? 6.5f : 8.0f, FontStyle.Regular);
        TextRenderer.DrawText(
            graphics,
            text,
            font,
            Rectangle.Round(new RectangleF(lensBounds.X - 54, lensBounds.Bottom + 3, lensBounds.Width + 108, 34)),
            Color.FromArgb(alpha, 244, 248, 246),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawDigitalThing(Graphics graphics, LivingLensRenderState state)
    {
        var progress = EaseOut(state.AbsorptionProgress);
        if (progress >= 1f && state.TargetEmergenceProgress >= 0.95f)
        {
            return;
        }

        var center = AbsorbedThingCenter(state.ThingCenter, state.LensCenter, progress);
        var scale = (state.ThingCompact ? 0.88f : 1.0f) * Math.Clamp(1f - (progress * 0.62f), 0.34f, 1f);
        var width = BaseThingSize.Width * scale;
        var height = BaseThingSize.Height * scale * (1f - (progress * 0.16f));
        var bounds = new RectangleF(center.X - (width / 2f), center.Y - (height / 2f), width, height);
        var distortion = progress * 0.72f;
        var opacity = progress < 0.78f ? 1f : Math.Clamp(1f - ((progress - 0.78f) / 0.22f), 0.12f, 1f);

        var previous = graphics.Transform;
        graphics.TranslateTransform(center.X, center.Y);
        graphics.RotateTransform(state.TiltX * 0.42f);
        graphics.TranslateTransform(-center.X, -center.Y);

        DrawThingShadow(graphics, bounds, state, progress, opacity);
        using var thingPath = CreateThingPath(bounds, state.LensCenter, distortion, state.TiltX, state.TiltY);
        using var thingFill = new LinearGradientBrush(
            Rectangle.Round(bounds),
            Color.FromArgb(Alpha(238, opacity), 250, 250, 244),
            Color.FromArgb(Alpha(210, opacity), 212, 226, 230),
            LinearGradientMode.Vertical);
        using var thingEdge = new Pen(Color.FromArgb(Alpha(112, opacity), 106, 126, 134), 0.9f);
        graphics.FillPath(thingFill, thingPath);
        graphics.DrawPath(thingEdge, thingPath);

        if (state.ThingPartiallyOccluded || progress > 0)
        {
            DrawDigitalGrip(graphics, thingPath, bounds, opacity, progress);
        }

        using var titleFont = new Font(FontFamily.GenericSansSerif, Math.Max(7f, 11f * scale), FontStyle.Bold);
        var textBounds = Rectangle.Round(new RectangleF(bounds.X + (bounds.Width * 0.14f), bounds.Y + (bounds.Height * 0.35f), bounds.Width * 0.72f, bounds.Height * 0.28f));
        TextRenderer.DrawText(
            graphics,
            "Rechnung.pdf",
            titleFont,
            textBounds,
            Color.FromArgb(Alpha(48, opacity), 42, 52, 58),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        graphics.Transform = previous;
        previous.Dispose();
    }

    private static void DrawThingShadow(Graphics graphics, RectangleF bounds, LivingLensRenderState state, float progress, float opacity)
    {
        var shadow = new RectangleF(
            bounds.X + state.ShadowX + (progress * 8f),
            bounds.Y + state.ShadowY - (progress * 4f),
            bounds.Width * (1f - (progress * 0.18f)),
            bounds.Height * (0.30f - (progress * 0.08f)));
        using var brush = new SolidBrush(Color.FromArgb(Alpha(state.GripShadowVisible ? 86 : 42, opacity), 0, 0, 0));
        graphics.FillEllipse(brush, shadow);
    }

    private static void DrawDigitalGrip(Graphics graphics, GraphicsPath thingPath, RectangleF bounds, float opacity, float progress)
    {
        var gripBounds = new RectangleF(bounds.X - (bounds.Width * 0.08f), bounds.Y - 4, bounds.Width * (0.48f + (progress * 0.18f)), bounds.Height + 8);
        var previousClip = graphics.Clip;
        graphics.SetClip(thingPath, CombineMode.Intersect);
        using var grip = new LinearGradientBrush(
            Rectangle.Round(gripBounds),
            Color.FromArgb(Alpha(52 + (progress * 42), opacity), 22, 26, 26),
            Color.FromArgb(0, 20, 28, 30),
            LinearGradientMode.Horizontal);
        graphics.FillEllipse(grip, gripBounds);
        graphics.Clip = previousClip;
        previousClip.Dispose();
    }

    private static void DrawTargetGhost(Graphics graphics, LivingLensRenderState state)
    {
        var progress = EaseOut(Math.Max(state.TargetEmergenceProgress, state.AbsorptionProgress > 0.52f ? (state.AbsorptionProgress - 0.52f) / 0.48f : 0f));
        if (progress <= 0)
        {
            return;
        }

        var offset = new PointF(72f, 46f);
        var center = new PointF(
            state.LensCenter.X + (offset.X * progress),
            state.LensCenter.Y + (offset.Y * progress));
        var width = BaseThingSize.Width * (0.22f + (progress * 0.64f));
        var height = BaseThingSize.Height * (0.20f + (progress * 0.62f));
        var bounds = new RectangleF(center.X - (width / 2f), center.Y - (height / 2f), width, height);
        using var path = CreateThingPath(bounds, state.LensCenter, Math.Max(0, 0.36f - (progress * 0.36f)), 0f, 0f);
        using var fill = new SolidBrush(Color.FromArgb(Alpha(36 + (progress * 138), 1f), 250, 252, 250));
        using var edge = new Pen(Color.FromArgb(Alpha(44 + (progress * 78), 1f), 168, 202, 214), 0.9f);
        graphics.FillPath(fill, path);
        graphics.DrawPath(edge, path);
    }

    private static PointF AbsorbedThingCenter(PointF source, PointF lens, float progress)
    {
        var pull = Math.Clamp(progress * 0.86f, 0f, 0.86f);
        return new PointF(
            source.X + ((lens.X - source.X) * pull),
            source.Y + ((lens.Y - source.Y) * pull));
    }

    private static GraphicsPath CreateThingPath(RectangleF bounds, PointF lensCenter, float distortion, float tiltX, float tiltY)
    {
        var pullRight = lensCenter.X >= bounds.X + (bounds.Width / 2f);
        var frontPull = bounds.Width * 0.18f * distortion;
        var backLag = bounds.Width * 0.05f * distortion;
        var topCurve = bounds.Height * 0.10f * distortion;
        var bottomCurve = bounds.Height * 0.12f * distortion;
        var left = bounds.Left + (pullRight ? backLag : -frontPull);
        var right = bounds.Right + (pullRight ? frontPull : -backLag);
        var perspectiveX = Math.Clamp(tiltX, -4f, 4f) * bounds.Width * 0.014f;
        var perspectiveY = Math.Clamp(tiltY, -4f, 4f) * bounds.Height * 0.020f;
        var topLeft = new PointF(left + 12 - (perspectiveX * 0.40f), bounds.Top + topCurve - (perspectiveY * 0.62f));
        var topRight = new PointF(right - 12 - (perspectiveX * 0.18f), bounds.Top + topCurve + (perspectiveY * 0.22f));
        var bottomRight = new PointF(right - 10 + (perspectiveX * 0.62f), bounds.Bottom - bottomCurve + (perspectiveY * 0.72f));
        var bottomLeft = new PointF(left + 12 + (perspectiveX * 0.16f), bounds.Bottom - bottomCurve - (perspectiveY * 0.24f));
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddBezier(topLeft, new PointF(bounds.Left + bounds.Width * 0.32f, bounds.Top - topCurve - (perspectiveY * 0.48f)), new PointF(bounds.Left + bounds.Width * 0.68f, bounds.Top + topCurve + (perspectiveY * 0.18f)), topRight);
        path.AddBezier(topRight, new PointF(right + (frontPull * 0.34f) + (perspectiveX * 0.24f), bounds.Top + bounds.Height * 0.42f), new PointF(right + (frontPull * 0.22f) + (perspectiveX * 0.34f), bounds.Bottom - 18), bottomRight);
        path.AddBezier(bottomRight, new PointF(right - (bounds.Width * 0.28f), bounds.Bottom + bottomCurve + (perspectiveY * 0.54f)), new PointF(bounds.Left + (bounds.Width * 0.34f), bounds.Bottom - bottomCurve - (perspectiveY * 0.22f)), bottomLeft);
        path.AddBezier(bottomLeft, new PointF(left - (backLag * 0.4f) - (perspectiveX * 0.18f), bounds.Top + bounds.Height * 0.55f), new PointF(left - (backLag * 0.2f) - (perspectiveX * 0.14f), bounds.Top + 18), topLeft);
        path.CloseFigure();
        return path;
    }

    private static GraphicsPath CreateOrganicOval(RectangleF bounds, float phase, float wobble)
    {
        var x = bounds.X;
        var y = bounds.Y;
        var w = bounds.Width;
        var h = bounds.Height;
        var k = 0.5522848f;
        var dx = w * wobble;
        var dy = h * wobble;
        var s1 = MathF.Sin(phase) * dx;
        var s2 = MathF.Sin(phase + 1.7f) * dy;
        var s3 = MathF.Sin(phase + 2.8f) * dx;
        var s4 = MathF.Sin(phase + 4.1f) * dy;
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddBezier(
            x + (w / 2f) + s1,
            y + s2,
            x + (w / 2f) + (w * k / 2f),
            y - s2,
            x + w + s3,
            y + (h / 2f) - (h * k / 2f),
            x + w + s3,
            y + (h / 2f));
        path.AddBezier(
            x + w + s3,
            y + (h / 2f),
            x + w - s3,
            y + (h / 2f) + (h * k / 2f),
            x + (w / 2f) + (w * k / 2f),
            y + h + s4,
            x + (w / 2f),
            y + h + s4);
        path.AddBezier(
            x + (w / 2f),
            y + h + s4,
            x + (w / 2f) - (w * k / 2f),
            y + h - s4,
            x - s1,
            y + (h / 2f) + (h * k / 2f),
            x - s1,
            y + (h / 2f));
        path.AddBezier(
            x - s1,
            y + (h / 2f),
            x + s1,
            y + (h / 2f) - (h * k / 2f),
            x + (w / 2f) - (w * k / 2f),
            y + s2,
            x + (w / 2f) + s1,
            y + s2);
        path.CloseFigure();
        return path;
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

    private static RectangleF Inflate(RectangleF bounds, float x, float y)
    {
        return new RectangleF(bounds.X - x, bounds.Y - y, bounds.Width + (x * 2f), bounds.Height + (y * 2f));
    }

    private static float EaseOut(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return 1f - MathF.Pow(1f - clamped, 3f);
    }

    private static int Alpha(float value, float multiplier)
    {
        return Math.Clamp((int)Math.Round(value * Math.Clamp(multiplier, 0f, 1f)), 0, 255);
    }
}
