using RKWorkspace.Shell;

namespace RKWorkspace.Shell.LivingLens.Windows;

public sealed class LivingLensOverlayWindow : Form
{
    private const int WsExLayered = 0x00080000;
    private const int WsExToolWindow = 0x00000080;

    private readonly WorkspaceShellRuntime _runtime;
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly Rectangle _screenBounds;
    private PointF _thingCenter;
    private PointF _targetCenter;
    private PointF _velocity;
    private PointF _lastTargetCenter;
    private PointF _grabOffset;
    private float _phase;
    private bool _isHolding;
    private bool _renderRequested = true;

    public LivingLensOverlayWindow(WorkspaceShellRuntime runtime)
    {
        _runtime = runtime;
        Session = new LivingLensSession();
        _screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(80, 80, 1280, 720);
        _thingCenter = new PointF(_screenBounds.Width * 0.36f, _screenBounds.Height * 0.50f);
        _targetCenter = _thingCenter;
        _lastTargetCenter = _targetCenter;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = _screenBounds;
        TopMost = true;
        ShowInTaskbar = false;
        KeyPreview = true;
        BackColor = Color.Black;
        TransparencyKey = Color.Empty;
        Cursor = Cursors.Default;
        DoubleBuffered = false;
        Text = string.Empty;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint,
            true);

