using RKWorkspace.Shell;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WColor = System.Windows.Media.Color;
using WCursors = System.Windows.Input.Cursors;
using WPen = System.Windows.Media.Pen;
using WPoint = System.Windows.Point;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class GpuLivingLensSurface : FrameworkElement
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly System.Drawing.Rectangle _screenBounds;
    private readonly GpuLivingLensSession _session = new();
    private readonly StopwatchClock _clock = new();
    private readonly Dictionary<string, BitmapSource> _lensSamples = new();
    private WPoint _thingCenter;
    private WPoint _targetCenter;
    private WPoint _lastTargetCenter;
    private WPoint _activeLensCenter;
    private Vector _velocity;
    private Vector _grabOffset;
    private GpuLivingLensLook _lensLook = GpuLivingLensLook.Hybrid;
    private bool _isHolding;
    private int _sampleFrame;
    private double _phase;

    public GpuLivingLensSurface(WorkspaceShellRuntime runtime, System.Drawing.Rectangle screenBounds)
    {
        _runtime = runtime;
        _screenBounds = screenBounds;
        Focusable = true;
        SnapsToDevicePixels = false;
        UseLayoutRounding = false;
        Cursor = WCursors.Arrow;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
        RenderOptions.SetCachingHint(this, CachingHint.Cache);
        _thingCenter = new WPoint(screenBounds.Width * 0.50, screenBounds.Height * 0.50);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _thingCenter;
        _activeLensCenter = new WPoint(screenBounds.Width - 88, screenBounds.Height * 0.50);
        _session.SetLensLook(_lensLook);
        WarmCleanDesktopPlates();
    }

    public GpuLivingLensSession Session => _session;

    public void SetLensLook(GpuLivingLensLook lensLook)
    {
        _lensLook = lensLook;
        _session.SetLensLook(lensLook);
        InvalidateVisual();
    }

    public void ActivatePickAt(WPoint point)
    {
        _thingCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
        _activeLensCenter = NearestLensCenter(point);
        _velocity = default;
        _grabOffset = default;
        _isHolding = true;
        CaptureMouse();
        Cursor = WCursors.SizeAll;
        _session.Pick();
        InvalidateVisual();
    }

    public void Tick()
    {
        var elapsed = _clock.TakeElapsedMilliseconds();
        _phase += elapsed / 1000.0;
        _session.Advance(elapsed);

        var delta = _targetCenter - _thingCenter;
        var spring = _isHolding ? 0.34 : 0.16;
        var damping = _isHolding ? 0.42 : 0.66;
        _velocity = new Vector(
            (_velocity.X * damping) + (delta.X * spring),
            (_velocity.Y * damping) + (delta.Y * spring));
        _thingCenter += _velocity;
        if (_isHolding && _velocity.Length > 0.05)
        {
            _session.Carry((float)(_velocity.X * 1.12), (float)(_velocity.Y * 1.12));
        }

        InvalidateVisual();
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
        var point = e.GetPosition(this);
        if (e.ChangedButton == MouseButton.Left && ThingBounds().Contains(point))
        {
            _isHolding = true;
            _grabOffset = point - _thingCenter;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.Pick();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            e.Handled = true;
            return;
        }

        var nearestLens = NearestLensCenter(point);
        if (e.ChangedButton == MouseButton.Left && IsNearLens(point, 230) && _session.CanPullOutFromLens)
        {
            _activeLensCenter = nearestLens;
            _thingCenter = _activeLensCenter;
            _targetCenter = point;
            _lastTargetCenter = point;
            _velocity = default;
            _grabOffset = default;
            _isHolding = true;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.PullOutFromLens();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        if (!_isHolding)
        {
            return;
        }

        var point = ProjectThroughScreenEdge(e.GetPosition(this));
        _targetCenter = point - _grabOffset;
        _lastTargetCenter = _targetCenter;
        _activeLensCenter = ResolveActiveLensCenter(_targetCenter);
        if (IsNearLens(_targetCenter, 210))
        {
            _session.ApproachLens((float)NormalizedLensNearness(_targetCenter));
        }
        else
        {
            _session.LeaveLens();
        }

        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Carried, "HX-002");
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (!_isHolding || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        _isHolding = false;
        ReleaseMouseCapture();
        Cursor = WCursors.Arrow;
        if (IsNearLens(_targetCenter, 210))
        {
            _activeLensCenter = NearestLensCenter(_targetCenter);
            _session.ApproachLens(1f);
            _session.PlaceIntoLens();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.NearSurface, "HX-002");
        }
        else
        {
            _session.LeaveLens();
            _session.PlaceOnSurface();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
        }

        e.Handled = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        DrawLenses(drawingContext);
        DrawThing(drawingContext);
        DrawLookControls(drawingContext);
    }

    private void DrawLookControls(DrawingContext drawingContext)
    {
        var text = new FormattedText(
            $"Look: {LensLookLabel(_lensLook)}   1 Glas   2 Wurmloch   3 Hybrid   Esc beendet",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            12,
            new SolidColorBrush(WColor.FromArgb(168, 28, 34, 36)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        var padding = new Rect(16, 16, text.Width + 22, text.Height + 12);
        var background = new SolidColorBrush(WColor.FromArgb(92, 248, 252, 250));
        var edge = new WPen(new SolidColorBrush(WColor.FromArgb(62, 255, 255, 255)), 0.8);
        drawingContext.DrawRoundedRectangle(background, edge, padding, 6, 6);
        drawingContext.DrawText(text, new WPoint(padding.X + 11, padding.Y + 6));
    }

    private void DrawLenses(DrawingContext drawingContext)
    {
        if (_session.LensEmergence <= 0)
        {
            return;
        }

        foreach (var center in LensCenters())
        {
            var activation = LensActivation(center);
            if (activation <= 0.02)
            {
                continue;
            }

            DrawLens(drawingContext, center, activation);
        }
    }

    private void DrawLens(DrawingContext drawingContext, WPoint center, double activation)
    {
        if (_session.LensEmergence <= 0)
        {
            return;
        }

        var open = EaseOut(_session.LensOpen) * activation;
        var bounds = LensBounds(center, activation);
        var throatCenter = TunnelThroatPoint(bounds, center, open);
        var profile = LensVisualProfile.For(_lensLook);

        var desktopSample = UpdateDesktopSample(bounds, center, activation);
        DrawLensContactShadow(drawingContext, bounds, center, open, profile);

        var clip = new EllipseGeometry(bounds);
        drawingContext.PushClip(clip);
        if (desktopSample is not null)
        {
            var refracted = new Rect(
                bounds.X - (bounds.Width * profile.RefractionOverscanX),
                bounds.Y - (bounds.Height * profile.RefractionOverscanY),
                bounds.Width * (1.0 + (profile.RefractionOverscanX * 2.0)),
                bounds.Height * (1.0 + (profile.RefractionOverscanY * 2.0)));
            drawingContext.PushOpacity(profile.BaseDesktopOpacity);
            drawingContext.DrawImage(desktopSample, refracted);
            drawingContext.Pop();

            for (var index = 1; index <= profile.RefractionLayers; index++)
            {
                var layer = index / (double)profile.RefractionLayers;
                var depthCurve = Math.Pow(layer, profile.RefractionCurve);
                var tunnelScale = 1.0 - (depthCurve * profile.RefractionDepth);
                var tunnelWidth = bounds.Width * tunnelScale;
                var tunnelHeight = bounds.Height * (1.0 - (depthCurve * profile.RefractionCompression));
                var outward = LensOutwardDirection(center);
                var tangent = new Vector(-outward.Y, outward.X);
                var tunnelOffset = (outward * (open * profile.RefractionPull * depthCurve)) +
                    (tangent * (Math.Sin(_phase * profile.ShimmerSpeed + index) * profile.ShimmerAmount));
                var tunnel = new Rect(
                    center.X - (tunnelWidth / 2) + tunnelOffset.X,
                    center.Y - (tunnelHeight / 2) + tunnelOffset.Y,
                    tunnelWidth,
                    tunnelHeight);

                drawingContext.PushOpacity(profile.RefractionLayerOpacity + (open * profile.RefractionOpenBoost));
                drawingContext.DrawImage(desktopSample, tunnel);
                drawingContext.Pop();
            }
        }

        DrawTunnelVolume(drawingContext, bounds, center, throatCenter, open, profile);
        DrawGlassCaustics(drawingContext, bounds, center, open, profile);
        DrawSpecularGlassSweeps(drawingContext, bounds, center, open, profile);

        var glass = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.34, 0.28),
            Center = new WPoint(0.48, 0.52),
            RadiusX = 0.76,
            RadiusY = 0.70,
            Opacity = profile.GlassOpacity
        };
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(profile.GlassCoreAlpha, 255, 255, 255), 0.0));
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(profile.GlassMidAlpha, 232, 244, 246), 0.42));
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(profile.GlassEdgeAlpha, 78, 94, 98), 1.0));
        drawingContext.DrawEllipse(glass, null, center, bounds.Width / 2, bounds.Height / 2);

        for (var index = 0; index < profile.GlassRingCount; index++)
        {
            var layer = index / Math.Max(1.0, profile.GlassRingCount - 1.0);
            var depthCurve = Math.Pow(layer, 1.52);
            var alpha = (byte)Math.Clamp(profile.GlassRingAlpha - (index * profile.GlassRingFade) + (open * profile.GlassRingOpenBoost), 6, 112);
            var ringPen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 248, 255, 255)), profile.GlassRingWidth + (open * 0.18));
            var rx = (bounds.Width / 2) * (1.0 - (depthCurve * profile.GlassRingDepthX));
            var ry = (bounds.Height / 2) * (1.0 - (depthCurve * profile.GlassRingDepthY));
            var ringCenter = Interpolate(center, throatCenter, depthCurve * profile.GlassRingThroatPull);
            drawingContext.DrawEllipse(null, ringPen, ringCenter, rx, ry);
        }

        drawingContext.Pop();

        DrawPhysicalGlassThickness(drawingContext, bounds, center, open, profile);
        DrawChromaticEdge(drawingContext, bounds, center, open, profile);
        var outer = new WPen(new SolidColorBrush(WColor.FromArgb(profile.OuterRimAlpha, 255, 255, 255)), profile.OuterRimWidth + (open * profile.OuterRimOpenWidth));
        var inner = new WPen(new SolidColorBrush(WColor.FromArgb(86, 18, 24, 26)), 1.0);
        DrawSoftFresnelEdge(drawingContext, bounds, center, open, profile);
        drawingContext.DrawEllipse(null, outer, center, bounds.Width / 2, bounds.Height / 2);
        drawingContext.DrawEllipse(null, inner, throatCenter, bounds.Width * (0.18 + (open * 0.10)), bounds.Height * (0.07 + (open * 0.05)));

        var highlight = new RadialGradientBrush(WColor.FromArgb(profile.HighlightAlpha, 255, 255, 255), WColor.FromArgb(0, 255, 255, 255));
        drawingContext.DrawEllipse(highlight, null, new WPoint(bounds.X + (bounds.Width * 0.66), bounds.Y + (bounds.Height * 0.23)), bounds.Width * profile.HighlightRadiusX, bounds.Height * profile.HighlightRadiusY);

        var depth = new RadialGradientBrush(WColor.FromArgb((byte)(profile.ThroatAlpha + (open * profile.ThroatOpenAlpha)), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0));
        drawingContext.DrawEllipse(depth, null, throatCenter, bounds.Width * (0.15 + (open * 0.11)), bounds.Height * (0.05 + (open * 0.06)));
    }

    private void DrawLensContactShadow(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        var shadowCenter = center + new Vector(bounds.Width * 0.018, bounds.Height * (0.16 + (open * 0.035)));
        var shadow = new RadialGradientBrush(WColor.FromArgb(profile.ContactShadowAlpha, 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.62,
            RadiusY = 0.46,
            Opacity = profile.ContactShadowOpacity * (0.72 + (open * 0.20))
        };
        drawingContext.DrawEllipse(shadow, null, shadowCenter, bounds.Width * 0.39, bounds.Height * 0.18);

        var innerContact = new RadialGradientBrush(WColor.FromArgb((byte)Math.Clamp(profile.ContactShadowAlpha * 0.72, 0, 255), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.70,
            RadiusY = 0.42,
            Opacity = profile.ContactShadowOpacity * 0.42
        };
        drawingContext.DrawEllipse(innerContact, null, shadowCenter + new Vector(bounds.Width * 0.02, bounds.Height * 0.015), bounds.Width * 0.25, bounds.Height * 0.08);
    }

    private static void DrawSoftFresnelEdge(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        var fresnel = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.38, 0.30),
            Center = new WPoint(0.50, 0.50),
            RadiusX = 0.74,
            RadiusY = 0.70,
            Opacity = profile.FresnelOpacity + (open * profile.FresnelOpenBoost)
        };
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.52));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(18, 255, 255, 255), 0.78));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(74, 255, 255, 255), 0.94));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(fresnel, null, center, bounds.Width / 2, bounds.Height / 2);
    }

    private static void DrawPhysicalGlassThickness(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        for (var layer = 0; layer < profile.GlassThicknessLayers; layer++)
        {
            var amount = layer / Math.Max(1.0, profile.GlassThicknessLayers - 1.0);
            var alpha = (byte)Math.Clamp(profile.GlassThicknessAlpha - (amount * profile.GlassThicknessFade) + (open * profile.GlassThicknessOpenBoost), 0, 132);
            if (alpha <= 1)
            {
                continue;
            }

            var insetX = bounds.Width * amount * 0.014;
            var insetY = bounds.Height * amount * 0.018;
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 248, 255, 255)), profile.GlassThicknessWidth + (amount * 0.18));
            drawingContext.DrawEllipse(null, pen, center - new Vector(insetX * 0.35, insetY * 0.45), (bounds.Width / 2) - insetX, (bounds.Height / 2) - insetY);
        }

        var lowerMass = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.10, 0.24),
            EndPoint = new WPoint(0.94, 0.82),
            Opacity = profile.GlassMassOpacity + (open * profile.GlassMassOpenBoost)
        };
        lowerMass.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
        lowerMass.GradientStops.Add(new GradientStop(WColor.FromArgb(38, 255, 255, 255), 0.44));
        lowerMass.GradientStops.Add(new GradientStop(WColor.FromArgb(18, 184, 208, 212), 0.66));
        lowerMass.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(lowerMass, null, center + new Vector(-bounds.Width * 0.03, bounds.Height * 0.06), bounds.Width * 0.47, bounds.Height * 0.30);
    }

    private static void DrawChromaticEdge(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        var alpha = (byte)Math.Clamp(profile.ChromaticEdgeAlpha + (open * profile.ChromaticEdgeOpenBoost), 0, 56);
        if (alpha <= 1)
        {
            return;
        }

        var offset = profile.ChromaticEdgeOffset + (open * 0.24);
        var red = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 110, 104)), profile.ChromaticEdgeWidth);
        var cyan = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 112, 226, 255)), profile.ChromaticEdgeWidth);
        drawingContext.DrawEllipse(null, red, center + new Vector(-offset, offset * 0.42), (bounds.Width / 2) * 0.996, (bounds.Height / 2) * 0.994);
        drawingContext.DrawEllipse(null, cyan, center + new Vector(offset, -offset * 0.42), (bounds.Width / 2) * 0.996, (bounds.Height / 2) * 0.994);
    }

    private void DrawTunnelVolume(DrawingContext drawingContext, Rect bounds, WPoint center, WPoint throatCenter, double open, LensVisualProfile profile)
    {
        var outward = LensOutwardDirection(center);
        var tangent = new Vector(-outward.Y, outward.X);
        var throat = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.54, 0.48),
            Center = new WPoint(0.54, 0.52),
            RadiusX = 0.70,
            RadiusY = 0.54,
            Opacity = Math.Clamp(profile.ApertureOpacity + (open * 0.10), 0.12, 0.82)
        };
        var throatCoreAlpha = (byte)Math.Clamp(profile.ThroatAlpha + (open * profile.ThroatOpenAlpha), 10, 180);
        var throatMidAlpha = (byte)Math.Clamp((profile.ThroatAlpha * 0.42) + (open * profile.ThroatOpenAlpha * 0.54), 4, 112);
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb(throatCoreAlpha, 8, 11, 12), 0.0));
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb(throatMidAlpha, 28, 34, 34), 0.42));
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            throat,
            null,
            throatCenter,
            bounds.Width * (profile.ThroatRadiusX + (open * 0.12)),
            bounds.Height * (profile.ThroatRadiusY + (open * 0.075)));

        for (var index = 0; index < profile.TunnelRingCount; index++)
        {
            var layer = index / Math.Max(1.0, profile.TunnelRingCount - 1.0);
            var depthCurve = Math.Pow(layer, profile.TunnelDepthCurve);
            var rx = (bounds.Width / 2) * (1.0 - (depthCurve * profile.TunnelDepthX));
            var ry = (bounds.Height / 2) * (1.0 - (depthCurve * profile.TunnelDepthY));
            if (rx < 5 || ry < 3)
            {
                continue;
            }

            var depthShift = outward * ((open * depthCurve * profile.TunnelOpenPull) + (depthCurve * profile.TunnelBasePull));
            var shimmerShift = tangent * (Math.Sin(_phase * profile.ShimmerSpeed + (index * 0.31)) * profile.TunnelShimmer);
            var ringCenter = center + depthShift + shimmerShift;
            var lightAlpha = (byte)Math.Clamp(profile.TunnelLightAlpha - (index * profile.TunnelLightFade) + (open * profile.TunnelLightOpenBoost), 5, 118);
            var darkAlpha = (byte)Math.Clamp(profile.TunnelDarkAlpha + (open * profile.TunnelDarkOpenBoost) - (index * profile.TunnelDarkFade), 4, 78);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(darkAlpha, 8, 11, 12)), profile.TunnelDarkWidth), ringCenter + (outward * 1.1), rx, ry);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(lightAlpha, 250, 253, 250)), profile.TunnelLightWidth + (open * 0.20)), ringCenter, rx, ry);
        }

        for (var ray = 0; ray < profile.LightRayCount; ray++)
        {
            var normalized = (ray / Math.Max(1.0, profile.LightRayCount - 1.0)) - 0.5;
            var angle = (normalized * 2.24) + (Math.Sin(_phase * 0.58 + ray) * 0.018);
            var startRadiusX = bounds.Width * (0.10 + (open * 0.04));
            var startRadiusY = bounds.Height * (0.035 + (open * 0.018));
            var endRadiusX = bounds.Width * (0.50 - (Math.Abs(normalized) * 0.09));
            var endRadiusY = bounds.Height * (0.38 - (Math.Abs(normalized) * 0.06));
            var radial = (outward * Math.Cos(angle)) + (tangent * Math.Sin(angle));
            var start = throatCenter + new Vector(radial.X * startRadiusX, radial.Y * startRadiusY);
            var end = center + new Vector(radial.X * endRadiusX, radial.Y * endRadiusY);
            var control = Interpolate(start, end, 0.58) + (outward * open * 22.0) + (tangent * Math.Sin(_phase * 0.82 + ray) * 1.4);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.QuadraticBezierTo(control, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(profile.LightRayAlpha + (open * profile.LightRayOpenBoost) - (Math.Abs(normalized) * 12), 3, 64);
            drawingContext.DrawGeometry(null, new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 255, 252)), profile.LightRayWidth), geometry);
        }

        DrawPremiumRefractionRibbons(drawingContext, bounds, center, throatCenter, open, profile);
        DrawPremiumTunnelAperture(drawingContext, bounds, throatCenter, outward, open, profile);
        DrawMicroGlassHighlights(drawingContext, bounds, center, open, profile);

        var reflectedEdge = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.06, 0.02),
            EndPoint = new WPoint(0.74, 0.84),
            Opacity = profile.ReflectedEdgeOpacity + (open * profile.ReflectedEdgeOpenBoost)
        };
        reflectedEdge.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
        reflectedEdge.GradientStops.Add(new GradientStop(WColor.FromArgb(72, 255, 255, 255), 0.42));
        reflectedEdge.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            reflectedEdge,
            null,
            center + new Vector(-bounds.Width * 0.06, -bounds.Height * 0.07),
            bounds.Width * 0.48,
            bounds.Height * 0.34);
    }

    private void DrawPremiumRefractionRibbons(DrawingContext drawingContext, Rect bounds, WPoint center, WPoint throatCenter, double open, LensVisualProfile profile)
    {
        for (var ribbon = 0; ribbon < profile.RibbonCount; ribbon++)
        {
            var normalized = (ribbon / Math.Max(1.0, profile.RibbonCount - 1.0)) - 0.5;
            var startAngle = (normalized * 2.0) + (Math.Sin(_phase * 0.42 + ribbon) * 0.030);
            var endAngle = startAngle * 0.34;
            var start = center + new Vector(
                Math.Cos(startAngle) * bounds.Width * (0.46 - (Math.Abs(normalized) * 0.06)),
                Math.Sin(startAngle) * bounds.Height * (0.36 - (Math.Abs(normalized) * 0.05)));
            var end = throatCenter + new Vector(
                Math.Cos(endAngle) * bounds.Width * (0.10 + (open * 0.05)),
                Math.Sin(endAngle) * bounds.Height * (0.035 + (open * 0.018)));
            var controlA = Interpolate(start, end, 0.32) + new Vector(open * 14.0, -normalized * bounds.Height * 0.07);
            var controlB = Interpolate(start, end, 0.72) + new Vector(open * 28.0, normalized * bounds.Height * 0.04);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.BezierTo(controlA, controlB, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(profile.RibbonAlpha + (open * profile.RibbonOpenBoost) - (Math.Abs(normalized) * 18), 3, 76);
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 250, 252, 248)), profile.RibbonWidth + (open * 0.18));
            drawingContext.DrawGeometry(null, pen, geometry);
        }

        var rimShard = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.16, 0.10),
            EndPoint = new WPoint(0.94, 0.72),
            Opacity = profile.RimShardOpacity + (open * 0.18)
        };
        rimShard.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
        rimShard.GradientStops.Add(new GradientStop(WColor.FromArgb(92, 255, 255, 255), 0.38));
        rimShard.GradientStops.Add(new GradientStop(WColor.FromArgb(18, 218, 226, 224), 0.58));
        rimShard.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            rimShard,
            null,
            center + new Vector(-bounds.Width * 0.12, -bounds.Height * 0.16),
            bounds.Width * 0.36,
            bounds.Height * 0.18);
    }

    private void DrawMicroGlassHighlights(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        for (var index = 0; index < profile.MicroHighlightCount; index++)
        {
            var seed = index * 1.61803398875;
            var angle = seed + (_phase * profile.MicroHighlightSpeed * (0.18 + (index * 0.012)));
            var radius = 0.16 + ((index % 5) * 0.055);
            var x = center.X + (Math.Cos(angle) * bounds.Width * radius);
            var y = center.Y + (Math.Sin(angle * 0.82) * bounds.Height * radius * 0.68);
            var alpha = (byte)Math.Clamp(profile.MicroHighlightAlpha + (open * profile.MicroHighlightOpenBoost) - ((index % 4) * 2.2), 0, 48);
            if (alpha <= 1)
            {
                continue;
            }

            var glint = new RadialGradientBrush(WColor.FromArgb(alpha, 255, 255, 255), WColor.FromArgb(0, 255, 255, 255))
            {
                RadiusX = 0.62,
                RadiusY = 0.62,
                Opacity = 0.82
            };
            var rx = 1.2 + ((index % 3) * 0.55);
            var ry = 0.7 + ((index % 2) * 0.40);
            drawingContext.DrawEllipse(glint, null, new WPoint(x, y), rx, ry);
        }
    }

    private void DrawGlassCaustics(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        for (var index = 0; index < profile.CausticLineCount; index++)
        {
            var normalized = (index / Math.Max(1.0, profile.CausticLineCount - 1.0)) - 0.5;
            var wave = Math.Sin(_phase * profile.CausticSpeed + index * 0.74);
            var y = center.Y + bounds.Height * (0.18 + (normalized * 0.22));
            var start = new WPoint(center.X - bounds.Width * (0.24 + Math.Abs(normalized) * 0.08), y + (wave * 1.6));
            var end = new WPoint(center.X + bounds.Width * (0.22 + Math.Abs(normalized) * 0.06), y - (wave * 1.2));
            var control = Interpolate(start, end, 0.50) + new Vector(wave * bounds.Width * 0.035, -bounds.Height * (0.018 + Math.Abs(normalized) * 0.012));

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.QuadraticBezierTo(control, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(profile.CausticAlpha + (open * profile.CausticOpenBoost) - (Math.Abs(normalized) * 12), 0, 72);
            if (alpha <= 1)
            {
                continue;
            }

            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 255, 246)), profile.CausticWidth);
            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }

    private void DrawSpecularGlassSweeps(DrawingContext drawingContext, Rect bounds, WPoint center, double open, LensVisualProfile profile)
    {
        for (var index = 0; index < profile.SpecularSweepCount; index++)
        {
            var amount = index / Math.Max(1.0, profile.SpecularSweepCount - 1.0);
            var drift = Math.Sin((_phase * profile.SpecularSweepSpeed) + (index * 0.92)) * bounds.Width * 0.018;
            var vertical = -0.30 + (amount * 0.34);
            var start = new WPoint(center.X - (bounds.Width * (0.38 - (amount * 0.04))) + drift, center.Y + (bounds.Height * vertical));
            var end = new WPoint(center.X + (bounds.Width * (0.30 + (amount * 0.08))) + drift, center.Y + (bounds.Height * (vertical + 0.25)));
            var controlA = Interpolate(start, end, 0.28) + new Vector(bounds.Width * 0.08, -bounds.Height * (0.10 + (amount * 0.03)));
            var controlB = Interpolate(start, end, 0.78) + new Vector(bounds.Width * 0.12, bounds.Height * (0.04 - (amount * 0.02)));

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.BezierTo(controlA, controlB, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(profile.SpecularSweepAlpha + (open * profile.SpecularSweepOpenBoost) - (amount * 14), 0, 88);
            if (alpha <= 1)
            {
                continue;
            }

            var sweepBrush = new LinearGradientBrush
            {
                StartPoint = new WPoint(0.0, 0.0),
                EndPoint = new WPoint(1.0, 1.0),
                Opacity = 0.80
            };
            sweepBrush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
            sweepBrush.GradientStops.Add(new GradientStop(WColor.FromArgb(alpha, 255, 255, 255), 0.42));
            sweepBrush.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)Math.Clamp(alpha * 0.42, 0, 255), 210, 232, 232), 0.58));
            sweepBrush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));

            var pen = new WPen(sweepBrush, profile.SpecularSweepWidth + (open * 0.20) - (amount * 0.08))
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }

    private static void DrawPremiumTunnelAperture(DrawingContext drawingContext, Rect bounds, WPoint throatCenter, Vector outward, double open, LensVisualProfile profile)
    {
        var tangent = new Vector(-outward.Y, outward.X);
        var apertureShadow = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.47, 0.48),
            Center = new WPoint(0.50, 0.52),
            RadiusX = 0.64,
            RadiusY = 0.46,
            Opacity = profile.ApertureOpacity + (open * 0.18)
        };
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(118 + (open * 34)), 5, 7, 8), 0.0));
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(72 + (open * 26)), 16, 20, 21), 0.46));
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            apertureShadow,
            null,
            throatCenter + (outward * open * 5.0) + (tangent * 1.5),
            bounds.Width * (0.18 + (open * 0.105)),
            bounds.Height * (0.052 + (open * 0.052)));

        for (var ring = 0; ring < profile.ApertureRingCount; ring++)
        {
            var layer = ring / Math.Max(1.0, profile.ApertureRingCount - 1.0);
            var alpha = (byte)Math.Clamp(profile.ApertureRingAlpha - (ring * 10) + (open * 28), 14, 126);
            var rx = bounds.Width * (0.18 + (open * 0.12) + (layer * 0.035));
            var ry = bounds.Height * (0.052 + (open * 0.050) + (layer * 0.014));
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 244, 250, 248)), 0.42 + (open * 0.18));
            drawingContext.DrawEllipse(null, pen, throatCenter + (outward * ((open * 6.0) + (layer * 2.0))) + (tangent * (1.0 + (layer * 0.7))), rx, ry);
        }

        var lowerGlass = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.08, 0.30),
            EndPoint = new WPoint(0.94, 0.70),
            Opacity = 0.22 + (open * 0.14)
        };
        lowerGlass.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
        lowerGlass.GradientStops.Add(new GradientStop(WColor.FromArgb(70, 255, 255, 255), 0.48));
        lowerGlass.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            lowerGlass,
            null,
            throatCenter - (outward * bounds.Width * 0.05) + (tangent * bounds.Height * 0.095),
            bounds.Width * 0.30,
            bounds.Height * 0.045);
    }

    private void DrawThing(DrawingContext drawingContext)
    {
        if (_session.Absorption >= 1 && !_isHolding)
        {
            if (_session.CanPullOutFromLens)
            {
                DrawTunnelRestingThing(drawingContext);
            }

            return;
        }

        var progress = SmoothStep(_session.Absorption);
        var throatTarget = ActiveTunnelThroatPoint();
        var center = Interpolate(_thingCenter, throatTarget, progress * 0.88);
        var recovery = EaseOut(_session.PullOutRecovery);
        var lifted = _session.IsHoldingThing || _isHolding || progress > 0.01;
        var profile = LensVisualProfile.For(_lensLook);
        var targetCarryScale = profile.CarryScaleBase + (recovery * profile.CarryScaleRecovery);
        var gripProgress = lifted ? Math.Max(EaseOut(_session.PickProgress), SmoothStep(progress)) : 0.0;
        var carryScale = 1.0 + ((targetCarryScale - 1.0) * gripProgress);
        var scale = carryScale * Math.Clamp(1.0 - (progress * 0.62), 0.34, 1.0);
        var width = 178 * scale;
        var height = 94 * scale * (1.0 - (progress * 0.14));
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
        var opacity = progress < 0.78 ? 1.0 : Math.Clamp(1.0 - ((progress - 0.78) / 0.22), 0.12, 1.0);
        var portalPull = GetPortalPullAmount(bounds, lifted);
        var portalSqueeze = Math.Max(portalPull, SmoothStep(progress) * 0.84);
        var throatPointCollapse = SmoothStep(1.0 - ((throatTarget - center).Length / (Math.Max(width, height) * 0.74))) * portalSqueeze;

        if (lifted)
        {
            var shadowSuction = Math.Max(SmoothStep(progress), portalSqueeze * 0.88);
            var shadowAlpha = (byte)Math.Clamp((58 + (Math.Abs(_session.TiltX) * 2.2) + (Math.Abs(_session.TiltY) * 1.8)) * opacity * (1.0 - (shadowSuction * 0.52)), 5, 96);
            var freeShadowCenter = new WPoint(center.X + _session.ShadowX, center.Y + (height * 0.48) + _session.ShadowY);
            var shadowCenter = Interpolate(freeShadowCenter, throatTarget, shadowSuction * 0.92);
            var shadowWidth = width * (0.64 + (Math.Abs(_session.TiltX) * 0.010)) * (1.0 - (shadowSuction * 0.62));
            var shadowHeight = height * (0.28 + (Math.Abs(_session.TiltY) * 0.006)) * (1.0 - (shadowSuction * 0.38));
            for (var layer = 10; layer >= 0; layer--)
            {
                var inflate = 4.5 + (layer * 3.6);
                var alpha = (byte)Math.Clamp(shadowAlpha / (2.3 + (layer * 0.82)), 2, 72);
                var shadowBounds = new Rect(
                    shadowCenter.X - (shadowWidth / 2) - inflate,
                    shadowCenter.Y - (shadowHeight / 2) - (inflate * 0.45),
                    shadowWidth + (inflate * 2),
                    shadowHeight + inflate);
                var shadowGeometry = CreateRectangularGeometry(
                    shadowBounds,
                    Math.Max(progress * 0.46, shadowSuction * 0.22),
                    _session.TiltX * 0.14,
                    _session.TiltY * 0.14,
                    throatTarget,
                    shadowSuction * 0.74);
                drawingContext.DrawGeometry(new SolidColorBrush(WColor.FromArgb(alpha, 0, 0, 0)), null, shadowGeometry);
            }
        }

        var geometry = CreateThingGeometry(bounds, progress, _session.TiltX, _session.TiltY, portalSqueeze, throatTarget);
        var fill = new LinearGradientBrush(WColor.FromArgb((byte)(242 * opacity), 252, 252, 246), WColor.FromArgb((byte)(216 * opacity), 214, 228, 230), 90);
        drawingContext.DrawGeometry(fill, new WPen(new SolidColorBrush(WColor.FromArgb((byte)(112 * opacity), 96, 112, 118)), 0.9), geometry);

        var text = new FormattedText(
            "Rechnung.pdf",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            Math.Max(8, 11 * scale),
            new SolidColorBrush(WColor.FromArgb((byte)(70 * opacity * (1.0 - Math.Max(portalSqueeze * 0.48, throatPointCollapse * 0.92))), 34, 42, 48)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            TextAlignment = TextAlignment.Center,
            MaxTextWidth = bounds.Width * 0.74,
            MaxTextHeight = bounds.Height * 0.32
        };
        drawingContext.DrawText(text, new WPoint(bounds.X + (bounds.Width * 0.13), bounds.Y + (bounds.Height * 0.36)));
    }

    private void DrawTunnelRestingThing(DrawingContext drawingContext)
    {
        var lensCenter = LensCenter();
        var throatTarget = ActiveTunnelThroatPoint();
        var inward = LensOutwardDirection(lensCenter) * -1.0;
        var pulse = 0.5 + (Math.Sin(_phase * 2.2) * 0.5);
        var remaining = _session.TransitRemainingMilliseconds / (double)_session.TransitTimeoutMilliseconds;
        var opacity = Math.Clamp(0.24 + (remaining * 0.24) + (pulse * 0.05), 0.18, 0.52);
        var width = 46 + (pulse * 1.4);
        var height = 22 + (pulse * 0.8);
        var center = throatTarget + (inward * (24 + (pulse * 2.0)));
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
        var geometry = CreateRectangularGeometry(bounds, 0.0, 0.4, -0.2, throatTarget, 0.0);
        var fill = new LinearGradientBrush(
            WColor.FromArgb((byte)(190 * opacity), 250, 252, 248),
            WColor.FromArgb((byte)(118 * opacity), 144, 160, 164),
            90);
        var edge = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(120 * opacity), 255, 255, 255)), 0.85);
        drawingContext.DrawGeometry(fill, edge, geometry);

        var innerShadow = new RadialGradientBrush(WColor.FromArgb((byte)(64 * opacity), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0));
        drawingContext.DrawEllipse(innerShadow, null, center + new Vector(6, 10), width * 0.38, height * 0.26);
    }

    private double GetPortalPullAmount(Rect bounds, bool lifted)
    {
        if (!lifted)
        {
            return 0;
        }

        var throatTarget = ActiveTunnelThroatPoint();
        var nearest = new WPoint(
            Math.Clamp(throatTarget.X, bounds.Left, bounds.Right),
            Math.Clamp(throatTarget.Y, bounds.Top, bounds.Bottom));
        var distance = (nearest - throatTarget).Length;
        return SmoothStep(1.0 - (distance / 196.0));
    }

    private BitmapSource? UpdateDesktopSample(Rect lensBounds, WPoint center, double activation)
    {
        var key = LensSampleKey(center);
        _sampleFrame++;
        var overlayIsOpticallyActive = _isHolding ||
            _session.IsHoldingThing ||
            _session.Absorption > 0.001f ||
            _session.LensEmergence > 0.06f ||
            _session.TransitState != GpuLivingLensTransitState.LocalReady;
        var isActiveLens = (center - _activeLensCenter).Length < 1.0;
        if (_lensSamples.TryGetValue(key, out var cached) && overlayIsOpticallyActive && !isActiveLens)
        {
            return cached;
        }

        var refreshEvery = overlayIsOpticallyActive
            ? (isActiveLens ? 5 : 48)
            : 96;
        if (_lensSamples.TryGetValue(key, out cached) && _sampleFrame % refreshEvery != 0)
        {
            return cached;
        }

        var sample = DesktopSampleRect(lensBounds);
        var desktopSample = DesktopRefractionSampler.Capture(sample);
        if (desktopSample is not null)
        {
            _lensSamples[key] = desktopSample;
        }

        return desktopSample;
    }

    private void WarmCleanDesktopPlates()
    {
        foreach (var center in LensCenters())
        {
            var sample = DesktopSampleRect(CleanPlateBounds(center));
            var desktopSample = DesktopRefractionSampler.Capture(sample);
            if (desktopSample is not null)
            {
                _lensSamples[LensSampleKey(center)] = desktopSample;
            }
        }
    }

    private Rect CleanPlateBounds(WPoint center)
    {
        var width = 338.0;
        var height = 252.0;
        return new Rect(center.X - (width / 2.0), center.Y - (height / 2.0), width, height);
    }

    private Int32Rect DesktopSampleRect(Rect lensBounds)
    {
        var x = Math.Max(_screenBounds.Left, _screenBounds.Left + (int)Math.Round(lensBounds.X - 54));
        var y = Math.Max(_screenBounds.Top, _screenBounds.Top + (int)Math.Round(lensBounds.Y - 44));
        var width = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Width + 108)), Math.Max(8, _screenBounds.Right - x));
        var height = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Height + 88)), Math.Max(8, _screenBounds.Bottom - y));
        return new Int32Rect(x, y, width, height);
    }

    private static string LensSampleKey(WPoint center)
    {
        return $"{Math.Round(center.X)}:{Math.Round(center.Y)}";
    }

    private WPoint ProjectThroughScreenEdge(WPoint point)
    {
        const double edgeBand = 150;
        const double continuation = 130;
        var x = point.X;
        if (point.X > RenderSize.Width - edgeBand)
        {
            var amount = SmoothStep((point.X - (RenderSize.Width - edgeBand)) / edgeBand);
            x += amount * continuation;
        }
        else if (point.X < edgeBand)
        {
            var amount = SmoothStep((edgeBand - point.X) / edgeBand);
            x -= amount * continuation;
        }

        var y = point.Y;
        if (point.Y > RenderSize.Height - edgeBand)
        {
            var amount = SmoothStep((point.Y - (RenderSize.Height - edgeBand)) / edgeBand);
            y += amount * continuation;
        }
        else if (point.Y < edgeBand)
        {
            var amount = SmoothStep((edgeBand - point.Y) / edgeBand);
            y -= amount * continuation;
        }

        return new WPoint(x, y);
    }

    private Rect ThingBounds()
    {
        return new Rect(_thingCenter.X - 90, _thingCenter.Y - 52, 180, 104);
    }

    private WPoint LensCenter()
    {
        return _activeLensCenter;
    }

    private WPoint ActiveTunnelThroatPoint()
    {
        var activation = LensActivation(_activeLensCenter);
        var open = EaseOut(_session.LensOpen) * activation;
        var bounds = LensBounds(_activeLensCenter, activation);
        return TunnelThroatPoint(bounds, _activeLensCenter, open);
    }

    private Rect LensBounds(WPoint center, double activation)
    {
        var profile = LensVisualProfile.For(_lensLook);
        var emergence = EaseOut(_session.LensEmergence) * (0.54 + (activation * 0.46));
        var open = EaseOut(_session.LensOpen) * activation;
        var distanceScale = 0.82 + (activation * 0.20);
        var width = 210 * profile.LensScale * distanceScale * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.12));
        var height = 150 * profile.LensScale * distanceScale * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.05));
        return new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
    }

    private WPoint TunnelThroatPoint(Rect bounds, WPoint center, double open)
    {
        var outward = LensOutwardDirection(center);
        return center + new Vector(
            outward.X * bounds.Width * (0.20 + (open * 0.14)),
            outward.Y * bounds.Height * (0.18 + (open * 0.12)));
    }

    private Vector LensOutwardDirection(WPoint center)
    {
        var width = RenderSize.Width > 1 ? RenderSize.Width : _screenBounds.Width;
        var height = RenderSize.Height > 1 ? RenderSize.Height : _screenBounds.Height;
        var roomCenter = new WPoint(width / 2.0, height / 2.0);
        var direction = center - roomCenter;
        if (Math.Abs(direction.X) < width * 0.08)
        {
            direction.X = 0;
        }

        if (Math.Abs(direction.Y) < height * 0.08)
        {
            direction.Y = 0;
        }

        if (direction.Length <= 0.001)
        {
            direction = new Vector(1, 0);
        }

        direction.Normalize();
        return direction;
    }

    private bool IsNearLens(WPoint point, double radius)
    {
        return (point - NearestLensCenter(point)).Length <= radius;
    }

    private double NormalizedLensNearness(WPoint point)
    {
        return Math.Clamp(1.0 - ((point - NearestLensCenter(point)).Length / 240.0), 0.0, 1.0);
    }

    private WPoint[] LensCenters()
    {
        var width = RenderSize.Width > 1 ? RenderSize.Width : _screenBounds.Width;
        var height = RenderSize.Height > 1 ? RenderSize.Height : _screenBounds.Height;
        const double fullLensWidth = 236;
        const double fullLensHeight = 168;
        var outsideX = fullLensWidth * (1.0 - _session.LensVisibleRatio);
        var outsideY = fullLensHeight * (1.0 - _session.LensVisibleRatio);
        var insetX = (fullLensWidth / 2.0) - outsideX;
        var insetY = (fullLensHeight / 2.0) - outsideY;

        return
        [
            new WPoint(insetX, insetY),
            new WPoint(width / 2.0, insetY),
            new WPoint(width - insetX, insetY),
            new WPoint(insetX, height / 2.0),
            new WPoint(width - insetX, height / 2.0),
            new WPoint(insetX, height - insetY),
            new WPoint(width / 2.0, height - insetY),
            new WPoint(width - insetX, height - insetY)
        ];
    }

    private WPoint NearestLensCenter(WPoint point)
    {
        var centers = LensCenters();
        var nearest = centers[0];
        var nearestDistance = double.MaxValue;
        foreach (var center in centers)
        {
            var distance = (point - center).LengthSquared;
            if (distance < nearestDistance)
            {
                nearest = center;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private WPoint ResolveActiveLensCenter(WPoint point)
    {
        var nearest = NearestLensCenter(point);
        if ((nearest - _activeLensCenter).Length < 1.0)
        {
            return _activeLensCenter;
        }

        var activeDistance = (point - _activeLensCenter).Length;
        var nearestDistance = (point - nearest).Length;
        if (activeDistance < 460 && nearestDistance > activeDistance - 142)
        {
            return _activeLensCenter;
        }

        return nearest;
    }

    private double LensActivation(WPoint center)
    {
        var isActive = (center - _activeLensCenter).Length < 1.0;
        if (_isHolding || _session.IsHoldingThing)
        {
            var pick = EaseOut(_session.PickProgress);
            var nearness = SmoothStep(1.0 - ((_targetCenter - center).Length / 360.0));
            return isActive
                ? 0.16 + (pick * 0.20) + (nearness * 0.54)
                : 0.035 + (pick * 0.06) + (nearness * 0.20);
        }

        if (_session.Absorption > 0 || _session.CanPullOutFromLens || _session.TransitState != GpuLivingLensTransitState.LocalReady)
        {
            return isActive ? 1.0 : 0.0;
        }

        return isActive ? 0.22 : 0.0;
    }

    private static WPoint Interpolate(WPoint source, WPoint target, double amount)
    {
        return new WPoint(source.X + ((target.X - source.X) * amount), source.Y + ((target.Y - source.Y) * amount));
    }

    private StreamGeometry CreateThingGeometry(Rect bounds, double absorption, double tiltX, double tiltY, double portalPull, WPoint suctionTarget)
    {
        return CreateRectangularGeometry(bounds, absorption, tiltX, tiltY, suctionTarget, portalPull);
    }

    private static StreamGeometry CreateRectangularGeometry(Rect bounds, double absorption, double tiltX, double tiltY, WPoint suctionTarget, double portalPull)
    {
        var portalAmount = SmoothStep(portalPull);
        var tiltDamping = 1.0 - (portalAmount * 0.88);
        var directionX = Math.Clamp(tiltX / 8.5, -1.0, 1.0) * tiltDamping;
        var directionY = Math.Clamp(-tiltY / 8.5, -1.0, 1.0) * tiltDamping;
        var pull = bounds.Width * 0.08 * absorption * (1.0 - (portalAmount * 0.62));
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        var topLeft = PerspectiveCorner(bounds, -1, -1, directionX, directionY, pull, suctionTarget, portalAmount);
        var topRight = PerspectiveCorner(bounds, 1, -1, directionX, directionY, pull, suctionTarget, portalAmount);
        var bottomRight = PerspectiveCorner(bounds, 1, 1, directionX, directionY, pull, suctionTarget, portalAmount);
        var bottomLeft = PerspectiveCorner(bounds, -1, 1, directionX, directionY, pull, suctionTarget, portalAmount);
        context.BeginFigure(topLeft, true, true);
        context.LineTo(topRight, true, false);
        context.LineTo(bottomRight, true, false);
        context.LineTo(bottomLeft, true, false);
        geometry.Freeze();
        return geometry;
    }

    private static WPoint PerspectiveCorner(Rect bounds, int cornerX, int cornerY, double directionX, double directionY, double pull, WPoint suctionTarget, double portalAmount)
    {
        const double baseInsetX = 0;
        const double baseInsetY = 0;
        var x = cornerX < 0 ? bounds.Left + baseInsetX : bounds.Right - baseInsetX;
        var y = cornerY < 0 ? bounds.Top + baseInsetY : bounds.Bottom - baseInsetY;
        var dot = (cornerX * directionX) + (cornerY * directionY);
        var recede = Math.Max(0, dot);
        var forward = Math.Max(0, -dot);
        var offsetX = (-cornerX * recede * bounds.Width * 0.070) + (cornerX * forward * bounds.Width * 0.045);
        var offsetY = (-cornerY * recede * bounds.Height * 0.105) + (cornerY * forward * bounds.Height * 0.060);
        var point = new WPoint(x + offsetX + pull, y + offsetY);
        if (portalAmount <= 0)
        {
            return point;
        }

        var center = new WPoint(bounds.Left + (bounds.Width / 2.0), bounds.Top + (bounds.Height / 2.0));
        var vectorToTarget = suctionTarget - center;
        var targetDistance = vectorToTarget.Length;
        if (targetDistance > 0.001)
        {
            vectorToTarget.Normalize();
        }
        else
        {
            vectorToTarget = new Vector(1, 0);
        }

        var cornerOffset = new Vector((bounds.Width / 2.0) * cornerX, (bounds.Height / 2.0) * cornerY);
        var cornerProjection = (cornerOffset.X * vectorToTarget.X) + (cornerOffset.Y * vectorToTarget.Y);
        var maxProjection = (Math.Abs(vectorToTarget.X) * bounds.Width / 2.0) + (Math.Abs(vectorToTarget.Y) * bounds.Height / 2.0);
        var leadingDistance = Math.Max(0.0, maxProjection - cornerProjection);
        var leadingBand = Math.Max(18.0, Math.Min(bounds.Width, bounds.Height) * 0.74);
        var leadWeight = SmoothStep(1.0 - (leadingDistance / leadingBand));
        var supportWeight = SmoothStep(1.0 - (leadingDistance / (leadingBand * 1.92))) * 0.18;
        var pointCollapse = SmoothStep(1.0 - (targetDistance / (Math.Max(bounds.Width, bounds.Height) * 0.74))) * portalAmount;
        var collapseWeight = Math.Clamp(Math.Max(leadWeight + supportWeight, pointCollapse), 0.0, 1.0);
        var apex = suctionTarget;
        var driftDistance = Math.Min(targetDistance * 0.10, bounds.Width * 0.045);
        var stableDrift = vectorToTarget * portalAmount * driftDistance * (1.0 - pointCollapse);
        if (collapseWeight <= 0.01)
        {
            return point + stableDrift;
        }

        var collapse = Math.Clamp((portalAmount - 0.10) / 0.90, 0.0, 1.0);
        collapse = Math.Max(SmoothStep(collapse) * (0.965 * collapseWeight), pointCollapse * 0.998);
        var collapsed = Interpolate(point + stableDrift, apex, collapse);
        return collapsed;
    }

    private static double EaseOut(double value)
    {
        var clamped = Math.Clamp(value, 0.0, 1.0);
        return 1.0 - Math.Pow(1.0 - clamped, 3.0);
    }

    private static double SmoothStep(double value)
    {
        var clamped = Math.Clamp(value, 0.0, 1.0);
        return clamped * clamped * (3.0 - (2.0 * clamped));
    }

    private static string LensLookLabel(GpuLivingLensLook look)
    {
        return look switch
        {
            GpuLivingLensLook.GlassBubble => "Glasblase",
            GpuLivingLensLook.Wormhole => "Wurmloch",
            _ => "Hybrid"
        };
    }

    private sealed class LensVisualProfile
    {
        public double LensScale { get; init; }
        public double RefractionOverscanX { get; init; }
        public double RefractionOverscanY { get; init; }
        public double BaseDesktopOpacity { get; init; }
        public int RefractionLayers { get; init; }
        public double RefractionCurve { get; init; }
        public double RefractionDepth { get; init; }
        public double RefractionCompression { get; init; }
        public double RefractionPull { get; init; }
        public double ShimmerSpeed { get; init; }
        public double ShimmerAmount { get; init; }
        public double RefractionLayerOpacity { get; init; }
        public double RefractionOpenBoost { get; init; }
        public double GlassOpacity { get; init; }
        public byte GlassCoreAlpha { get; init; }
        public byte GlassMidAlpha { get; init; }
        public byte GlassEdgeAlpha { get; init; }
        public int GlassRingCount { get; init; }
        public double GlassRingAlpha { get; init; }
        public double GlassRingFade { get; init; }
        public double GlassRingOpenBoost { get; init; }
        public double GlassRingWidth { get; init; }
        public double GlassRingDepthX { get; init; }
        public double GlassRingDepthY { get; init; }
        public double GlassRingThroatPull { get; init; }
        public byte OuterRimAlpha { get; init; }
        public double OuterRimWidth { get; init; }
        public double OuterRimOpenWidth { get; init; }
        public double FresnelOpacity { get; init; }
        public double FresnelOpenBoost { get; init; }
        public byte HighlightAlpha { get; init; }
        public double HighlightRadiusX { get; init; }
        public double HighlightRadiusY { get; init; }
        public double ThroatAlpha { get; init; }
        public double ThroatOpenAlpha { get; init; }
        public double ThroatRadiusX { get; init; }
        public double ThroatRadiusY { get; init; }
        public int TunnelRingCount { get; init; }
        public double TunnelDepthCurve { get; init; }
        public double TunnelDepthX { get; init; }
        public double TunnelDepthY { get; init; }
        public double TunnelOpenPull { get; init; }
        public double TunnelBasePull { get; init; }
        public double TunnelShimmer { get; init; }
        public double TunnelLightAlpha { get; init; }
        public double TunnelLightFade { get; init; }
        public double TunnelLightOpenBoost { get; init; }
        public double TunnelDarkAlpha { get; init; }
        public double TunnelDarkOpenBoost { get; init; }
        public double TunnelDarkFade { get; init; }
        public double TunnelDarkWidth { get; init; }
        public double TunnelLightWidth { get; init; }
        public int LightRayCount { get; init; }
        public double LightRayAlpha { get; init; }
        public double LightRayOpenBoost { get; init; }
        public double LightRayWidth { get; init; }
        public double ReflectedEdgeOpacity { get; init; }
        public double ReflectedEdgeOpenBoost { get; init; }
        public int RibbonCount { get; init; }
        public double RibbonAlpha { get; init; }
        public double RibbonOpenBoost { get; init; }
        public double RibbonWidth { get; init; }
        public double RimShardOpacity { get; init; }
        public byte ContactShadowAlpha { get; init; }
        public double ContactShadowOpacity { get; init; }
        public int GlassThicknessLayers { get; init; }
        public double GlassThicknessAlpha { get; init; }
        public double GlassThicknessFade { get; init; }
        public double GlassThicknessOpenBoost { get; init; }
        public double GlassThicknessWidth { get; init; }
        public double GlassMassOpacity { get; init; }
        public double GlassMassOpenBoost { get; init; }
        public double ChromaticEdgeAlpha { get; init; }
        public double ChromaticEdgeOpenBoost { get; init; }
        public double ChromaticEdgeOffset { get; init; }
        public double ChromaticEdgeWidth { get; init; }
        public int MicroHighlightCount { get; init; }
        public double MicroHighlightAlpha { get; init; }
        public double MicroHighlightOpenBoost { get; init; }
        public double MicroHighlightSpeed { get; init; }
        public int CausticLineCount { get; init; }
        public double CausticAlpha { get; init; }
        public double CausticOpenBoost { get; init; }
        public double CausticWidth { get; init; }
        public double CausticSpeed { get; init; }
        public int SpecularSweepCount { get; init; }
        public double SpecularSweepAlpha { get; init; }
        public double SpecularSweepOpenBoost { get; init; }
        public double SpecularSweepWidth { get; init; }
        public double SpecularSweepSpeed { get; init; }
        public double ApertureOpacity { get; init; }
        public int ApertureRingCount { get; init; }
        public double ApertureRingAlpha { get; init; }
        public double CarryScaleBase { get; init; }
        public double CarryScaleRecovery { get; init; }

        public static LensVisualProfile For(GpuLivingLensLook look)
        {
            return look switch
            {
                GpuLivingLensLook.GlassBubble => new LensVisualProfile
                {
                    LensScale = 0.96,
                    RefractionOverscanX = 0.22,
                    RefractionOverscanY = 0.18,
                    BaseDesktopOpacity = 0.88,
                    RefractionLayers = 16,
                    RefractionCurve = 1.24,
                    RefractionDepth = 0.24,
                    RefractionCompression = 0.28,
                    RefractionPull = 10.0,
                    ShimmerSpeed = 0.44,
                    ShimmerAmount = 0.16,
                    RefractionLayerOpacity = 0.0035,
                    RefractionOpenBoost = 0.006,
                    GlassOpacity = 0.22,
                    GlassCoreAlpha = 14,
                    GlassMidAlpha = 7,
                    GlassEdgeAlpha = 28,
                    GlassRingCount = 6,
                    GlassRingAlpha = 12,
                    GlassRingFade = 1.60,
                    GlassRingOpenBoost = 2,
                    GlassRingWidth = 0.12,
                    GlassRingDepthX = 0.30,
                    GlassRingDepthY = 0.34,
                    GlassRingThroatPull = 0.08,
                    OuterRimAlpha = 42,
                    OuterRimWidth = 0.26,
                    OuterRimOpenWidth = 0.08,
                    FresnelOpacity = 0.18,
                    FresnelOpenBoost = 0.04,
                    HighlightAlpha = 136,
                    HighlightRadiusX = 0.14,
                    HighlightRadiusY = 0.085,
                    ThroatAlpha = 6,
                    ThroatOpenAlpha = 12,
                    ThroatRadiusX = 0.09,
                    ThroatRadiusY = 0.025,
                    TunnelRingCount = 4,
                    TunnelDepthCurve = 1.42,
                    TunnelDepthX = 0.28,
                    TunnelDepthY = 0.34,
                    TunnelOpenPull = 8,
                    TunnelBasePull = 4,
                    TunnelShimmer = 0.10,
                    TunnelLightAlpha = 5,
                    TunnelLightFade = 0.58,
                    TunnelLightOpenBoost = 3,
                    TunnelDarkAlpha = 2,
                    TunnelDarkOpenBoost = 3,
                    TunnelDarkFade = 0.22,
                    TunnelDarkWidth = 0.42,
                    TunnelLightWidth = 0.18,
                    LightRayCount = 2,
                    LightRayAlpha = 2,
                    LightRayOpenBoost = 3,
                    LightRayWidth = 0.14,
                    ReflectedEdgeOpacity = 0.22,
                    ReflectedEdgeOpenBoost = 0.03,
                    RibbonCount = 2,
                    RibbonAlpha = 4,
                    RibbonOpenBoost = 4,
                    RibbonWidth = 0.14,
                    RimShardOpacity = 0.14,
                    ContactShadowAlpha = 58,
                    ContactShadowOpacity = 0.34,
                    GlassThicknessLayers = 7,
                    GlassThicknessAlpha = 46,
                    GlassThicknessFade = 7.2,
                    GlassThicknessOpenBoost = 8,
                    GlassThicknessWidth = 0.34,
                    GlassMassOpacity = 0.34,
                    GlassMassOpenBoost = 0.08,
                    ChromaticEdgeAlpha = 9,
                    ChromaticEdgeOpenBoost = 5,
                    ChromaticEdgeOffset = 1.05,
                    ChromaticEdgeWidth = 0.34,
                    MicroHighlightCount = 32,
                    MicroHighlightAlpha = 18,
                    MicroHighlightOpenBoost = 14,
                    MicroHighlightSpeed = 0.72,
                    CausticLineCount = 7,
                    CausticAlpha = 19,
                    CausticOpenBoost = 10,
                    CausticWidth = 0.38,
                    CausticSpeed = 0.84,
                    SpecularSweepCount = 5,
                    SpecularSweepAlpha = 32,
                    SpecularSweepOpenBoost = 16,
                    SpecularSweepWidth = 2.1,
                    SpecularSweepSpeed = 0.34,
                    ApertureOpacity = 0.10,
                    ApertureRingCount = 1,
                    ApertureRingAlpha = 12,
                    CarryScaleBase = 0.62,
                    CarryScaleRecovery = 0.14
                },
                GpuLivingLensLook.Wormhole => new LensVisualProfile
                {
                    LensScale = 1.08,
                    RefractionOverscanX = 0.30,
                    RefractionOverscanY = 0.24,
                    BaseDesktopOpacity = 0.70,
                    RefractionLayers = 96,
                    RefractionCurve = 1.86,
                    RefractionDepth = 0.76,
                    RefractionCompression = 0.86,
                    RefractionPull = 54.0,
                    ShimmerSpeed = 0.82,
                    ShimmerAmount = 0.46,
                    RefractionLayerOpacity = 0.012,
                    RefractionOpenBoost = 0.036,
                    GlassOpacity = 0.32,
                    GlassCoreAlpha = 18,
                    GlassMidAlpha = 9,
                    GlassEdgeAlpha = 48,
                    GlassRingCount = 40,
                    GlassRingAlpha = 24,
                    GlassRingFade = 0.78,
                    GlassRingOpenBoost = 10,
                    GlassRingWidth = 0.24,
                    GlassRingDepthX = 0.64,
                    GlassRingDepthY = 0.78,
                    GlassRingThroatPull = 0.56,
                    OuterRimAlpha = 62,
                    OuterRimWidth = 0.38,
                    OuterRimOpenWidth = 0.18,
                    FresnelOpacity = 0.18,
                    FresnelOpenBoost = 0.04,
                    HighlightAlpha = 104,
                    HighlightRadiusX = 0.11,
                    HighlightRadiusY = 0.065,
                    ThroatAlpha = 74,
                    ThroatOpenAlpha = 50,
                    ThroatRadiusX = 0.22,
                    ThroatRadiusY = 0.078,
                    TunnelRingCount = 92,
                    TunnelDepthCurve = 1.92,
                    TunnelDepthX = 0.72,
                    TunnelDepthY = 0.86,
                    TunnelOpenPull = 54,
                    TunnelBasePull = 30,
                    TunnelShimmer = 0.34,
                    TunnelLightAlpha = 24,
                    TunnelLightFade = 0.26,
                    TunnelLightOpenBoost = 24,
                    TunnelDarkAlpha = 30,
                    TunnelDarkOpenBoost = 26,
                    TunnelDarkFade = 0.12,
                    TunnelDarkWidth = 1.12,
                    TunnelLightWidth = 0.34,
                    LightRayCount = 16,
                    LightRayAlpha = 9,
                    LightRayOpenBoost = 22,
                    LightRayWidth = 0.34,
                    ReflectedEdgeOpacity = 0.26,
                    ReflectedEdgeOpenBoost = 0.12,
                    RibbonCount = 10,
                    RibbonAlpha = 16,
                    RibbonOpenBoost = 28,
                    RibbonWidth = 0.32,
                    RimShardOpacity = 0.22,
                    ContactShadowAlpha = 72,
                    ContactShadowOpacity = 0.24,
                    GlassThicknessLayers = 4,
                    GlassThicknessAlpha = 22,
                    GlassThicknessFade = 4.2,
                    GlassThicknessOpenBoost = 4,
                    GlassThicknessWidth = 0.26,
                    GlassMassOpacity = 0.18,
                    GlassMassOpenBoost = 0.04,
                    ChromaticEdgeAlpha = 5,
                    ChromaticEdgeOpenBoost = 4,
                    ChromaticEdgeOffset = 0.72,
                    ChromaticEdgeWidth = 0.24,
                    MicroHighlightCount = 8,
                    MicroHighlightAlpha = 7,
                    MicroHighlightOpenBoost = 6,
                    MicroHighlightSpeed = 0.42,
                    CausticLineCount = 3,
                    CausticAlpha = 7,
                    CausticOpenBoost = 5,
                    CausticWidth = 0.28,
                    CausticSpeed = 0.42,
                    SpecularSweepCount = 2,
                    SpecularSweepAlpha = 11,
                    SpecularSweepOpenBoost = 8,
                    SpecularSweepWidth = 1.1,
                    SpecularSweepSpeed = 0.18,
                    ApertureOpacity = 0.52,
                    ApertureRingCount = 7,
                    ApertureRingAlpha = 56,
                    CarryScaleBase = 0.60,
                    CarryScaleRecovery = 0.13
                },
                _ => new LensVisualProfile
                {
                    LensScale = 1.03,
                    RefractionOverscanX = 0.26,
                    RefractionOverscanY = 0.21,
                    BaseDesktopOpacity = 0.76,
                    RefractionLayers = 46,
                    RefractionCurve = 1.62,
                    RefractionDepth = 0.54,
                    RefractionCompression = 0.64,
                    RefractionPull = 34.0,
                    ShimmerSpeed = 0.68,
                    ShimmerAmount = 0.32,
                    RefractionLayerOpacity = 0.010,
                    RefractionOpenBoost = 0.028,
                    GlassOpacity = 0.30,
                    GlassCoreAlpha = 20,
                    GlassMidAlpha = 10,
                    GlassEdgeAlpha = 44,
                    GlassRingCount = 18,
                    GlassRingAlpha = 18,
                    GlassRingFade = 1.05,
                    GlassRingOpenBoost = 5,
                    GlassRingWidth = 0.18,
                    GlassRingDepthX = 0.54,
                    GlassRingDepthY = 0.68,
                    GlassRingThroatPull = 0.46,
                    OuterRimAlpha = 54,
                    OuterRimWidth = 0.34,
                    OuterRimOpenWidth = 0.14,
                    FresnelOpacity = 0.20,
                    FresnelOpenBoost = 0.04,
                    HighlightAlpha = 126,
                    HighlightRadiusX = 0.13,
                    HighlightRadiusY = 0.078,
                    ThroatAlpha = 48,
                    ThroatOpenAlpha = 38,
                    ThroatRadiusX = 0.19,
                    ThroatRadiusY = 0.066,
                    TunnelRingCount = 34,
                    TunnelDepthCurve = 1.78,
                    TunnelDepthX = 0.62,
                    TunnelDepthY = 0.78,
                    TunnelOpenPull = 42,
                    TunnelBasePull = 24,
                    TunnelShimmer = 0.24,
                    TunnelLightAlpha = 18,
                    TunnelLightFade = 0.32,
                    TunnelLightOpenBoost = 22,
                    TunnelDarkAlpha = 16,
                    TunnelDarkOpenBoost = 18,
                    TunnelDarkFade = 0.16,
                    TunnelDarkWidth = 0.86,
                    TunnelLightWidth = 0.28,
                    LightRayCount = 6,
                    LightRayAlpha = 5,
                    LightRayOpenBoost = 10,
                    LightRayWidth = 0.20,
                    ReflectedEdgeOpacity = 0.22,
                    ReflectedEdgeOpenBoost = 0.06,
                    RibbonCount = 4,
                    RibbonAlpha = 8,
                    RibbonOpenBoost = 12,
                    RibbonWidth = 0.20,
                    RimShardOpacity = 0.18,
                    ContactShadowAlpha = 64,
                    ContactShadowOpacity = 0.29,
                    GlassThicknessLayers = 5,
                    GlassThicknessAlpha = 32,
                    GlassThicknessFade = 5.8,
                    GlassThicknessOpenBoost = 6,
                    GlassThicknessWidth = 0.30,
                    GlassMassOpacity = 0.26,
                    GlassMassOpenBoost = 0.06,
                    ChromaticEdgeAlpha = 7,
                    ChromaticEdgeOpenBoost = 4,
                    ChromaticEdgeOffset = 0.86,
                    ChromaticEdgeWidth = 0.30,
                    MicroHighlightCount = 12,
                    MicroHighlightAlpha = 10,
                    MicroHighlightOpenBoost = 8,
                    MicroHighlightSpeed = 0.52,
                    CausticLineCount = 5,
                    CausticAlpha = 13,
                    CausticOpenBoost = 7,
                    CausticWidth = 0.34,
                    CausticSpeed = 0.62,
                    SpecularSweepCount = 3,
                    SpecularSweepAlpha = 22,
                    SpecularSweepOpenBoost = 12,
                    SpecularSweepWidth = 1.6,
                    SpecularSweepSpeed = 0.26,
                    ApertureOpacity = 0.32,
                    ApertureRingCount = 4,
                    ApertureRingAlpha = 40,
                    CarryScaleBase = 0.61,
                    CarryScaleRecovery = 0.14
                }
            };
        }
    }

    private sealed class StopwatchClock
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        private long _lastMilliseconds;

        public int TakeElapsedMilliseconds()
        {
            var now = _stopwatch.ElapsedMilliseconds;
            var elapsed = Math.Clamp(now - _lastMilliseconds, 1, 34);
            _lastMilliseconds = now;
            return (int)elapsed;
        }
    }
}
