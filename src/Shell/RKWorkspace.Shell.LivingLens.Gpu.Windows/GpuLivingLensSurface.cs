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
    private BitmapSource? _desktopSample;
    private WPoint _thingCenter;
    private WPoint _targetCenter;
    private WPoint _lastTargetCenter;
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
        _thingCenter = new WPoint(screenBounds.Width * 0.34, screenBounds.Height * 0.52);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _thingCenter;
    }

    public GpuLivingLensSession Session => _session;

    public void ActivatePickAt(WPoint point)
    {
        _thingCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
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

        if (e.ChangedButton == MouseButton.Left && IsNearLens(point, 230) && _session.CanPullOutFromLens)
        {
            _thingCenter = LensCenter();
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
        DrawLens(drawingContext);
        DrawThing(drawingContext);
    }

    private void DrawLens(DrawingContext drawingContext)
    {
        if (_session.LensEmergence <= 0)
        {
            return;
        }

        var center = LensCenter();
        var emergence = EaseOut(_session.LensEmergence);
        var open = EaseOut(_session.LensOpen);
        var width = 210 * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.12));
        var height = 150 * (0.36 + (0.64 * emergence)) * (1.0 + (open * 0.05));
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);

        UpdateDesktopSample(bounds);

        var clip = new EllipseGeometry(bounds);
        drawingContext.PushClip(clip);
        if (_desktopSample is not null)
        {
            var refracted = new Rect(bounds.X - (bounds.Width * 0.16), bounds.Y - (bounds.Height * 0.12), bounds.Width * 1.32, bounds.Height * 1.24);
            drawingContext.DrawImage(_desktopSample, refracted);

            for (var index = 1; index <= 42; index++)
            {
                var layer = index / 42.0;
                var depthCurve = Math.Pow(layer, 1.42);
                var tunnelScale = 1.0 - (depthCurve * 0.52);
                var tunnelWidth = bounds.Width * tunnelScale;
                var tunnelHeight = bounds.Height * (1.0 - (depthCurve * 0.70));
                var tunnelShift = (open * 24.0 * depthCurve) + (Math.Sin(_phase * 1.35 + index) * 0.72);
                var tunnel = new Rect(
                    center.X - (tunnelWidth / 2) + tunnelShift,
                    center.Y - (tunnelHeight / 2) + (depthCurve * 16.0),
                    tunnelWidth,
                    tunnelHeight);

                drawingContext.PushOpacity(0.025 + (open * 0.040));
                drawingContext.DrawImage(_desktopSample, tunnel);
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

        for (var index = 0; index < 52; index++)
        {
            var layer = index / 51.0;
            var depthCurve = Math.Pow(layer, 1.68);
            var rx = (bounds.Width / 2) * (1.0 - (depthCurve * 0.60));
            var ry = (bounds.Height / 2) * (1.0 - (depthCurve * 0.76));
            if (rx < 5 || ry < 3)
            {
                continue;
            }

            var shift = new Vector(
                (open * depthCurve * 26.0) + (Math.Sin(_phase * 0.9 + (index * 0.37)) * 0.64),
                (depthCurve * 17.0) + (Math.Cos(_phase * 0.8 + index) * 0.36));
            var ringCenter = center + shift;
            var lightAlpha = (byte)Math.Clamp(40 - (index * 0.48) + (open * 28), 8, 78);
            var darkAlpha = (byte)Math.Clamp(20 + (open * 20) - (index * 0.24), 4, 42);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(darkAlpha, 12, 16, 17)), 1.25), ringCenter + new Vector(0, 1.1), rx, ry);
            drawingContext.DrawEllipse(null, new WPen(new SolidColorBrush(WColor.FromArgb(lightAlpha, 248, 252, 250)), 0.46 + (open * 0.18)), ringCenter, rx, ry);
        }

        for (var ray = 0; ray < 10; ray++)
        {
            var normalized = (ray / 9.0) - 0.5;
            var angle = (normalized * 2.12) + (Math.Sin(_phase * 0.72 + ray) * 0.025);
            var startRadiusX = bounds.Width * (0.10 + (open * 0.04));
            var startRadiusY = bounds.Height * (0.035 + (open * 0.018));
            var endRadiusX = bounds.Width * (0.46 - (Math.Abs(normalized) * 0.08));
            var endRadiusY = bounds.Height * (0.34 - (Math.Abs(normalized) * 0.05));
            var start = throatCenter + new Vector(Math.Cos(angle) * startRadiusX, Math.Sin(angle) * startRadiusY);
            var end = center + new Vector(Math.Cos(angle) * endRadiusX, Math.Sin(angle) * endRadiusY);
            var control = Interpolate(start, end, 0.58) + new Vector(open * 18.0, Math.Sin(_phase + ray) * 1.8);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.QuadraticBezierTo(control, end, true, false);
            }

            geometry.Freeze();
            var alpha = (byte)Math.Clamp(14 + (open * 22) - (Math.Abs(normalized) * 12), 5, 38);
            drawingContext.DrawGeometry(null, new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 255, 252)), 0.58), geometry);
        }

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

        if (lifted)
        {
            var shadowSuction = Math.Max(SmoothStep(progress), portalPull * 0.66);
            var shadowAlpha = (byte)Math.Clamp((56 + (Math.Abs(_session.TiltX) * 2.4) + (Math.Abs(_session.TiltY) * 2.0)) * opacity * (1.0 - (shadowSuction * 0.48)), 7, 96);
            var freeShadowCenter = new WPoint(center.X + _session.ShadowX, center.Y + (height * 0.48) + _session.ShadowY);
            var shadowCenter = Interpolate(freeShadowCenter, LensCenter() + new Vector(-18, 22), shadowSuction * 0.84);
            var shadowWidth = width * (0.58 + (Math.Abs(_session.TiltX) * 0.010)) * (1.0 - (shadowSuction * 0.48));
            var shadowHeight = height * (0.24 + (Math.Abs(_session.TiltY) * 0.007)) * (1.0 - (shadowSuction * 0.24));
            for (var layer = 8; layer >= 0; layer--)
            {
                var inflate = 6.0 + (layer * 4.2);
                var alpha = (byte)Math.Clamp(shadowAlpha / (2.5 + (layer * 0.92)), 3, 72);
                var shadowBounds = new Rect(
                    shadowCenter.X - (shadowWidth / 2) - inflate,
                    shadowCenter.Y - (shadowHeight / 2) - (inflate * 0.45),
                    shadowWidth + (inflate * 2),
                    shadowHeight + inflate);
                var shadowGeometry = CreateRectangularGeometry(shadowBounds, progress * 0.40, _session.TiltX * 0.18, _session.TiltY * 0.18, LensCenter(), shadowSuction * 0.20);
                drawingContext.DrawGeometry(new SolidColorBrush(WColor.FromArgb(alpha, 0, 0, 0)), null, shadowGeometry);
            }
        }

        var geometry = CreateThingGeometry(bounds, progress, _session.TiltX, _session.TiltY, portalPull);
        var fill = new LinearGradientBrush(WColor.FromArgb((byte)(242 * opacity), 252, 252, 246), WColor.FromArgb((byte)(216 * opacity), 214, 228, 230), 90);
        drawingContext.DrawGeometry(fill, new WPen(new SolidColorBrush(WColor.FromArgb((byte)(112 * opacity), 96, 112, 118)), 0.9), geometry);

        var text = new FormattedText(
            "Rechnung.pdf",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            Math.Max(8, 11 * scale),
            new SolidColorBrush(WColor.FromArgb((byte)(70 * opacity), 34, 42, 48)),
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
        var width = 54 + (pulse * 2.5);
        var height = 27 + (pulse * 1.2);
        var center = lensCenter + new Vector(-18 + (pulse * 2.0), 12);
        var bounds = new Rect(center.X - (width / 2), center.Y - (height / 2), width, height);
        var geometry = CreateRectangularGeometry(bounds, 0.72, -2.4, 1.2, lensCenter, 0.78);
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
        return SmoothStep(1.0 - (distance / 178.0));
    }

    private void UpdateDesktopSample(Rect lensBounds)
    {
        _sampleFrame++;
        if (_desktopSample is not null && _sampleFrame % 3 != 0)
        {
            return;
        }

        var x = Math.Max(_screenBounds.Left, _screenBounds.Left + (int)Math.Round(lensBounds.X - 34));
        var y = Math.Max(_screenBounds.Top, _screenBounds.Top + (int)Math.Round(lensBounds.Y - 28));
        var width = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Width + 68)), Math.Max(8, _screenBounds.Right - x));
        var height = Math.Min(Math.Max(8, (int)Math.Round(lensBounds.Height + 56)), Math.Max(8, _screenBounds.Bottom - y));
        var sample = new Int32Rect(x, y, width, height);
        _desktopSample = DesktopRefractionSampler.Capture(sample);
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

        return new WPoint(x, point.Y);
    }

    private Rect ThingBounds()
    {
        return new Rect(_thingCenter.X - 90, _thingCenter.Y - 52, 180, 104);
    }

    private WPoint LensCenter()
    {
        const double fullLensWidth = 236;
        var outside = fullLensWidth * (1.0 - _session.LensVisibleRatio);
        var inset = (fullLensWidth / 2.0) - outside;
        return new WPoint(RenderSize.Width - inset, RenderSize.Height * _session.LensY);
    }

    private bool IsNearLens(WPoint point, double radius)
    {
        return (point - LensCenter()).Length <= radius;
    }

    private double NormalizedLensNearness(WPoint point)
    {
        return Math.Clamp(1.0 - ((point - LensCenter()).Length / 240.0), 0.0, 1.0);
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
        var directionX = Math.Clamp(tiltX / 8.5, -1.0, 1.0);
        var directionY = Math.Clamp(-tiltY / 8.5, -1.0, 1.0);
        var pull = bounds.Width * 0.20 * absorption;
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        var topLeft = PerspectiveCorner(bounds, -1, -1, directionX, directionY, pull * 0.04, lensCenter, portalPull);
        var topRight = PerspectiveCorner(bounds, 1, -1, directionX, directionY, pull, lensCenter, portalPull);
        var bottomRight = PerspectiveCorner(bounds, 1, 1, directionX, directionY, pull * 0.58, lensCenter, portalPull);
        var bottomLeft = PerspectiveCorner(bounds, -1, 1, directionX, directionY, pull * 0.02, lensCenter, portalPull);
        context.BeginFigure(topLeft, true, true);
        context.LineTo(topRight, true, false);
        context.LineTo(bottomRight, true, false);
        context.LineTo(bottomLeft, true, false);
        geometry.Freeze();
        return geometry;
    }

    private static WPoint PerspectiveCorner(Rect bounds, int cornerX, int cornerY, double directionX, double directionY, double pull, WPoint lensCenter, double portalPull)
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
        if (portalPull <= 0)
        {
            return point;
        }

        var distance = (point - lensCenter).Length;
        var localPull = portalPull * SmoothStep(1.0 - (distance / (bounds.Width * 1.45)));
        if (localPull <= 0)
        {
            return point;
        }

        var vectorToLens = lensCenter - point;
        if (vectorToLens.Length > 0.001)
        {
            vectorToLens.Normalize();
        }

        var center = new WPoint(bounds.Left + (bounds.Width / 2), bounds.Top + (bounds.Height / 2));
        var towardCenter = center - point;
        return point + (vectorToLens * localPull * (24.0 + (bounds.Width * 0.18))) + (towardCenter * localPull * 0.10);
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
