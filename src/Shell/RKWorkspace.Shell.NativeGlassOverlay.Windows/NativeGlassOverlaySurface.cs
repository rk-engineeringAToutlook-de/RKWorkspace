using RKWorkspace.Shell;
using RKWorkspace.Frame.Pdf;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WColor = System.Windows.Media.Color;
using WCursors = System.Windows.Input.Cursors;
using WPen = System.Windows.Media.Pen;
using WPoint = System.Windows.Point;
using WRect = System.Windows.Rect;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativeGlassOverlaySurface : FrameworkElement
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly System.Drawing.Rectangle _screenBounds;
    private readonly string _sourcePdfPath;
    private readonly string _sourceFileName;
    private readonly string _placementSignalPath;
    private readonly ImageSource? _pdfPreview;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly NativeGlassOverlaySession _session = new();
    private WPoint _thingCenter;
    private WPoint _targetCenter;
    private WPoint _lastTargetCenter;
    private WPoint _lensCenter;
    private Vector _velocity;
    private Vector _grabOffset;
    private bool _isHolding;
    private bool _captureExclusionReady;
    private bool _pendingRemotePlacement;
    private bool _placementSignalWritten;
    private long _lastMilliseconds;
    private double _phase;

    public NativeGlassOverlaySurface(WorkspaceShellRuntime runtime, System.Drawing.Rectangle screenBounds, NativeGlassOverlayOptions options)
    {
        _runtime = runtime;
        _screenBounds = screenBounds;
        _sourcePdfPath = options.SourcePdfPath;
        _sourceFileName = Path.GetFileName(options.SourcePdfPath);
        _placementSignalPath = options.PlacementSignalPath;
        _pdfPreview = TryLoadPdfPreview(options.SourcePdfPath);
        Focusable = true;
        Cursor = WCursors.Arrow;
        Width = screenBounds.Width;
        Height = screenBounds.Height;
        SnapsToDevicePixels = false;
        UseLayoutRounding = false;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

        _thingCenter = new WPoint(screenBounds.Width * 0.38, screenBounds.Height * 0.50);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _thingCenter;
        _lensCenter = new WPoint(screenBounds.Width - 30, screenBounds.Height * 0.52);
    }

    public NativeGlassOverlaySession Session => _session;

    public void MarkCaptureExclusion(bool ready)
    {
        _captureExclusionReady = ready;
    }

    public void ActivatePickAt(WPoint point)
    {
        if (!IsValidPoint(point))
        {
            point = _thingCenter;
        }

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
        var elapsed = TakeElapsedMilliseconds();
        _phase += elapsed / 1000.0;
        _session.Advance(elapsed);

        if (_pendingRemotePlacement &&
            !_placementSignalWritten &&
            _session.TransitRemainingMilliseconds <= 0 &&
            _session.State is NativeGlassOverlayCarryState.Closing or NativeGlassOverlayCarryState.Closed or NativeGlassOverlayCarryState.PlacedRemote)
        {
            SignalPlacementReady();
            _placementSignalWritten = true;
            _pendingRemotePlacement = false;
        }

        if (_session.State == NativeGlassOverlayCarryState.InTransit && !_isHolding)
        {
            _targetCenter = Interpolate(_targetCenter, _lensCenter, 0.035);
        }

        var delta = _targetCenter - _thingCenter;
        var spring = _isHolding ? 0.31 : 0.15;
        var damping = _isHolding ? 0.48 : 0.70;
        _velocity = new Vector(
            (_velocity.X * damping) + (delta.X * spring),
            (_velocity.Y * damping) + (delta.Y * spring));
        _thingCenter += _velocity;

        if (_isHolding && _velocity.Length > 0.04)
        {
            _session.Carry((float)_velocity.X, (float)_velocity.Y);
        }

        InvalidateVisual();
    }

    public NativeGlassShaderSnapshot CreateShaderSnapshot()
    {
        if (_session.LensEmergence <= 0.001f)
        {
            return default;
        }

        var bounds = LensBounds();
        var capture = DesktopSampleRect(bounds);
        var render = new WRect(
            capture.X - _screenBounds.Left,
            capture.Y - _screenBounds.Top,
            capture.Width,
            capture.Height);

        var intensity = Math.Clamp(
            (_session.LensEmergence * 0.52) +
            (_session.Approach * 0.26) +
            (_session.LensOpen * 0.22),
            0.0,
            1.0);

        return new NativeGlassShaderSnapshot(
            true,
            capture,
            render,
            (_lensCenter.X - render.X) / render.Width,
            (_lensCenter.Y - render.Y) / render.Height,
            (bounds.Width / 2.0) / render.Width,
            (bounds.Height / 2.0) / render.Height,
            _session.LensOpen,
            intensity,
            _phase);
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
        var point = e.GetPosition(this);
        if (e.ChangedButton == MouseButton.Left && ThingBounds().Contains(point) && IsThingVisible())
        {
            _isHolding = true;
            _grabOffset = point - _thingCenter;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.Pick();
            _pendingRemotePlacement = false;
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            e.Handled = true;
            return;
        }

        if (e.ChangedButton == MouseButton.Left && IsNearLens(point, 230) && _session.CanPullOut)
        {
            _thingCenter = _lensCenter;
            _targetCenter = point;
            _lastTargetCenter = point;
            _velocity = default;
            _grabOffset = default;
            _isHolding = true;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.PullOut();
            _pendingRemotePlacement = false;
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

        var point = e.GetPosition(this);
        _targetCenter = point - _grabOffset;
        var movement = _targetCenter - _lastTargetCenter;
        _lastTargetCenter = _targetCenter;
        _session.Carry((float)movement.X, (float)movement.Y);

        if (IsOverPortalEdge(_targetCenter, ThingBounds()))
        {
            _session.ApproachLens(1f);
        }
        else if (IsNearLens(_targetCenter, 245))
        {
            _session.ApproachLens((float)LensNearness(_targetCenter));
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
        if (IsOverPortalEdge(_targetCenter, ThingBounds()) || IsNearLens(_targetCenter, 228))
        {
            _session.ApproachLens(1f);
            _session.PlaceIntoLens();
            _pendingRemotePlacement = true;
            _targetCenter = Interpolate(_targetCenter, _lensCenter, 0.40);
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.NearSurface, "HX-002");
        }
        else
        {
            _session.PlaceOnDesktop();
            _pendingRemotePlacement = false;
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
        }

        e.Handled = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        DrawLens(drawingContext);
        DrawThing(drawingContext);
        DrawPortalDirectionGuide(drawingContext);
        DrawGlassPortalEdge(drawingContext);
        DrawMicroStatus(drawingContext);
    }

    private void DrawGlassPortalEdge(DrawingContext drawingContext)
    {
        var edge = PortalEdgeBounds();
        var active = Math.Clamp(
            (_session.LensEmergence * 0.62) +
            (_session.PickProgress * 0.24) +
            (_session.Approach * 0.34),
            0.0,
            1.0);
        var opacity = 0.72 + (active * 0.28);

        drawingContext.PushOpacity(opacity);

        var backShadow = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.0, 0.5),
            EndPoint = new WPoint(1.0, 0.5)
        };
        backShadow.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 0, 0, 0), 0.00));
        backShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(26 + (active * 40)), 0, 0, 0), 0.58));
        backShadow.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(70 + (active * 60)), 0, 0, 0), 1.00));
        drawingContext.DrawRectangle(backShadow, null, new WRect(edge.X - 120, edge.Y, edge.Width + 120, edge.Height));

        var body = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.0, 0.5),
            EndPoint = new WPoint(1.0, 0.5)
        };
        body.GradientStops.Add(new GradientStop(WColor.FromArgb(16, 255, 255, 255), 0.00));
        body.GradientStops.Add(new GradientStop(WColor.FromArgb(92, 255, 255, 255), 0.18));
        body.GradientStops.Add(new GradientStop(WColor.FromArgb(156, 248, 252, 255), 0.46));
        body.GradientStops.Add(new GradientStop(WColor.FromArgb(74, 255, 255, 255), 0.72));
        body.GradientStops.Add(new GradientStop(WColor.FromArgb(8, 255, 255, 255), 1.00));
        drawingContext.DrawRoundedRectangle(body, null, edge, 24, 24);

        var innerLight = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.0, 0.0),
            EndPoint = new WPoint(0.0, 1.0)
        };
        innerLight.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.00));
        innerLight.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(102 + (active * 82)), 255, 255, 255), 0.30));
        innerLight.GradientStops.Add(new GradientStop(WColor.FromArgb((byte)(68 + (active * 76)), 220, 242, 255), 0.58));
        innerLight.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.00));
        drawingContext.DrawRoundedRectangle(innerLight, null, new WRect(edge.X + edge.Width * 0.34, edge.Y + 24, edge.Width * 0.34, edge.Height - 48), 16, 16);

        var edgeLine = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(186 + (active * 58)), 255, 255, 255)), 3.2 + (active * 1.6));
        drawingContext.DrawLine(edgeLine, new WPoint(edge.X + edge.Width * 0.48, edge.Y + 24), new WPoint(edge.X + edge.Width * 0.48, edge.Bottom - 24));

        var glassRim = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(128 + (active * 86)), 255, 255, 255)), 1.8);
        drawingContext.DrawRoundedRectangle(null, glassRim, edge, 24, 24);

        var portalMouth = new WPoint(edge.X + (edge.Width * 0.36), _lensCenter.Y);
        var depth = new RadialGradientBrush(WColor.FromArgb((byte)(80 + (active * 132)), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.54,
            RadiusY = 0.74,
            Opacity = 0.88
        };
        drawingContext.DrawEllipse(depth, null, portalMouth, edge.Width * (0.32 + active * 0.16), edge.Height * (0.18 + active * 0.10));

        if (active > 0.16)
        {
            for (var index = 0; index < 8; index++)
            {
                var t = index / 7.0;
                var alpha = (byte)Math.Clamp((42 - (t * 24)) + (active * 44), 12, 86);
                var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 255, 255, 255)), 0.7 + active);
                drawingContext.DrawEllipse(
                    null,
                    pen,
                    portalMouth + new Vector(Math.Sin(_phase * 0.9 + index) * 1.2, 0),
                    edge.Width * (0.14 + (t * 0.19) + (active * 0.04)),
                    edge.Height * (0.055 + (t * 0.045) + (active * 0.035)));
            }
        }

        if (_session.PickProgress > 0.10f || _session.State == NativeGlassOverlayCarryState.InTransit || opacity > 0.40)
        {
            DrawPortalLabel(drawingContext, edge, active);
        }

        drawingContext.Pop();
    }

    private void DrawPortalLabel(DrawingContext drawingContext, WRect edge, double active)
    {
        var text = new FormattedText(
            "macOS",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI Semibold"),
            15,
            new SolidColorBrush(WColor.FromArgb((byte)(196 + (active * 50)), 255, 255, 255)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = 150,
            MaxLineCount = 1,
            Trimming = TextTrimming.CharacterEllipsis
        };

        drawingContext.PushTransform(new RotateTransform(-90, edge.X + 30, edge.Y + (edge.Height / 2)));
        drawingContext.DrawText(text, new WPoint(edge.X + 30 - (text.Width / 2), edge.Y + (edge.Height / 2) - (text.Height / 2)));
        drawingContext.Pop();
    }

    private void DrawPortalDirectionGuide(DrawingContext drawingContext)
    {
        if (!_isHolding || _session.PickProgress <= 0.08f)
        {
            return;
        }

        var active = Math.Clamp((_session.PickProgress * 0.48) + (_session.Approach * 0.52), 0.0, 1.0);
        var start = new WPoint(_thingCenter.X + (ThingBounds().Width * 0.58), _thingCenter.Y);
        var end = new WPoint(PortalEdgeBounds().X + 18, _lensCenter.Y);
        var distance = Distance(start, end);
        if (distance < 120)
        {
            return;
        }

        var pen = new WPen(new SolidColorBrush(WColor.FromArgb((byte)(34 + active * 74), 236, 250, 255)), 1.0)
        {
            DashStyle = new DashStyle(new[] { 7.0, 10.0 }, (_phase * 14.0) % 17.0)
        };
        drawingContext.DrawLine(pen, start, end);

        var direction = end - start;
        direction.Normalize();
        var normal = new Vector(-direction.Y, direction.X);
        var tip = end - (direction * 10);
        var arrow = new StreamGeometry();
        using (var context = arrow.Open())
        {
            context.BeginFigure(tip, true, true);
            context.LineTo(tip - (direction * 18) + (normal * 7), true, false);
            context.LineTo(tip - (direction * 18) - (normal * 7), true, false);
        }

        arrow.Freeze();
        drawingContext.DrawGeometry(new SolidColorBrush(WColor.FromArgb((byte)(72 + active * 84), 236, 250, 255)), null, arrow);
    }

    private void DrawLens(DrawingContext drawingContext)
    {
        if (_session.LensEmergence <= 0.001f)
        {
            return;
        }

        var bounds = LensBounds();
        var emergence = EaseOut(_session.LensEmergence);
        var open = EaseOut(_session.LensOpen);
        var center = _lensCenter;

        var contactShadow = new RadialGradientBrush(WColor.FromArgb(66, 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.72,
            RadiusY = 0.38,
            Opacity = 0.22 + (open * 0.12)
        };
        drawingContext.DrawEllipse(contactShadow, null, center + new Vector(bounds.Width * 0.02, bounds.Height * 0.20), bounds.Width * 0.43, bounds.Height * 0.15);

        var glassBody = new RadialGradientBrush
        {
            Center = new WPoint(0.49, 0.52),
            GradientOrigin = new WPoint(0.34, 0.24),
            RadiusX = 0.78,
            RadiusY = 0.70,
            Opacity = 0.18 + (open * 0.04)
        };
        glassBody.GradientStops.Add(new GradientStop(WColor.FromArgb(10, 255, 255, 255), 0.0));
        glassBody.GradientStops.Add(new GradientStop(WColor.FromArgb(4, 255, 255, 255), 0.44));
        glassBody.GradientStops.Add(new GradientStop(WColor.FromArgb(32, 224, 238, 240), 0.86));
        glassBody.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(glassBody, null, center, bounds.Width / 2, bounds.Height / 2);

        for (var index = 0; index < 12; index++)
        {
            var t = index / 11.0;
            var alpha = (byte)Math.Clamp((26 - (t * 18)) * emergence, 2, 26);
            var pen = new WPen(new SolidColorBrush(WColor.FromArgb(alpha, 246, 255, 255)), 0.12 + (t * 0.035));
            var offset = Math.Sin((_phase * 0.36) + index) * 0.65;
            drawingContext.DrawEllipse(
                null,
                pen,
                center + new Vector(offset, t * open * 16),
                (bounds.Width / 2) * (1.0 - (t * 0.44)),
                (bounds.Height / 2) * (1.0 - (t * 0.56)));
        }

        var fresnel = new RadialGradientBrush
        {
            Center = new WPoint(0.50, 0.50),
            GradientOrigin = new WPoint(0.38, 0.28),
            RadiusX = 0.76,
            RadiusY = 0.72,
            Opacity = 0.22
        };
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.58));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(12, 255, 255, 255), 0.80));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(54, 255, 255, 255), 0.96));
        fresnel.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 1.0));
        drawingContext.DrawEllipse(fresnel, null, center, bounds.Width / 2, bounds.Height / 2);

        var highlight = new RadialGradientBrush(WColor.FromArgb(138, 255, 255, 255), WColor.FromArgb(0, 255, 255, 255))
        {
            Opacity = 0.58
        };
        drawingContext.DrawEllipse(highlight, null, new WPoint(bounds.X + (bounds.Width * 0.64), bounds.Y + (bounds.Height * 0.22)), bounds.Width * 0.13, bounds.Height * 0.07);

        if (open > 0.04)
        {
            var throat = LensThroatPoint();
            var depth = new RadialGradientBrush(WColor.FromArgb((byte)(72 * open), 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
            {
                RadiusX = 0.72,
                RadiusY = 0.34,
                Opacity = 0.72
            };
            drawingContext.DrawEllipse(depth, null, throat, bounds.Width * (0.13 + (open * 0.12)), bounds.Height * (0.035 + (open * 0.06)));
        }
    }

    private void DrawThing(DrawingContext drawingContext)
    {
        if (!IsThingVisible())
        {
            return;
        }

        var bounds = ThingBounds();
        var corners = PaperCorners(bounds);
        DrawPaperShadow(drawingContext, corners, bounds);

        var geometry = Polygon(corners);
        var fill = new LinearGradientBrush
        {
            StartPoint = new WPoint(0.50, 0.00),
            EndPoint = new WPoint(0.50, 1.00)
        };
        fill.GradientStops.Add(new GradientStop(WColor.FromArgb(246, 252, 252, 248), 0.0));
        fill.GradientStops.Add(new GradientStop(WColor.FromArgb(236, 228, 235, 238), 1.0));
        var border = new WPen(new SolidColorBrush(WColor.FromArgb(156, 96, 106, 112)), 0.9);
        drawingContext.DrawGeometry(fill, border, geometry);

        if (_pdfPreview is not null)
        {
            drawingContext.PushClip(geometry);
            var previewRect = PdfPreviewRect(bounds);
            drawingContext.DrawImage(_pdfPreview, previewRect);
            var veil = new LinearGradientBrush
            {
                StartPoint = new WPoint(0.0, 0.0),
                EndPoint = new WPoint(0.0, 1.0),
                Opacity = 0.18
            };
            veil.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 255, 255, 255), 0.0));
            veil.GradientStops.Add(new GradientStop(WColor.FromArgb(90, 255, 255, 255), 1.0));
            drawingContext.DrawRectangle(veil, null, previewRect);
            drawingContext.Pop();
        }

        var gripOpacity = Math.Clamp(_session.PickProgress * 0.28, 0.0, 0.28);
        if (gripOpacity > 0.02)
        {
            var grip = new LinearGradientBrush(WColor.FromArgb(82, 26, 32, 34), WColor.FromArgb(0, 26, 32, 34), new WPoint(0, 0.5), new WPoint(1, 0.5))
            {
                Opacity = gripOpacity
            };
            drawingContext.PushClip(geometry);
            drawingContext.DrawRectangle(grip, null, new WRect(bounds.X, bounds.Y, bounds.Width * 0.46, bounds.Height));
            drawingContext.Pop();
        }

        if (_session.Absorption < 0.72)
        {
            DrawPaperLabel(drawingContext, bounds);
        }
    }

    private void DrawPaperShadow(DrawingContext drawingContext, WPoint[] corners, WRect bounds)
    {
        var lift = _session.IsHolding ? Math.Max(20.0, _session.ShadowLift) : 4.0;
        var shadowCenter = new WPoint(
            _thingCenter.X + _session.ShadowOffsetX,
            _thingCenter.Y + (bounds.Height * 0.54) + lift);
        var target = LensThroatPoint();
        var suction = SmoothStep(Math.Clamp((_session.Approach - 0.46) / 0.54, 0.0, 1.0));
        if (_session.Absorption > 0.01)
        {
            suction = Math.Max(suction, _session.Absorption);
        }

        shadowCenter = Interpolate(shadowCenter, target, suction * 0.72);
        var width = bounds.Width * (0.72 - (suction * 0.34));
        var height = bounds.Height * (0.20 - (suction * 0.10));
        var alpha = (byte)Math.Clamp((_session.IsHolding ? 76 : 22) * (1.0 - (suction * 0.52)), 0, 76);
        var brush = new RadialGradientBrush(WColor.FromArgb(alpha, 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.70,
            RadiusY = 0.42
        };
        drawingContext.DrawEllipse(brush, null, shadowCenter, width, height);

        var contactAlpha = (byte)Math.Clamp(alpha * 0.58, 0, 44);
        var contact = new RadialGradientBrush(WColor.FromArgb(contactAlpha, 0, 0, 0), WColor.FromArgb(0, 0, 0, 0))
        {
            RadiusX = 0.62,
            RadiusY = 0.34
        };
        var nearCorner = corners.OrderBy(point => Distance(point, target)).First();
        drawingContext.DrawEllipse(contact, null, Interpolate(shadowCenter, nearCorner, 0.20), width * 0.52, height * 0.56);
    }

    private void DrawPaperLabel(DrawingContext drawingContext, WRect bounds)
    {
        var opacity = Math.Clamp(1.0 - (_session.Absorption * 1.45), 0.0, 1.0);
        if (opacity <= 0.02)
        {
            return;
        }

        var text = new FormattedText(
            string.IsNullOrWhiteSpace(_sourceFileName) ? "PDF" : _sourceFileName,
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI Semibold"),
            13,
            new SolidColorBrush(WColor.FromArgb(232, 28, 36, 42)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(32, bounds.Width - 28),
            MaxLineCount = 1,
            Trimming = TextTrimming.CharacterEllipsis
        };

        drawingContext.PushOpacity(opacity);
        var y = _pdfPreview is null
            ? bounds.Y + (bounds.Height / 2) - (text.Height / 2)
            : bounds.Bottom - text.Height - 8;
        drawingContext.DrawText(text, new WPoint(bounds.X + 10, y));
        drawingContext.Pop();
    }

    private WRect PdfPreviewRect(WRect bounds)
    {
        var margin = Math.Max(7.0, bounds.Width * 0.055);
        var labelHeight = Math.Min(34.0, bounds.Height * 0.18);
        return new WRect(
            bounds.X + margin,
            bounds.Y + margin,
            Math.Max(4, bounds.Width - (margin * 2)),
            Math.Max(4, bounds.Height - (margin * 2) - labelHeight));
    }

    private void SignalPlacementReady()
    {
        if (string.IsNullOrWhiteSpace(_placementSignalPath))
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(_placementSignalPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(_placementSignalPath, new[]
            {
                "{",
                $"  \"placedAtUtc\": \"{DateTimeOffset.UtcNow:O}\",",
                $"  \"sourcePdfName\": \"{EscapeJson(_sourceFileName)}\",",
                $"  \"sourcePdfOwner\": \"Windows\",",
                $"  \"sourcePdfPathLocalOnly\": \"{EscapeJson(_sourcePdfPath)}\"",
                "}"
            });
        }
        catch
        {
            // The overlay must never crash while the owner is carrying a document.
        }
    }

    private void DrawMicroStatus(DrawingContext drawingContext)
    {
        if (_session.State is not NativeGlassOverlayCarryState.InTransit || _session.TransitRemainingMilliseconds <= 0)
        {
            return;
        }

        var text = new FormattedText(
            $"{Math.Ceiling(_session.TransitRemainingMilliseconds / 1000.0):0}s",
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            10,
            new SolidColorBrush(WColor.FromArgb(118, 255, 255, 255)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        drawingContext.DrawText(text, LensThroatPoint() + new Vector(-text.Width / 2, 28));
    }

    private WPoint[] PaperCorners(WRect bounds)
    {
        var w = bounds.Width;
        var h = bounds.Height;
        var center = new WPoint(bounds.X + (w / 2), bounds.Y + (h / 2));
        var raw = new[]
        {
            new Vector(-w / 2, -h / 2),
            new Vector(w / 2, -h / 2),
            new Vector(w / 2, h / 2),
            new Vector(-w / 2, h / 2)
        };

        var tiltX = _session.TiltX * 0.012;
        var tiltY = _session.TiltY * 0.012;
        var points = raw
            .Select(vector =>
            {
                var perspectiveX = vector.X + (vector.Y * tiltX);
                var perspectiveY = vector.Y + (vector.X * tiltY);
                return new WPoint(center.X + perspectiveX, center.Y + perspectiveY);
            })
            .ToArray();

        var throat = LensThroatPoint();
        var approach = SmoothStep(Math.Clamp((_session.Approach - 0.20) / 0.80, 0.0, 1.0));
        var collapse = Math.Max(approach * 0.72, _session.Absorption * 0.98);
        if (collapse <= 0.001)
        {
            return points;
        }

        var distances = points.Select(point => Distance(point, throat)).ToArray();
        var min = distances.Min();
        var max = distances.Max();
        var range = Math.Max(1.0, max - min);

        for (var index = 0; index < points.Length; index++)
        {
            var nearWeight = 1.0 - ((distances[index] - min) / range);
            var localPull = Math.Clamp((collapse * 0.22) + (nearWeight * collapse * 0.78), 0.0, 0.985);
            points[index] = Interpolate(points[index], throat, localPull);
        }

        return points;
    }

    private WRect ThingBounds()
    {
        var scale = 1.0;
        if (_session.IsHolding || _session.PickProgress > 0.001f)
        {
            scale -= 0.28 * EaseOut(_session.PickProgress);
        }

        if (_session.PullOutRecovery < 1f)
        {
            scale = 0.52 + (0.20 * EaseOut(_session.PullOutRecovery));
        }

        if (_session.Absorption > 0.01f)
        {
            scale *= 1.0 - (0.86 * EaseOut(_session.Absorption));
        }

        scale = Math.Clamp(scale, 0.08, 1.0);
        var width = (_pdfPreview is null ? 188.0 : 176.0) * scale;
        var height = (_pdfPreview is null ? 96.0 : 232.0) * scale;
        return new WRect(_thingCenter.X - (width / 2), _thingCenter.Y - (height / 2), width, height);
    }

    private WRect LensBounds()
    {
        var emergence = EaseOut(_session.LensEmergence);
        var approach = EaseOut(_session.Approach);
        var width = 270.0 * (0.84 + (emergence * 0.16) + (approach * 0.06));
        var height = 205.0 * (0.84 + (emergence * 0.16) + (approach * 0.04));
        return new WRect(_lensCenter.X - (width / 2), _lensCenter.Y - (height / 2), width, height);
    }

    private WRect PortalEdgeBounds()
    {
        var width = 210.0;
        var height = Math.Max(520.0, ActualHeight * 0.98);
        return new WRect(ActualWidth - width + 2, (ActualHeight - height) / 2, width, height);
    }

    private bool IsOverPortalEdge(WPoint point, WRect thingBounds)
    {
        var activationX = Math.Max(0, ActualWidth - 320);
        var hit = new WRect(activationX, 0, Math.Max(320, ActualWidth - activationX), ActualHeight);
        return hit.Contains(point) || hit.IntersectsWith(thingBounds);
    }

    private static ImageSource? TryLoadPdfPreview(string pdfPath)
    {
        try
        {
            if (!File.Exists(pdfPath))
            {
                return null;
            }

            var document = PdfFrameDocument.Load(pdfPath);
            var renderer = PdfFrameRendererFactory.CreateDefault();
            var result = renderer.Render(new PdfFrameRenderRequest(
                document,
                1,
                new PdfFrameRenderOptions(RequestedWidth: 520),
                "ablage-windows-owner",
                document.ThingId));

            if (!result.ContainsPixelPayload || result.PixelData is null || result.FrameFormat != FrameFormat.PngFrame)
            {
                return null;
            }

            using var stream = new MemoryStream(result.PixelData);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private Int32Rect DesktopSampleRect(WRect bounds)
    {
        var overscanX = bounds.Width * 0.30;
        var overscanY = bounds.Height * 0.24;
        var x = (int)Math.Floor(_screenBounds.Left + bounds.X - overscanX);
        var y = (int)Math.Floor(_screenBounds.Top + bounds.Y - overscanY);
        var width = (int)Math.Ceiling(bounds.Width + (overscanX * 2));
        var height = (int)Math.Ceiling(bounds.Height + (overscanY * 2));

        var screenLeft = _screenBounds.Left;
        var screenTop = _screenBounds.Top;
        var screenRight = _screenBounds.Right;
        var screenBottom = _screenBounds.Bottom;
        var clampedX = Math.Clamp(x, screenLeft, screenRight - 4);
        var clampedY = Math.Clamp(y, screenTop, screenBottom - 4);
        var clampedRight = Math.Clamp(x + width, clampedX + 4, screenRight);
        var clampedBottom = Math.Clamp(y + height, clampedY + 4, screenBottom);
        return new Int32Rect(clampedX, clampedY, clampedRight - clampedX, clampedBottom - clampedY);
    }

    private WPoint LensThroatPoint()
    {
        var bounds = LensBounds();
        var outward = LensOutwardDirection();
        return _lensCenter - (outward * (bounds.Width * (0.11 + (_session.LensOpen * 0.04))));
    }

    private Vector LensOutwardDirection()
    {
        var center = new WPoint(ActualWidth / 2, ActualHeight / 2);
        var vector = _lensCenter - center;
        if (vector.Length <= 0.001)
        {
            return new Vector(1, 0);
        }

        vector.Normalize();
        return vector;
    }

    private bool IsThingVisible()
    {
        return _session.State is not NativeGlassOverlayCarryState.PlacedRemote and not NativeGlassOverlayCarryState.Closed &&
            (_session.Absorption < 0.995f || _session.IsHolding);
    }

    private bool IsNearLens(WPoint point, double radius)
    {
        return Distance(point, _lensCenter) <= radius;
    }

    private double LensNearness(WPoint point)
    {
        var distance = Distance(point, _lensCenter);
        return Math.Clamp(1.0 - ((distance - 62.0) / 190.0), 0.0, 1.0);
    }

    private int TakeElapsedMilliseconds()
    {
        var now = _stopwatch.ElapsedMilliseconds;
        var elapsed = Math.Clamp(now - _lastMilliseconds, 1, 34);
        _lastMilliseconds = now;
        return (int)elapsed;
    }

    private static Geometry Polygon(IReadOnlyList<WPoint> points)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(points[0], true, true);
            for (var index = 1; index < points.Count; index++)
            {
                context.LineTo(points[index], true, false);
            }
        }

        geometry.Freeze();
        return geometry;
    }

    private static bool IsValidPoint(WPoint point)
    {
        return !double.IsNaN(point.X) && !double.IsNaN(point.Y) && point.X > 0 && point.Y > 0;
    }

    private static WPoint Interpolate(WPoint start, WPoint end, double amount)
    {
        var t = Math.Clamp(amount, 0.0, 1.0);
        return new WPoint(start.X + ((end.X - start.X) * t), start.Y + ((end.Y - start.Y) * t));
    }

    private static double Distance(WPoint left, WPoint right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
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

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
