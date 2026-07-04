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
    private bool _isHolding;
    private int _sampleFrame;
    private double _phase;

    public GpuLivingLensSurface(WorkspaceShellRuntime runtime, System.Drawing.Rectangle screenBounds)
    {
        _runtime = runtime;
        _screenBounds = screenBounds;
        Focusable = true;
        Cursor = WCursors.Arrow;
        _thingCenter = new WPoint(screenBounds.Width * 0.50, screenBounds.Height * 0.50);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _thingCenter;
        _activeLensCenter = new WPoint(screenBounds.Width - 88, screenBounds.Height * 0.50);
    }

    public GpuLivingLensSession Session => _session;

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
        _activeLensCenter = NearestLensCenter(_targetCenter);
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

        var emergence = EaseOut(_session.LensEmergence) * (0.54 + (activation * 0.46));
        var open = EaseOut(_session.LensOpen) * activation;
        var width = 210 * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.12));
        var height = 150 * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.05));
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);

        var desktopSample = UpdateDesktopSample(bounds, center, activation);

        var clip = new EllipseGeometry(bounds);
        drawingContext.PushClip(clip);
        if (desktopSample is not null)
        {
            var refracted = new Rect(bounds.X - (bounds.Width * 0.16), bounds.Y - (bounds.Height * 0.12), bounds.Width * 1.32, bounds.Height * 1.24);
            drawingContext.DrawImage(desktopSample, refracted);

            for (var index = 1; index <= 56; index++)
            {
                var layer = index / 56.0;
                var depthCurve = Math.Pow(layer, 1.48);
                var tunnelScale = 1.0 - (depthCurve * 0.56);
                var tunnelWidth = bounds.Width * tunnelScale;
                var tunnelHeight = bounds.Height * (1.0 - (depthCurve * 0.74));
                var tunnelShift = (open * 28.0 * depthCurve) + (Math.Sin(_phase * 1.08 + index) * 0.58);
                var tunnel = new Rect(
                    center.X - (tunnelWidth / 2) + tunnelShift,
                    center.Y - (tunnelHeight / 2) + (depthCurve * 18.0),
                    tunnelWidth,
                    tunnelHeight);

                drawingContext.PushOpacity(0.020 + (open * 0.038));
                drawingContext.DrawImage(desktopSample, tunnel);
                drawingContext.Pop();
            }
        }

        DrawTunnelVolume(drawingContext, bounds, center, open);

        var glass = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.34, 0.28),
            Center = new WPoint(0.48, 0.52),
            RadiusX = 0.76,
            RadiusY = 0.70,
            Opacity = 0.62
        };
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(64, 255, 255, 255), 0.0));
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(28, 232, 244, 246), 0.42));
        glass.GradientStops.Add(new GradientStop(WColor.FromArgb(104, 96, 116, 120), 1.0));
        drawingContext.DrawEllipse(glass, null, center, bounds.Width / 2, bounds.Height / 2);

        for (var index = 0; index < 34; index++)
        {
            var layer = index / 33.0;
            var depthCurve = Math.Pow(layer, 1.55);
            var alpha = (byte)Math.Clamp(42 - (index * 0.88) + (open * 26), 7, 76);
            var ringPen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 248, 255, 255)), 0.42 + (open * 0.20));
            var rx = (bounds.Width / 2) * (1.0 - (depthCurve * 0.50));
            var ry = (bounds.Height / 2) * (1.0 - (depthCurve * 0.68));
            var ringCenter = new WPoint(center.X + (depthCurve * open * 20), center.Y + (depthCurve * 15));
            drawingContext.DrawEllipse(null, ringPen, ringCenter, rx, ry);
        }

        drawingContext.Pop();

        var outer = new WPen(new SolidColorBrush(WColor.FromArgb(210, 255, 255, 255)), 1.8 + open);
        var inner = new WPen(new SolidColorBrush(WColor.FromArgb(86, 18, 24, 26)), 1.0);
        drawingContext.DrawEllipse(null, outer, center, bounds.Width / 2, bounds.Height / 2);
        drawingContext.DrawEllipse(null, inner, center + new Vector(0, 3), bounds.Width * 0.43, bounds.Height * 0.42);

        var highlight = new RadialGradientBrush(WColor.FromArgb(190, 255, 255, 255), WColor.FromArgb(0, 255, 255, 255));
        drawingContext.DrawEllipse(highlight, null, new WPoint(bounds.X + (bounds.Width * 0.66), bounds.Y + (bounds.Height * 0.23)), bounds.Width * 0.15, bounds.Height * 0.10);

        var depth = new RadialGradientBrush(WColor.FromArgb((byte)(70 + (open * 54)), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0));
        drawingContext.DrawEllipse(depth, null, new WPoint(bounds.X + (bounds.Width * 0.50), bounds.Y + (bounds.Height * 0.61)), bounds.Width * (0.17 + (open * 0.12)), bounds.Height * (0.06 + (open * 0.06)));
    }

    private void DrawTunnelVolume(DrawingContext drawingContext, Rect bounds, WPoint center, double open)
    {
        var throatCenter = center + new Vector(open * 20.0, 13.0);
        var throat = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.54, 0.48),
            Center = new WPoint(0.54, 0.52),
            RadiusX = 0.70,
            RadiusY = 0.54,
            Opacity = 0.78
        };
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(108 + (open * 38)), 8, 11, 12), 0.0));
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(58 + (open * 30)), 28, 34, 34), 0.42));
        throat.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            throat,
            null,
            throatCenter,
            bounds.Width * (0.20 + (open * 0.12)),
            bounds.Height * (0.075 + (open * 0.075)));

        for (var index = 0; index < 72; index++)
        {
            var layer = index / 71.0;
            var depthCurve = Math.Pow(layer, 1.74);
            var rx = (bounds.Width / 2) * (1.0 - (depthCurve * 0.64));
            var ry = (bounds.Height / 2) * (1.0 - (depthCurve * 0.80));
            if (rx < 5 || ry < 3)
            {
                continue;
            }

            var shift = new Vector(
                (open * depthCurve * 31.0) + (Math.Sin(_phase * 0.72 + (index * 0.31)) * 0.48),
                (depthCurve * 19.0) + (Math.Cos(_phase * 0.68 + index) * 0.28));
            var ringCenter = center + shift;
            var lightAlpha = (byte)Math.Clamp(34 - (index * 0.34) + (open * 34), 6, 82);
            var darkAlpha = (byte)Math.Clamp(22 + (open * 24) - (index * 0.18), 4, 46);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(darkAlpha, 10, 13, 14)), 1.34), ringCenter + new Vector(0, 1.1), rx, ry);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(lightAlpha, 250, 253, 250)), 0.38 + (open * 0.20)), ringCenter, rx, ry);
        }

        for (var ray = 0; ray < 16; ray++)
        {
            var normalized = (ray / 15.0) - 0.5;
            var angle = (normalized * 2.24) + (Math.Sin(_phase * 0.58 + ray) * 0.018);
            var startRadiusX = bounds.Width * (0.10 + (open * 0.04));
            var startRadiusY = bounds.Height * (0.035 + (open * 0.018));
            var endRadiusX = bounds.Width * (0.50 - (Math.Abs(normalized) * 0.09));
            var endRadiusY = bounds.Height * (0.38 - (Math.Abs(normalized) * 0.06));
            var start = throatCenter + new Vector(Math.Cos(angle) * startRadiusX, Math.Sin(angle) * startRadiusY);
            var end = center + new Vector(Math.Cos(angle) * endRadiusX, Math.Sin(angle) * endRadiusY);
            var control = Interpolate(start, end, 0.58) + new Vector(open * 22.0, Math.Sin(_phase * 0.82 + ray) * 1.4);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.QuadraticBezierTo(control, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(12 + (open * 24) - (Math.Abs(normalized) * 10), 4, 42);
            drawingContext.DrawGeometry(null, new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 255, 252)), 0.48), geometry);
        }

        DrawPremiumRefractionRibbons(drawingContext, bounds, center, throatCenter, open);
        DrawPremiumTunnelAperture(drawingContext, bounds, throatCenter, open);

        var reflectedEdge = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.06, 0.02),
            EndPoint = new WPoint(0.74, 0.84),
            Opacity = 0.42 + (open * 0.16)
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

    private void DrawPremiumRefractionRibbons(DrawingContext drawingContext, Rect bounds, WPoint center, WPoint throatCenter, double open)
    {
        for (var ribbon = 0; ribbon < 9; ribbon++)
        {
            var normalized = (ribbon / 8.0) - 0.5;
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
            var alpha = (byte)Math.Clamp(18 + (open * 32) - (Math.Abs(normalized) * 16), 4, 54);
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 250, 252, 248)), 0.42 + (open * 0.18));
            drawingContext.DrawGeometry(null, pen, geometry);
        }

        var rimShard = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.16, 0.10),
            EndPoint = new WPoint(0.94, 0.72),
            Opacity = 0.32 + (open * 0.18)
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

    private static void DrawPremiumTunnelAperture(DrawingContext drawingContext, Rect bounds, WPoint throatCenter, double open)
    {
        var apertureShadow = new RadialGradientBrush
        {
            GradientOrigin = new WPoint(0.47, 0.48),
            Center = new WPoint(0.50, 0.52),
            RadiusX = 0.64,
            RadiusY = 0.46,
            Opacity = 0.50 + (open * 0.18)
        };
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(118 + (open * 34)), 5, 7, 8), 0.0));
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(72 + (open * 26)), 16, 20, 21), 0.46));
        apertureShadow.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(
            apertureShadow,
            null,
            throatCenter + new Vector(open * 5.0, 1.5),
            bounds.Width * (0.18 + (open * 0.105)),
            bounds.Height * (0.052 + (open * 0.052)));

        for (var ring = 0; ring < 5; ring++)
        {
            var layer = ring / 4.0;
            var alpha = (byte)Math.Clamp(72 - (ring * 12) + (open * 28), 18, 110);
            var rx = bounds.Width * (0.18 + (open * 0.12) + (layer * 0.035));
            var ry = bounds.Height * (0.052 + (open * 0.050) + (layer * 0.014));
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 244, 250, 248)), 0.42 + (open * 0.18));
            drawingContext.DrawEllipse(null, pen, throatCenter + new Vector((open * 6.0) + (layer * 2.0), 1.0 + (layer * 0.7)), rx, ry);
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
            throatCenter + new Vector(-bounds.Width * 0.05, bounds.Height * 0.095),
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
        var center = Interpolate(_thingCenter, LensCenter(), progress * 0.88);
        var recovery = EaseOut(_session.PullOutRecovery);
        var lifted = _session.IsHoldingThing || _isHolding || progress > 0.01;
        var carryScale = lifted ? 0.72 + (recovery * 0.18) : 1.0;
        var scale = carryScale * Math.Clamp(1.0 - (progress * 0.62), 0.34, 1.0);
        var width = 178 * scale;
        var height = 94 * scale * (1.0 - (progress * 0.14));
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
        var opacity = progress < 0.78 ? 1.0 : Math.Clamp(1.0 - ((progress - 0.78) / 0.22), 0.12, 1.0);
        var portalPull = GetPortalPullAmount(bounds, lifted);
        var portalSqueeze = Math.Max(portalPull, SmoothStep(progress) * 0.84);

        if (lifted)
        {
            var shadowSuction = Math.Max(SmoothStep(progress), portalSqueeze * 0.88);
            var shadowAlpha = (byte)Math.Clamp((58 + (Math.Abs(_session.TiltX) * 2.2) + (Math.Abs(_session.TiltY) * 1.8)) * opacity * (1.0 - (shadowSuction * 0.52)), 5, 96);
            var freeShadowCenter = new WPoint(center.X + _session.ShadowX, center.Y + (height * 0.48) + _session.ShadowY);
            var shadowCenter = Interpolate(freeShadowCenter, LensCenter() + new Vector(-22, 20), shadowSuction * 0.92);
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
                    LensCenter(),
                    shadowSuction * 0.74);
                drawingContext.DrawGeometry(new SolidColorBrush(WColor.FromArgb(alpha, 0, 0, 0)), null, shadowGeometry);
            }
        }

        var geometry = CreateThingGeometry(bounds, progress, _session.TiltX, _session.TiltY, portalSqueeze);
        var fill = new LinearGradientBrush(WColor.FromArgb((byte)(242 * opacity), 252, 252, 246), WColor.FromArgb((byte)(216 * opacity), 214, 228, 230), 90);
        drawingContext.DrawGeometry(fill, new WPen(new SolidColorBrush(WColor.FromArgb((byte)(112 * opacity), 96, 112, 118)), 0.9), geometry);

        var text = new FormattedText(
            "Rechnung.pdf",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            Math.Max(8, 11 * scale),
            new SolidColorBrush(WColor.FromArgb((byte)(70 * opacity * (1.0 - (portalSqueeze * 0.48))), 34, 42, 48)),
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
        var pulse = 0.5 + (Math.Sin(_phase * 2.2) * 0.5);
        var remaining = _session.TransitRemainingMilliseconds / (double)_session.TransitTimeoutMilliseconds;
        var opacity = Math.Clamp(0.24 + (remaining * 0.24) + (pulse * 0.05), 0.18, 0.52);
        var width = 46 + (pulse * 1.4);
        var height = 22 + (pulse * 0.8);
        var center = lensCenter + new Vector(-18 + (pulse * 2.0), 12);
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
        var geometry = CreateRectangularGeometry(bounds, 0.0, 0.4, -0.2, lensCenter, 0.0);
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

        var lensCenter = LensCenter();
        var nearest = new WPoint(
            Math.Clamp(lensCenter.X, bounds.Left, bounds.Right),
            Math.Clamp(lensCenter.Y, bounds.Top, bounds.Bottom));
        var distance = (nearest - lensCenter).Length;
        return SmoothStep(1.0 - (distance / 214.0));
    }

    private BitmapSource? UpdateDesktopSample(Rect lensBounds, WPoint center, double activation)
    {
        var key = $"{Math.Round(center.X)}:{Math.Round(center.Y)}";
        _sampleFrame++;
        var refreshEvery = activation > 0.74 ? 3 : 18;
        if (_lensSamples.TryGetValue(key, out var cached) && _sampleFrame % refreshEvery != 0)
        {
            return cached;
        }

        var x = Math.Max(_screenBounds.Left, _screenBounds.Left + (int)Math.Round(lensBounds.X - 34));
        var y = Math.Max(_screenBounds.Top, _screenBounds.Top + (int)Math.Round(lensBounds.Y - 28));
        var width = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Width + 68)), Math.Max(8, _screenBounds.Right - x));
        var height = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Height + 56)), Math.Max(8, _screenBounds.Bottom - y));
        var sample = new Int32Rect(x, y, width, height);
        var desktopSample = DesktopRefractionSampler.Capture(sample);
        if (desktopSample is not null)
        {
            _lensSamples[key] = desktopSample;
        }

        return desktopSample;
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

    private double LensActivation(WPoint center)
    {
        var isActive = (center - _activeLensCenter).Length < 1.0;
        if (_isHolding || _session.IsHoldingThing)
        {
            var nearness = SmoothStep(1.0 - ((_targetCenter - center).Length / 300.0));
            return isActive
                ? 0.74 + (nearness * 0.26)
                : 0.28 + (nearness * 0.26);
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

    private StreamGeometry CreateThingGeometry(Rect bounds, double absorption, double tiltX, double tiltY, double portalPull)
    {
        return CreateRectangularGeometry(bounds, absorption, tiltX, tiltY, LensCenter(), portalPull);
    }

    private static StreamGeometry CreateRectangularGeometry(Rect bounds, double absorption, double tiltX, double tiltY, WPoint lensCenter, double portalPull)
    {
        var portalAmount = SmoothStep(portalPull);
        var tiltDamping = 1.0 - (portalAmount * 0.88);
        var directionX = Math.Clamp(tiltX / 8.5, -1.0, 1.0) * tiltDamping;
        var directionY = Math.Clamp(-tiltY / 8.5, -1.0, 1.0) * tiltDamping;
        var pull = bounds.Width * 0.08 * absorption * (1.0 - (portalAmount * 0.62));
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        var topLeft = PerspectiveCorner(bounds, -1, -1, directionX, directionY, pull, lensCenter, portalAmount);
        var topRight = PerspectiveCorner(bounds, 1, -1, directionX, directionY, pull, lensCenter, portalAmount);
        var bottomRight = PerspectiveCorner(bounds, 1, 1, directionX, directionY, pull, lensCenter, portalAmount);
        var bottomLeft = PerspectiveCorner(bounds, -1, 1, directionX, directionY, pull, lensCenter, portalAmount);
        context.BeginFigure(topLeft, true, true);
        context.LineTo(topRight, true, false);
        context.LineTo(bottomRight, true, false);
        context.LineTo(bottomLeft, true, false);
        geometry.Freeze();
        return geometry;
    }

    private static WPoint PerspectiveCorner(Rect bounds, int cornerX, int cornerY, double directionX, double directionY, double pull, WPoint lensCenter, double portalAmount)
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
        var vectorToLens = lensCenter - center;
        if (vectorToLens.Length > 0.001)
        {
            vectorToLens.Normalize();
        }
        else
        {
            vectorToLens = new Vector(1, 0);
        }

        var cornerOffset = new Vector((bounds.Width / 2.0) * cornerX, (bounds.Height / 2.0) * cornerY);
        var cornerProjection = (cornerOffset.X * vectorToLens.X) + (cornerOffset.Y * vectorToLens.Y);
        var maxProjection = (Math.Abs(vectorToLens.X) * bounds.Width / 2.0) + (Math.Abs(vectorToLens.Y) * bounds.Height / 2.0);
        var leadingDistance = Math.Max(0.0, maxProjection - cornerProjection);
        var leadingBand = Math.Max(18.0, Math.Min(bounds.Width, bounds.Height) * 0.74);
        var leadWeight = SmoothStep(1.0 - (leadingDistance / leadingBand));
        var supportWeight = SmoothStep(1.0 - (leadingDistance / (leadingBand * 1.92))) * 0.18;
        var collapseWeight = Math.Clamp(leadWeight + supportWeight, 0.0, 1.0);
        var apexDistance = maxProjection + (bounds.Width * (0.14 + (portalAmount * 0.40)));
        var apex = center + (vectorToLens * apexDistance);
        var stableDrift = vectorToLens * portalAmount * bounds.Width * 0.045;
        if (collapseWeight <= 0.01)
        {
            return point + stableDrift;
        }

        var collapse = Math.Clamp((portalAmount - 0.10) / 0.90, 0.0, 1.0);
        collapse = SmoothStep(collapse) * (0.965 * collapseWeight);
        var collapsed = Interpolate(point + stableDrift, apex, collapse);
        var funnelPull = vectorToLens * portalAmount * bounds.Width * 0.11 * collapseWeight;
        return collapsed + funnelPull;
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