        _timer.Interval = 16;
        _timer.Tick += (_, _) => Tick();
        Session.Start();
    }

    public LivingLensSession Session { get; }

    public bool IsNativeOverlay => true;

    public bool HasBrowserSurface => false;

    public bool HasWebView => false;

    public bool UsesPerPixelAlphaOverlay => true;

    public bool UsesColorKeyTransparency => TransparencyKey != Color.Empty;

    public bool DesktopVisiblePrepared => UsesPerPixelAlphaOverlay &&
        !UsesColorKeyTransparency &&
        FormBorderStyle == FormBorderStyle.None &&
        !ShowInTaskbar;

    public bool EscExitReady { get; private set; } = true;

    public bool SmokeModelOk { get; private set; }

    public bool SmokeExportOk { get; private set; }

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= WsExLayered | WsExToolWindow;
            return createParams;
        }
    }

    public bool SmokeCheck()
    {
        SmokeModelOk = Session.CheckSmokeModel();
        SmokeExportOk = LivingLensFrameExporter.ExportDefaultFrames();
        return SmokeModelOk &&
            SmokeExportOk &&
            IsNativeOverlay &&
            !HasBrowserSurface &&
            !HasWebView &&
            DesktopVisiblePrepared &&
            EscExitReady;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        RenderFrame();
        _timer.Start();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        base.OnFormClosed(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            EscExitReady = true;
            Close();
            return;
        }

        if (e.Control && e.Alt && e.KeyCode == Keys.Space)
        {
            e.Handled = true;
            ActivatePickAt(PointToClient(Cursor.Position));
            return;
        }

        if (e.KeyCode is >= Keys.D1 and <= Keys.D5)
        {
            e.Handled = true;
            Session.SwitchVariant(e.KeyCode - Keys.D1);
            RequestRender();
            return;
        }

        if (e.KeyCode is >= Keys.NumPad1 and <= Keys.NumPad5)
        {
            e.Handled = true;
            Session.SwitchVariant(e.KeyCode - Keys.NumPad1);
            RequestRender();
            return;
        }

        if (e.KeyCode == Keys.A)
        {
            e.Handled = true;
            Session.PlayAbsorption();
            _targetCenter = LensCenter(Session.ActiveLens);
            RequestRender();
            return;
        }

        if (e.KeyCode == Keys.O)
        {
            e.Handled = true;
            Session.ReplayOpening();
            RequestRender();
            return;
        }

        if (e.KeyCode == Keys.T)
        {
            e.Handled = true;
            Session.CycleTiming();
            RequestRender();
            return;
        }

        if (e.KeyCode is Keys.Add or Keys.Oemplus)
        {
            e.Handled = true;
            Session.IncreaseIntensity();
            RequestRender();
            return;
        }

        if (e.KeyCode is Keys.Subtract or Keys.OemMinus)
        {
            e.Handled = true;
            Session.DecreaseIntensity();
            RequestRender();
            return;
        }

        if (e.KeyCode == Keys.D)
        {
            e.Handled = true;
            Session.ToggleDebug();
            RequestRender();
            return;
        }

        if (e.KeyCode == Keys.R)
        {
            e.Handled = true;
            Session.ResetVisualExperiment();
            _thingCenter = new PointF(_screenBounds.Width * 0.36f, _screenBounds.Height * 0.50f);
            _targetCenter = _thingCenter;
            _lastTargetCenter = _thingCenter;
            _velocity = PointF.Empty;
            _isHolding = false;
            Capture = false;
            Cursor = Cursors.Default;
            RequestRender();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && LivingLensRenderer.EstimateThingBounds(_thingCenter, Session.AbsorptionProgress).Contains(e.Location))
        {
            _isHolding = true;
            _grabOffset = new PointF(e.Location.X - _thingCenter.X, e.Location.Y - _thingCenter.Y);
            Capture = true;
            Cursor = Cursors.SizeAll;
            Session.Pick();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            RequestRender();
            return;
        }

        var pullOutLens = DetectLens(e.Location, 224f);
        if (e.Button == MouseButtons.Left &&
            pullOutLens is not null &&
            Session.AbsorptionProgress >= 1f &&
            Session.TargetGhostVisible)
        {
            _isHolding = true;
            _thingCenter = LensCenter(pullOutLens);
            _targetCenter = new PointF(e.X, e.Y);
            _lastTargetCenter = _targetCenter;
            _grabOffset = PointF.Empty;
            _velocity = PointF.Empty;
            Capture = true;
            Cursor = Cursors.SizeAll;
            Session.PullOutFromLens();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            RequestRender();
            return;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isHolding)
        {
            base.OnMouseMove(e);
            return;
        }

        _targetCenter = new PointF(e.X - _grabOffset.X, e.Y - _grabOffset.Y);
        var movement = new PointF(_targetCenter.X - _lastTargetCenter.X, _targetCenter.Y - _lastTargetCenter.Y);
        _lastTargetCenter = _targetCenter;
        Session.Carry(movement.X, movement.Y);

        var lens = DetectLens(_targetCenter, 176f);
        if (lens is not null)
        {
            Session.ApproachLens(lens.LensId, NormalizedLensNearness(_targetCenter, LensCenter(lens)));
        }
        else
        {
            Session.LeaveLens();
        }

        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Carried, "HX-002");
        RequestRender();
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_isHolding)
        {
            base.OnMouseUp(e);
            return;
        }

        _isHolding = false;
        Capture = false;
        Cursor = Cursors.Default;

        var lens = DetectLens(_targetCenter, 176f);
        if (lens is not null)
        {
            Session.ApproachLens(lens.LensId, 1f);
            Session.PlayAbsorption();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.NearSurface, "HX-002");
        }
        else
        {
            Session.DropFree();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
        }

        RequestRender();
        base.OnMouseUp(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        RenderFrame();
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        RenderFrame();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RenderFrame();
    }

    private void Tick()
    {
        _phase += 0.018f;
        Session.Advance(_timer.Interval);

        var dx = _targetCenter.X - _thingCenter.X;
        var dy = _targetCenter.Y - _thingCenter.Y;
        var spring = _isHolding ? 0.30f : 0.13f;
        var damping = _isHolding ? 0.48f : 0.70f;
        _velocity = new PointF(
            (_velocity.X * damping) + (dx * spring),
            (_velocity.Y * damping) + (dy * spring));
        _thingCenter = new PointF(_thingCenter.X + _velocity.X, _thingCenter.Y + _velocity.Y);

        if (Session.AbsorptionProgress >= 1f)
        {
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
        }

        if (_renderRequested ||
            _isHolding ||
            Session.LensesVisible ||
            Session.AbsorptionProgress > 0 ||
            Math.Abs(_velocity.X) > 0.05f ||
            Math.Abs(_velocity.Y) > 0.05f)
        {
            RenderFrame();
            _renderRequested = false;
        }
    }

    private void ActivatePickAt(Point point)
    {
        if (!ClientRectangle.Contains(point))
        {
            point = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        }

        _thingCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
        _velocity = PointF.Empty;
        _grabOffset = PointF.Empty;
        _isHolding = true;
        Capture = true;
        Cursor = Cursors.SizeAll;
        Session.Pick();
        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
        RequestRender();
    }

    private LivingLensRenderState CreateRenderState(bool drawTestBackground)
    {
        var lensCenter = LensCenter(Session.ActiveLens);
        return new LivingLensRenderState
        {
            Variant = Session.ActiveVariant.Kind,
            ExtremeFxMode = Session.ExtremeFxMode,
            EffectIntensity = Session.EffectIntensity,
            DebugVisible = Session.DebugVisible,
            AbsorptionDurationMs = Session.AbsorptionDurationMs,
            Phase = _phase,
            EmergenceProgress = Session.LensEmergenceProgress,
            OpenProgress = Session.LensOpenProgress,
            AbsorptionProgress = Session.AbsorptionProgress,
            TargetEmergenceProgress = Session.TargetEmergenceProgress,
            ThingRecoveryProgress = Session.ThingRecoveryProgress,
            TiltX = Session.TiltX,
            TiltY = Session.TiltY,
            ShadowX = Session.ShadowX,
            ShadowY = Session.ShadowY,
            ThingCompact = Session.ThingCompact,
            ThingPartiallyOccluded = Session.ThingPartiallyOccluded,
            GripShadowVisible = Session.GripShadowVisible,
            ThingCenter = _thingCenter,
            LensCenter = lensCenter,
            LensLabel = Session.ActiveLens.Label,
            NameVisibility = Session.NameVisibility,
            DrawTestBackground = drawTestBackground,
            DrawThing = true,
            DrawLens = Session.LensesVisible,
            DrawGhost = Session.TargetGhostVisible
        };
    }

    private void RenderFrame()
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        PerPixelAlphaWindowRenderer.Render(this, (graphics, size) =>
        {
            LivingLensRenderer.DrawScene(graphics, size, CreateRenderState(false));
        });
    }

    private void RequestRender()
    {
        _renderRequested = true;
    }

    private LivingLensTarget? DetectLens(PointF point, float radius)
    {
        foreach (var lens in Session.Lenses)
        {
            if (Distance(point, LensCenter(lens)) < radius)
            {
                return lens;
            }
        }

        return null;
    }

    private PointF LensCenter(LivingLensTarget lens)
    {
        return new PointF(ClientSize.Width * lens.X, ClientSize.Height * lens.Y);
    }

    private static float NormalizedLensNearness(PointF point, PointF center)
    {
        return Math.Clamp(1f - (Distance(point, center) / 220f), 0f, 1f);
    }

    private static float Distance(PointF left, PointF right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }
}
