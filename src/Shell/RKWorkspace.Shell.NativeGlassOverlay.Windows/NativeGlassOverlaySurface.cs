using RKWorkspace.Shell;
using RKWorkspace.Frame.Pdf;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
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
    private readonly double _surfaceWidth;
    private readonly double _surfaceHeight;
    private readonly string _placementSignalPath;
    private readonly NativeGlassOverlayOptions _options;
    private readonly NativeGlassOverlayDiagnostics _diagnostics;
    private readonly WRect _portalScreenBounds;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly NativeGlassOverlaySession _session = new();
    private string? _sourcePdfPath;
    private string _sourceFileName = string.Empty;
    private ImageSource? _pdfPreview;
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
    private bool _gestureCarryMode;
    private int _placementSequence;
    private long _lastMilliseconds;
    private double _phase;
    private NearestAblageResult _nearestAblage;

    public NativeGlassOverlaySurface(
        WorkspaceShellRuntime runtime,
        System.Drawing.Rectangle screenBounds,
        double surfaceWidth,
        double surfaceHeight,
        NativeGlassOverlayOptions options)
    {
        _runtime = runtime;
        _screenBounds = screenBounds;
        _surfaceWidth = Math.Max(1.0, surfaceWidth);
        _surfaceHeight = Math.Max(1.0, surfaceHeight);
        _options = options;
        _placementSignalPath = options.PlacementSignalPath;
        _diagnostics = new NativeGlassOverlayDiagnostics(options.DiagnosticsPath);
        _nearestAblage = ResolveNearestAblage();
        _diagnostics.Set("Gegenseite", DescribeNearestAblage());
        _diagnostics.Set("Glaskante", $"bereit (aus): {PortalEdgeDirection()} -> {_nearestAblage.TargetDisplayName}");
        if (!string.IsNullOrWhiteSpace(options.SourcePdfPath))
        {
            LoadSourcePdf(options.SourcePdfPath, deferPreview: options.PickImmediately);
        }

        _portalScreenBounds = new WRect(0, 0, _surfaceWidth, _surfaceHeight);
        Focusable = true;
        Cursor = WCursors.Arrow;
        Width = _surfaceWidth;
        Height = _surfaceHeight;
        SnapsToDevicePixels = false;
        UseLayoutRounding = false;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

        _thingCenter = new WPoint(_surfaceWidth * 0.38, _surfaceHeight * 0.50);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _thingCenter;
        _lensCenter = new WPoint(_portalScreenBounds.Right - 78, _portalScreenBounds.Y + (_portalScreenBounds.Height * 0.52));
    }

    public NativeGlassOverlaySession Session => _session;

    public NativeGlassOverlayDiagnostics Diagnostics => _diagnostics;

    public bool WantsPointerInput => !_options.RealPdfGestureMode || IsThingVisible() || _isHolding;

    public event EventHandler? PointerInputModeChanged;

    public void MarkCaptureExclusion(bool ready)
    {
        _captureExclusionReady = ready;
    }

    public void ActivatePickAt(WPoint point)
    {
        if (_sourcePdfPath is null)
        {
            _diagnostics.Set("Geste", "keine PDF geladen");
            return;
        }

        if (!IsValidPoint(point))
        {
            point = _thingCenter;
        }

        _thingCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
        _velocity = default;
        _grabOffset = default;
        SetHolding(true);
        _gestureCarryMode = false;
        CaptureMouse();
        Cursor = WCursors.SizeAll;
        _session.Pick();
        _diagnostics.Set("Carry", $"genommen: {_sourceFileName}");
        InvalidateVisual();
    }

    public void PickSelectedPdfFromGesture(string gestureName = "Hotkey")
    {
        _diagnostics.Set("Geste", $"Pick-Geste empfangen ({gestureName})");
        var selectedPdf = NativeSelectedPdfResolver.TryResolveSelectedPdf(Dispatcher, _diagnostics);
        if (string.IsNullOrWhiteSpace(selectedPdf) || !File.Exists(selectedPdf))
        {
            _diagnostics.Set("Geste", "PDF nicht erkannt - PDF markieren und Strg+Alt+Leertaste oder F9 druecken");
            InvalidateVisual();
            return;
        }

        LoadSourcePdf(selectedPdf, deferPreview: true);
        PickCurrentPdfAtCursor("PDF genommen");
    }

    public void PickLoadedPdfFromContext()
    {
        if (string.IsNullOrWhiteSpace(_sourcePdfPath) || !File.Exists(_sourcePdfPath))
        {
            _diagnostics.Set("Geste", "Kontextaufruf ohne gueltige PDF");
            InvalidateVisual();
            return;
        }

        PickCurrentPdfAtCursor("Windows-Kontext");
    }

    public NativePdfContextPickResult PickPdfFromContextPath(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            _diagnostics.Set("Kontext", "kein PDF-Pfad empfangen");
            InvalidateVisual();
            return NativePdfContextPickResult.Failed("Kein PDF-Pfad empfangen.");
        }

        var resolvedPath = Path.GetFullPath(pdfPath);
        if (!File.Exists(resolvedPath))
        {
            _diagnostics.Set("Kontext", "PDF existiert nicht");
            InvalidateVisual();
            return NativePdfContextPickResult.Failed("PDF existiert nicht.");
        }

        if (!resolvedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            _diagnostics.Set("Kontext", "Datei ist keine PDF");
            InvalidateVisual();
            return NativePdfContextPickResult.Failed("Datei ist keine PDF.");
        }

        _diagnostics.Set("Kontext", "PDF-Pfad empfangen");
        LoadSourcePdf(resolvedPath, deferPreview: true);
        PickCurrentPdfAtCursor("Windows-Kontext");
        return NativePdfContextPickResult.Ok($"PDF genommen: {_sourceFileName}");
    }

    private void PickCurrentPdfAtCursor(string gestureName)
    {
        if (string.IsNullOrWhiteSpace(_sourcePdfPath))
        {
            _diagnostics.Set("Geste", "keine PDF geladen");
            InvalidateVisual();
            return;
        }

        var screen = System.Windows.Forms.Cursor.Position;
        var point = PointFromScreen(new WPoint(screen.X, screen.Y));
        if (!IsValidPoint(point))
        {
            point = new WPoint(SurfaceWidth() * 0.46, SurfaceHeight() * 0.52);
        }

        _thingCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
        _velocity = default;
        _grabOffset = default;
        _gestureCarryMode = true;
        SetHolding(true);
        Cursor = WCursors.SizeAll;
        _session.Pick();
        _pendingRemotePlacement = false;
        _placementSignalWritten = false;
        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
        _diagnostics.Set("Geste", gestureName);
        _diagnostics.Set("Carry", $"in virtueller Hand: {_sourceFileName}");
        _diagnostics.Set("Glaskante", $"sichtbar: {PortalEdgeDirection()} -> {_nearestAblage.TargetDisplayName}");
        PointerInputModeChanged?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }

    public void Tick()
    {
        UpdatePortalGeometry();
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
        return default;
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
        var point = e.GetPosition(this);
        if (_isHolding && _gestureCarryMode && e.ChangedButton == MouseButton.Left)
        {
            CompletePlacementOrReturn(point);
            e.Handled = true;
            return;
        }

        if (e.ChangedButton == MouseButton.Left && ThingBounds().Contains(point) && IsThingVisible())
        {
            SetHolding(true);
            _gestureCarryMode = false;
            _grabOffset = point - _thingCenter;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.Pick();
            _pendingRemotePlacement = false;
            _diagnostics.Set("Carry", $"genommen: {_sourceFileName}");
            _diagnostics.Set("Glaskante", $"sichtbar: {PortalEdgeDirection()} -> {_nearestAblage.TargetDisplayName}");
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
            SetHolding(true);
            _gestureCarryMode = false;
            CaptureMouse();
            Cursor = WCursors.SizeAll;
            _session.PullOut();
            _pendingRemotePlacement = false;
            _diagnostics.Set("Glaskante", $"sichtbar: {PortalEdgeDirection()} -> {_nearestAblage.TargetDisplayName}");
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
            _diagnostics.Set("Glaskante", $"aktiv: {_nearestAblage.EdgeHint} -> {_nearestAblage.TargetDisplayName}");
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
        _diagnostics.Set("Carry", $"wird getragen: {_sourceFileName}");
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (!_isHolding || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        SetHolding(false);
        ReleaseMouseCapture();
        Cursor = WCursors.Arrow;
        CompletePlacementOrReturn(_targetCenter);

        e.Handled = true;
    }

    private void CompletePlacementOrReturn(WPoint point)
    {
        _gestureCarryMode = false;
        SetHolding(false);
        ReleaseMouseCapture();
        Cursor = WCursors.Arrow;

        if (IsOverPortalEdge(point, ThingBounds()) || IsNearLens(point, 228))
        {
            _session.ApproachLens(1f);
            _session.PlaceIntoLens();
            _targetCenter = Interpolate(_targetCenter, _lensCenter, 0.40);
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.NearSurface, "HX-002");
            _diagnostics.Set("Drop", $"auf Glaskante abgelegt -> {_nearestAblage.TargetDisplayName}");
            if (_options.InstantPlacementSignal)
            {
                SignalPlacementReady();
                _placementSignalWritten = true;
                _pendingRemotePlacement = false;
                _diagnostics.Set("Signal", "sofort geschrieben");
            }
            else
            {
                _pendingRemotePlacement = true;
                _diagnostics.Set("Signal", "wartet 10s Retake-Fenster");
            }
        }
        else
        {
            _session.PlaceOnDesktop();
            _pendingRemotePlacement = false;
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
            _diagnostics.Set("Drop", "zurück auf Windows-Ablage");
            _diagnostics.Set("Signal", "nicht geschrieben");
            _diagnostics.Set("Glaskante", $"aus (kein Transfer): {PortalEdgeDirection()} -> {_nearestAblage.TargetDisplayName}");
        }

        PointerInputModeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetHolding(bool value)
    {
        if (_isHolding == value)
        {
            return;
        }

        _isHolding = value;
        PointerInputModeChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        UpdatePortalGeometry();
        DrawThing(drawingContext);
        DrawPortalDirectionGuide(drawingContext);
        DrawGlassPortalEdge(drawingContext);
        DrawMicroStatus(drawingContext);
        DrawDiagnostics(drawingContext);
    }

    private void DrawGlassPortalEdge(DrawingContext drawingContext)
    {
        if (!ShouldShowGlassPortalEdge())
        {
            return;
        }

        var edge = PortalEdgeBounds();
        var direction = PortalEdgeDirection();
        var active = Math.Clamp((_session.PickProgress * 0.42) + (_session.Approach * 0.46) + (_session.LensOpen * 0.20), 0.0, 1.0);
        var closingFade = _session.State == NativeGlassOverlayCarryState.Closing
            ? Math.Clamp(_session.LensEmergence, 0.0, 1.0)
            : 1.0;
        var opacity = Math.Clamp((0.68 + (active * 0.16)) * closingFade, 0.0, 0.88);

        drawingContext.PushOpacity(opacity);
        drawingContext.PushOpacityMask(ProgressiveEdgeMask(direction));

        var desktopMaterial = EdgeAxisGradient(direction,
            WColor.FromArgb(0, 255, 255, 255),
            WColor.FromArgb(4, 255, 255, 255),
            WColor.FromArgb(18, 230, 250, 255),
            WColor.FromArgb(32, 255, 255, 255));
        drawingContext.DrawRectangle(desktopMaterial, null, edge);

        var body = EdgeAxisGradient(direction,
            WColor.FromArgb(0, 255, 255, 255),
            WColor.FromArgb(8, 255, 255, 255),
            WColor.FromArgb(24, 174, 232, 244),
            WColor.FromArgb(42, 255, 255, 255));
        drawingContext.DrawRectangle(body, null, edge);

        var glow = CrossAxisGradient(direction,
            WColor.FromArgb(0, 190, 245, 255),
            WColor.FromArgb(30, 255, 255, 255),
            WColor.FromArgb(26, 142, 236, 245),
            WColor.FromArgb(30, 255, 255, 255),
            WColor.FromArgb(0, 190, 245, 255));
        glow.Opacity = 0.52;
        drawingContext.DrawRectangle(glow, null, edge);

        drawingContext.Pop();

        DrawGlassThroat(drawingContext, edge, direction, active);
        DrawPhysicalGlassEdge(drawingContext, edge, direction, active);
        DrawInnerGlassCatchlight(drawingContext, edge, direction);

        drawingContext.Pop();
    }

    private bool ShouldShowGlassPortalEdge()
    {
        if (string.IsNullOrWhiteSpace(_sourcePdfPath))
        {
            return false;
        }

        return _isHolding ||
            _pendingRemotePlacement ||
            _session.State is NativeGlassOverlayCarryState.Held or
                NativeGlassOverlayCarryState.InTransit or
                NativeGlassOverlayCarryState.Closing ||
            _session.PickProgress > 0.03f ||
            _session.Approach > 0.03f ||
            _session.LensOpen > 0.03f;
    }

    private void DrawGlassThroat(DrawingContext drawingContext, WRect edge, AblageDirection direction, double active)
    {
        var fill = CrossAxisGradient(direction,
            WColor.FromArgb(0, 255, 255, 255),
            WColor.FromArgb((byte)Math.Clamp(72 + (active * 16), 72, 88), 255, 255, 255),
            WColor.FromArgb((byte)Math.Clamp(54 + (active * 14), 54, 68), 136, 235, 245),
            WColor.FromArgb(0, 255, 255, 255));
        var halo = CrossAxisGradient(direction,
            WColor.FromArgb(0, 145, 235, 246),
            WColor.FromArgb(24, 150, 236, 246),
            WColor.FromArgb(18, 255, 255, 255),
            WColor.FromArgb(0, 145, 235, 246));

        if (direction is AblageDirection.Left or AblageDirection.Right)
        {
            var height = Math.Min(edge.Height * 0.38, 320.0);
            var centerX = direction == AblageDirection.Right ? edge.Right - 18.0 : edge.Left + 18.0;
            var y = edge.Y + ((edge.Height - height) / 2.0);
            drawingContext.DrawRoundedRectangle(halo, null, new WRect(centerX - 23.0, y - 16.0, 46.0, height + 32.0), 23.0, 23.0);
            drawingContext.DrawRoundedRectangle(fill, null, new WRect(centerX - 6.5, y, 13.0, height), 6.5, 6.5);
            return;
        }

        var width = Math.Min(edge.Width * 0.38, 360.0);
        var centerY = direction == AblageDirection.Down ? edge.Bottom - 18.0 : edge.Top + 18.0;
        var x = edge.X + ((edge.Width - width) / 2.0);
        drawingContext.DrawRoundedRectangle(halo, null, new WRect(x - 16.0, centerY - 23.0, width + 32.0, 46.0), 23.0, 23.0);
        drawingContext.DrawRoundedRectangle(fill, null, new WRect(x, centerY - 6.5, width, 13.0), 6.5, 6.5);
    }

    private void DrawPhysicalGlassEdge(DrawingContext drawingContext, WRect edge, AblageDirection direction, double active)
    {
        var fill = CrossAxisGradient(direction,
            WColor.FromArgb(0, 180, 245, 255),
            WColor.FromArgb((byte)Math.Clamp(62 + (active * 18), 62, 80), 255, 255, 255),
            WColor.FromArgb((byte)Math.Clamp(42 + (active * 16), 42, 58), 126, 228, 240),
            WColor.FromArgb((byte)Math.Clamp(62 + (active * 18), 62, 80), 255, 255, 255),
            WColor.FromArgb(0, 180, 245, 255));

        switch (direction)
        {
            case AblageDirection.Left:
                drawingContext.DrawRectangle(fill, null, new WRect(edge.Left, edge.Top, 2.0, edge.Height));
                break;
            case AblageDirection.Up:
                drawingContext.DrawRectangle(fill, null, new WRect(edge.Left, edge.Top, edge.Width, 2.0));
                break;
            case AblageDirection.Down:
                drawingContext.DrawRectangle(fill, null, new WRect(edge.Left, edge.Bottom - 2.0, edge.Width, 2.0));
                break;
            default:
                drawingContext.DrawRectangle(fill, null, new WRect(edge.Right - 2.0, edge.Top, 2.0, edge.Height));
                break;
        }
    }

    private void DrawInnerGlassCatchlight(DrawingContext drawingContext, WRect edge, AblageDirection direction)
    {
        var fill = CrossAxisGradient(direction,
            WColor.FromArgb(0, 255, 255, 255),
            WColor.FromArgb(48, 255, 255, 255),
            WColor.FromArgb(24, 142, 230, 242),
            WColor.FromArgb(0, 255, 255, 255));
        fill.Opacity = 0.52;

        if (direction is AblageDirection.Left or AblageDirection.Right)
        {
            var x = direction == AblageDirection.Right
                ? edge.Left + (edge.Width * 0.82)
                : edge.Left + (edge.Width * 0.18);
            drawingContext.DrawRoundedRectangle(fill, null, new WRect(x - 1.6, edge.Y + (edge.Height * 0.14), 3.2, edge.Height * 0.72), 1.6, 1.6);
            return;
        }

        var y = direction == AblageDirection.Down
            ? edge.Top + (edge.Height * 0.82)
            : edge.Top + (edge.Height * 0.18);
        drawingContext.DrawRoundedRectangle(fill, null, new WRect(edge.X + (edge.Width * 0.14), y - 1.6, edge.Width * 0.72, 3.2), 1.6, 1.6);
    }

    private static LinearGradientBrush ProgressiveEdgeMask(AblageDirection direction)
    {
        var brush = new LinearGradientBrush();
        switch (direction)
        {
            case AblageDirection.Left:
                brush.StartPoint = new WPoint(0.0, 0.5);
                brush.EndPoint = new WPoint(1.0, 0.5);
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(255, 0, 0, 0), 0.00));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(174, 0, 0, 0), 0.42));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(20, 0, 0, 0), 0.82));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 0, 0, 0), 1.00));
                break;
            case AblageDirection.Up:
                brush.StartPoint = new WPoint(0.5, 0.0);
                brush.EndPoint = new WPoint(0.5, 1.0);
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(255, 0, 0, 0), 0.00));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(174, 0, 0, 0), 0.42));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(20, 0, 0, 0), 0.82));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 0, 0, 0), 1.00));
                break;
            case AblageDirection.Down:
                brush.StartPoint = new WPoint(0.5, 0.0);
                brush.EndPoint = new WPoint(0.5, 1.0);
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 0, 0, 0), 0.00));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(20, 0, 0, 0), 0.18));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(174, 0, 0, 0), 0.58));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(255, 0, 0, 0), 1.00));
                break;
            default:
                brush.StartPoint = new WPoint(0.0, 0.5);
                brush.EndPoint = new WPoint(1.0, 0.5);
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(0, 0, 0, 0), 0.00));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(20, 0, 0, 0), 0.18));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(174, 0, 0, 0), 0.58));
                brush.GradientStops.Add(new GradientStop(WColor.FromArgb(255, 0, 0, 0), 1.00));
                break;
        }

        return brush;
    }

    private static LinearGradientBrush EdgeAxisGradient(AblageDirection direction, WColor far, WColor soft, WColor glass, WColor edge)
    {
        var brush = new LinearGradientBrush();
        switch (direction)
        {
            case AblageDirection.Left:
                brush.StartPoint = new WPoint(0.0, 0.5);
                brush.EndPoint = new WPoint(1.0, 0.5);
                brush.GradientStops.Add(new GradientStop(edge, 0.00));
                brush.GradientStops.Add(new GradientStop(glass, 0.42));
                brush.GradientStops.Add(new GradientStop(soft, 0.82));
                brush.GradientStops.Add(new GradientStop(far, 1.00));
                break;
            case AblageDirection.Up:
                brush.StartPoint = new WPoint(0.5, 0.0);
                brush.EndPoint = new WPoint(0.5, 1.0);
                brush.GradientStops.Add(new GradientStop(edge, 0.00));
                brush.GradientStops.Add(new GradientStop(glass, 0.42));
                brush.GradientStops.Add(new GradientStop(soft, 0.82));
                brush.GradientStops.Add(new GradientStop(far, 1.00));
                break;
            case AblageDirection.Down:
                brush.StartPoint = new WPoint(0.5, 0.0);
                brush.EndPoint = new WPoint(0.5, 1.0);
                brush.GradientStops.Add(new GradientStop(far, 0.00));
                brush.GradientStops.Add(new GradientStop(soft, 0.18));
                brush.GradientStops.Add(new GradientStop(glass, 0.58));
                brush.GradientStops.Add(new GradientStop(edge, 1.00));
                break;
            default:
                brush.StartPoint = new WPoint(0.0, 0.5);
                brush.EndPoint = new WPoint(1.0, 0.5);
                brush.GradientStops.Add(new GradientStop(far, 0.00));
                brush.GradientStops.Add(new GradientStop(soft, 0.18));
                brush.GradientStops.Add(new GradientStop(glass, 0.58));
                brush.GradientStops.Add(new GradientStop(edge, 1.00));
                break;
        }

        return brush;
    }

    private static LinearGradientBrush CrossAxisGradient(AblageDirection direction, params WColor[] colors)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = direction is AblageDirection.Left or AblageDirection.Right
                ? new WPoint(0.5, 0.0)
                : new WPoint(0.0, 0.5),
            EndPoint = direction is AblageDirection.Left or AblageDirection.Right
                ? new WPoint(0.5, 1.0)
                : new WPoint(1.0, 0.5)
        };

        for (var index = 0; index < colors.Length; index++)
        {
            var location = colors.Length == 1 ? 1.0 : (double)index / (colors.Length - 1);
            brush.GradientStops.Add(new GradientStop(colors[index], location));
        }

        return brush;
    }

    private void DrawPortalLabel(DrawingContext drawingContext, WRect edge, double active)
    {
        var text = new FormattedText(
            string.IsNullOrWhiteSpace(_nearestAblage.TargetDisplayName) ? _options.TargetDisplayName : _nearestAblage.TargetDisplayName,
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
        var start = PortalGuideStartPoint();
        var end = _lensCenter;
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
        if (string.IsNullOrWhiteSpace(_placementSignalPath) || string.IsNullOrWhiteSpace(_sourcePdfPath))
        {
            return;
        }

        try
        {
            var placementId = $"placement-windows-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Interlocked.Increment(ref _placementSequence):000}";
            var directory = Path.GetDirectoryName(_placementSignalPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(_placementSignalPath, new[]
            {
                "{",
                $"  \"signalSchema\": \"rkws-placement-v2\",",
                $"  \"placementId\": \"{EscapeJson(placementId)}\",",
                $"  \"placedAtUtc\": \"{DateTimeOffset.UtcNow:O}\",",
                $"  \"sourcePdfName\": \"{EscapeJson(_sourceFileName)}\",",
                $"  \"sourcePdfOwner\": \"Windows\",",
                $"  \"sourcePdfPathLocalOnly\": \"{EscapeJson(_sourcePdfPath)}\",",
                $"  \"targetAblageName\": \"{EscapeJson(_nearestAblage.TargetDisplayName)}\",",
                $"  \"targetEdge\": \"{EscapeJson(_nearestAblage.EdgeHint.ToString())}\",",
                $"  \"targetDistanceMeters\": \"{_nearestAblage.DistanceMeters:0.00}\",",
                $"  \"targetDistanceSource\": \"{EscapeJson(_nearestAblage.Source.ToString())}\",",
                "  \"requestedFrameFormat\": \"TransientPdfBytes\",",
                "  \"transientPdfFrame\": \"true\",",
                "  \"supportsTransientPdfBytes\": \"true\",",
                "  \"pdfLeaseMode\": \"MemoryOnly\",",
                "  \"ownerKeepsOriginal\": \"true\",",
                "  \"guestMayPersistPdf\": \"false\",",
                "  \"guestMayExportPdf\": \"false\",",
                "  \"allowTextSelection\": \"true\",",
                "  \"committed\": \"true\"",
                "}"
            });
            _diagnostics.Set("Placement", $"committed: {placementId} | {_sourceFileName}");
            _diagnostics.Set("Signal", $"geschrieben: {_placementSignalPath}");
            _diagnostics.Set("macOS", "FrameGuest darf Frame jetzt holen");
        }
        catch (Exception ex)
        {
            _diagnostics.Set("Signal", $"Fehler: {ex.Message}");
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

    private void DrawDiagnostics(DrawingContext drawingContext)
    {
        var lines = _diagnostics.Steps
            .Select(pair => $"{pair.Key}: {pair.Value}")
            .Take(11)
            .ToArray();
        var text = new FormattedText(
            string.Join(Environment.NewLine, lines),
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            11,
            new SolidColorBrush(WColor.FromArgb(220, 238, 246, 250)),
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = 460,
            Trimming = TextTrimming.CharacterEllipsis
        };

        var panel = new WRect(18, Math.Max(18, SurfaceHeight() - text.Height - 38), 490, text.Height + 22);
        var background = new SolidColorBrush(WColor.FromArgb(132, 14, 18, 20));
        drawingContext.DrawRoundedRectangle(background, null, panel, 8, 8);
        drawingContext.DrawRoundedRectangle(
            null,
            new WPen(new SolidColorBrush(WColor.FromArgb(80, 240, 250, 255)), 0.8),
            panel,
            8,
            8);
        drawingContext.DrawText(text, new WPoint(panel.X + 12, panel.Y + 10));
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
        var surfaceWidth = Math.Max(1.0, SurfaceWidth());
        var surfaceHeight = Math.Max(1.0, SurfaceHeight());
        var depth = Math.Clamp(Math.Min(surfaceWidth, surfaceHeight) * 0.24, 176.0, 224.0);

        return PortalEdgeDirection() switch
        {
            AblageDirection.Left => new WRect(0, 0, Math.Min(depth, surfaceWidth), surfaceHeight),
            AblageDirection.Up => new WRect(0, 0, surfaceWidth, Math.Min(depth, surfaceHeight)),
            AblageDirection.Down => new WRect(0, Math.Max(0, surfaceHeight - depth), surfaceWidth, Math.Min(depth, surfaceHeight)),
            _ => new WRect(Math.Max(0, surfaceWidth - depth), 0, Math.Min(depth, surfaceWidth), surfaceHeight)
        };
    }

    private void UpdatePortalGeometry()
    {
        var edge = PortalEdgeBounds();
        _lensCenter = PortalEdgeDirection() switch
        {
            AblageDirection.Left => new WPoint(edge.Left + 18.0, edge.Y + (edge.Height * 0.50)),
            AblageDirection.Up => new WPoint(edge.X + (edge.Width * 0.50), edge.Top + 18.0),
            AblageDirection.Down => new WPoint(edge.X + (edge.Width * 0.50), edge.Bottom - 18.0),
            _ => new WPoint(edge.Right - 18.0, edge.Y + (edge.Height * 0.50))
        };
    }

    private bool IsOverPortalEdge(WPoint point, WRect thingBounds)
    {
        var edge = PortalEdgeBounds();
        var surfaceWidth = Math.Max(1.0, SurfaceWidth());
        var surfaceHeight = Math.Max(1.0, SurfaceHeight());
        var hit = PortalEdgeDirection() switch
        {
            AblageDirection.Left => new WRect(0, 0, Math.Min(surfaceWidth, edge.Right + 230.0), surfaceHeight),
            AblageDirection.Up => new WRect(0, 0, surfaceWidth, Math.Min(surfaceHeight, edge.Bottom + 230.0)),
            AblageDirection.Down => new WRect(0, Math.Max(0, edge.Y - 230.0), surfaceWidth, surfaceHeight - Math.Max(0, edge.Y - 230.0)),
            _ => new WRect(Math.Max(0, edge.X - 230.0), 0, surfaceWidth - Math.Max(0, edge.X - 230.0), surfaceHeight)
        };
        return hit.Contains(point) || hit.IntersectsWith(thingBounds);
    }

    private AblageDirection PortalEdgeDirection()
    {
        return _nearestAblage.EdgeHint switch
        {
            AblageDirection.Left or AblageDirection.Right or AblageDirection.Up or AblageDirection.Down => _nearestAblage.EdgeHint,
            AblageDirection.UpLeft or AblageDirection.DownLeft => AblageDirection.Left,
            AblageDirection.UpRight or AblageDirection.DownRight => AblageDirection.Right,
            AblageDirection.Front => AblageDirection.Up,
            AblageDirection.Back => AblageDirection.Down,
            _ => AblageDirection.Right
        };
    }

    private WPoint PortalGuideStartPoint()
    {
        var bounds = ThingBounds();
        return PortalEdgeDirection() switch
        {
            AblageDirection.Left => new WPoint(bounds.Left, bounds.Top + (bounds.Height * 0.50)),
            AblageDirection.Up => new WPoint(bounds.Left + (bounds.Width * 0.50), bounds.Top),
            AblageDirection.Down => new WPoint(bounds.Left + (bounds.Width * 0.50), bounds.Bottom),
            _ => new WPoint(bounds.Right, bounds.Top + (bounds.Height * 0.50))
        };
    }

    private void LoadSourcePdf(string pdfPath, bool deferPreview = false)
    {
        _sourcePdfPath = Path.GetFullPath(pdfPath);
        _sourceFileName = Path.GetFileName(_sourcePdfPath);
        _pdfPreview = null;
        _diagnostics.Set("PDF", _sourceFileName);
        _diagnostics.Set("Gegenseite", DescribeNearestAblage());
        if (deferPreview)
        {
            _diagnostics.Set("PDF", $"{_sourceFileName} | Vorschau im Hintergrund");
            BeginLoadPdfPreview(_sourcePdfPath);
            return;
        }

        _pdfPreview = TryLoadPdfPreview(_sourcePdfPath);
    }

    private void BeginLoadPdfPreview(string pdfPath)
    {
        var expectedPath = Path.GetFullPath(pdfPath);
        Task.Run(() => TryLoadPdfPreview(expectedPath))
            .ContinueWith(task =>
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.BeginInvoke(() => ApplyPreview(expectedPath, task));
                    return;
                }

                ApplyPreview(expectedPath, task);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
    }

    private void ApplyPreview(string expectedPath, Task<ImageSource?> task)
    {
        if (!string.Equals(_sourcePdfPath, expectedPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (task.Status == TaskStatus.RanToCompletion)
        {
            _pdfPreview = task.Result;
            _diagnostics.Set("PDF", _pdfPreview is null
                ? $"{_sourceFileName} | Vorschau nicht verfuegbar"
                : $"{_sourceFileName} | Vorschau bereit");
        }
        else
        {
            _diagnostics.Set("PDF", $"{_sourceFileName} | Vorschaufehler");
        }

        InvalidateVisual();
    }

    private NearestAblageResult ResolveNearestAblage()
    {
        if (_options.TargetDistanceMeters > 0)
        {
            var direction = ParseDirection(_options.TargetDirection);
            var source = ParseSource(_options.TargetDistanceSource);
            var distanceKind = _options.TargetDistanceMeters <= 0.50
                ? AblageDistanceKind.VeryNear
                : AblageDistanceKind.Near;
            return new NearestAblageResult(
                SimulatedAblageProximityProvider.WindowsAblageId,
                new AblageIdentity("ablage-macos"),
                _options.TargetDisplayName,
                AblageSurfacePlatform.MacOS,
                direction,
                direction,
                distanceKind,
                _options.TargetDistanceMeters,
                1.0,
                source,
                true,
                true,
                "Manuell kalibriert fuer Live-Test");
        }

        var selector = new NearestAblageSelector();
        var snapshot = new SimulatedAblageProximityProvider().GetSnapshot(SimulatedAblageProximityProvider.WindowsAblageId);
        var nearest = selector.Select(snapshot);
        if (nearest.HasTarget)
        {
            return nearest;
        }

        return new NearestAblageResult(
            SimulatedAblageProximityProvider.WindowsAblageId,
            new AblageIdentity("ablage-macos"),
            _options.TargetDisplayName,
            AblageSurfacePlatform.MacOS,
            AblageDirection.Right,
            AblageDirection.Right,
            AblageDistanceKind.Near,
            1.2,
            0.9,
            AblageProximitySource.Simulated,
            true,
            true,
            "Fallback: macOS rechts");
    }

    private static AblageDirection ParseDirection(string value)
    {
        return Enum.TryParse<AblageDirection>(value, ignoreCase: true, out var direction)
            ? direction
            : AblageDirection.Right;
    }

    private static AblageProximitySource ParseSource(string value)
    {
        return Enum.TryParse<AblageProximitySource>(value, ignoreCase: true, out var source)
            ? source
            : AblageProximitySource.ManualMap;
    }

    private string DescribeNearestAblage()
    {
        if (!_nearestAblage.HasTarget)
        {
            return "keine Gegenseite erkannt";
        }

        return $"{_nearestAblage.TargetDisplayName} | Seite: {_nearestAblage.EdgeHint} | Abstand: {_nearestAblage.DistanceMeters:0.00}m | Quelle: {_nearestAblage.Source} | Stabil: {(_nearestAblage.IsStable ? "ja" : "nein")}";
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

    private static WRect GetPrimaryScreenBoundsRelativeTo(System.Drawing.Rectangle virtualBounds)
    {
        var primary = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? virtualBounds;
        return new WRect(
            primary.Left - virtualBounds.Left,
            primary.Top - virtualBounds.Top,
            primary.Width,
            primary.Height);
    }

    private double SurfaceWidth()
    {
        if (IsUsableDimension(RenderSize.Width))
        {
            return RenderSize.Width;
        }

        if (IsUsableDimension(ActualWidth))
        {
            return ActualWidth;
        }

        if (IsUsableDimension(Width))
        {
            return Width;
        }

        return _surfaceWidth;
    }

    private double SurfaceHeight()
    {
        if (IsUsableDimension(RenderSize.Height))
        {
            return RenderSize.Height;
        }

        if (IsUsableDimension(ActualHeight))
        {
            return ActualHeight;
        }

        if (IsUsableDimension(Height))
        {
            return Height;
        }

        return _surfaceHeight;
    }

    private static bool IsUsableDimension(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 1.0;
    }

    private Int32Rect DesktopSampleRect(WRect bounds)
    {
        var scaleX = _screenBounds.Width / Math.Max(1.0, SurfaceWidth());
        var scaleY = _screenBounds.Height / Math.Max(1.0, SurfaceHeight());
        var overscanX = bounds.Width * 0.30;
        var overscanY = bounds.Height * 0.24;
        var x = (int)Math.Floor(_screenBounds.Left + ((bounds.X - overscanX) * scaleX));
        var y = (int)Math.Floor(_screenBounds.Top + ((bounds.Y - overscanY) * scaleY));
        var width = (int)Math.Ceiling((bounds.Width + (overscanX * 2)) * scaleX);
        var height = (int)Math.Ceiling((bounds.Height + (overscanY * 2)) * scaleY);

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
        return _sourcePdfPath is not null &&
            _session.State is not NativeGlassOverlayCarryState.PlacedRemote and not NativeGlassOverlayCarryState.Closed &&
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
