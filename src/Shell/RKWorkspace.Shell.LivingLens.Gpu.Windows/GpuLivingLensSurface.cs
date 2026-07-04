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

        if (e.ChangedButton == MouseButton.Left && IsNearLens(point, 230) && _session.Absorption >= 0.98f)
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
        var movement = _targetCenter - _lastTargetCenter;
        _lastTargetCenter = _targetCenter;
        _session.Carry((float)movement.X, (float)movement.Y);
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

            for (var index = 1; index <= 7; index++)
            {
                var layer = index / 7.0;
                var tunnelScale = 1.0 - (layer * 0.28);
                var tunnelWidth = bounds.Width * tunnelScale;
                var tunnelHeight = bounds.Height * (1.0 - (layer * 0.36));
                var tunnelShift = (open * 8.0 * layer) + (Math.Sin(_phase * 1.6 + index) * 1.5);
                var tunnel = new Rect(
                    center.X - (tunnelWidth / 2) + tunnelShift,
                    center.Y - (tunnelHeight / 2) + (layer * 5.0),
                    tunnelWidth,
                    tunnelHeight);

                drawingContext.PushOpacity(0.10 + (open * 0.035));
                drawingContext.DrawImage(_desktopSample, tunnel);
                drawingContext.Pop();
            }
        }

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

        for (var index = 0; index < 12; index++)
        {
            var layer = index / 11.0;
            var alpha = (byte)Math.Clamp(48 - (index * 3) + (open * 20), 10, 72);
            var ringPen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 248, 255, 255)), 0.55 + (open * 0.18));
            var rx = (bounds.Width / 2) * (1.0 - (layer * 0.44));
            var ry = (bounds.Height / 2) * (1.0 - (layer * 0.54));
            var ringCenter = new WPoint(center.X + (layer * open * 12), center.Y + (layer * 8));
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

    private void DrawThing(DrawingContext drawingContext)
    {
        if (_session.Absorption >= 1 && !_isHolding)
        {
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

        if (lifted)
        {
            var shadowAlpha = (byte)Math.Clamp((92 + (Math.Abs(_session.TiltX) * 5.0) + (Math.Abs(_session.TiltY) * 4.0)) * opacity, 20, 150);
            var shadowBrush = new RadialGradientBrush(WColor.FromArgb(shadowAlpha, 0, 0, 0), WColor.FromArgb(0, 0, 0, 0));
            var shadowCenter = new WPoint(center.X + _session.ShadowX, center.Y + (height * 0.48) + _session.ShadowY);
            drawingContext.DrawEllipse(shadowBrush, null, shadowCenter, width * (0.45 + (Math.Abs(_session.TiltX) * 0.012)), height * (0.15 + (Math.Abs(_session.TiltY) * 0.008)));
        }

        drawingContext.PushTransform(new RotateTransform(_session.TiltX * 0.18, center.X, center.Y));
        var geometry = CreateThingGeometry(bounds, progress, _session.TiltX, _session.TiltY);
        var fill = new LinearGradientBrush(WColor.FromArgb((byte)(242 * opacity), 252, 252, 246), WColor.FromArgb((byte)(216 * opacity), 214, 228, 230), 90);
        drawingContext.DrawGeometry(fill, new WPen(new SolidColorBrush(WColor.FromArgb((byte)(112 * opacity), 96, 112, 118)), 0.9), geometry);

        if (_session.IsHoldingThing || progress > 0)
        {
            drawingContext.PushClip(geometry);
            var grip = new LinearGradientBrush(WColor.FromArgb((byte)(70 * opacity), 18, 22, 22), WColor.FromArgb(0, 18, 22, 22), 0);
            drawingContext.DrawEllipse(grip, null, new WPoint(bounds.X + (bounds.Width * 0.18), bounds.Y + (bounds.Height * 0.50)), bounds.Width * 0.34, bounds.Height * 0.58);
            drawingContext.Pop();
        }

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
        drawingContext.Pop();
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

    private static StreamGeometry CreateThingGeometry(Rect bounds, double absorption, double tiltX, double tiltY)
    {
        var directionX = Math.Clamp(tiltX / 12.0, -1.0, 1.0);
        var directionY = Math.Clamp(-tiltY / 12.0, -1.0, 1.0);
        var pull = bounds.Width * 0.20 * absorption;
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        var topLeft = PerspectiveCorner(bounds, -1, -1, directionX, directionY, pull * 0.04);
        var topRight = PerspectiveCorner(bounds, 1, -1, directionX, directionY, pull);
        var bottomRight = PerspectiveCorner(bounds, 1, 1, directionX, directionY, pull * 0.58);
        var bottomLeft = PerspectiveCorner(bounds, -1, 1, directionX, directionY, pull * 0.02);
        context.BeginFigure(topLeft, true, true);
        context.BezierTo(Interpolate(topLeft, topRight, 0.32) + new Vector(0, -7), Interpolate(topLeft, topRight, 0.68) + new Vector(0, 5), topRight, true, false);
        context.BezierTo(new WPoint(bounds.Right + 4 + pull, bounds.Top + bounds.Height * 0.42), new WPoint(bounds.Right + 2 + pull, bounds.Bottom - 16), bottomRight, true, false);
        context.BezierTo(Interpolate(bottomRight, bottomLeft, 0.30) + new Vector(0, 8), Interpolate(bottomRight, bottomLeft, 0.66) + new Vector(0, -4), bottomLeft, true, false);
        context.BezierTo(new WPoint(bounds.Left - 2, bounds.Top + bounds.Height * 0.55), new WPoint(bounds.Left + 2, bounds.Top + 18), topLeft, true, false);
        geometry.Freeze();
        return geometry;
    }

    private static WPoint PerspectiveCorner(Rect bounds, int cornerX, int cornerY, double directionX, double directionY, double pull)
    {
        const double baseInsetX = 13;
        const double baseInsetY = 2;
        var x = cornerX < 0 ? bounds.Left + baseInsetX : bounds.Right - baseInsetX;
        var y = cornerY < 0 ? bounds.Top + baseInsetY : bounds.Bottom - baseInsetY;
        var dot = (cornerX * directionX) + (cornerY * directionY);
        var recede = Math.Max(0, dot);
        var forward = Math.Max(0, -dot);
        var offsetX = (-cornerX * recede * bounds.Width * 0.095) + (cornerX * forward * bounds.Width * 0.060);
        var offsetY = (-cornerY * recede * bounds.Height * 0.140) + (cornerY * forward * bounds.Height * 0.085);
        return new WPoint(x + offsetX + pull, y + offsetY);
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
