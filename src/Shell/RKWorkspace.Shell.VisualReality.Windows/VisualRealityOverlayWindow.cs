using System.Drawing.Drawing2D;
using RKWorkspace.Shell;

namespace RKWorkspace.Shell.VisualReality.Windows;

public sealed class VisualRealityOverlayWindow : Form
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly System.Windows.Forms.Timer _motionTimer = new();
    private readonly Rectangle _screenBounds;
    private readonly SizeF _baseThingSize = new(210, 108);
    private PointF _visualCenter;
    private PointF _targetCenter;
    private PointF _velocity;
    private PointF _lastTargetCenter;
    private PointF _grabOffset;
    private float _phase;
    private bool _isHolding;
    private int _glideTicks;
    private string _pendingPlaceLens = string.Empty;

    public VisualRealityOverlayWindow(WorkspaceShellRuntime runtime)
    {
        _runtime = runtime;
        Session = new VisualRealitySession();
        _screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(100, 100, 1280, 720);
        _visualCenter = new PointF(_screenBounds.Width * 0.36f, _screenBounds.Height * 0.48f);
        _targetCenter = _visualCenter;
        _lastTargetCenter = _targetCenter;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = _screenBounds;
        TopMost = true;
        ShowInTaskbar = false;
        KeyPreview = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        Cursor = Cursors.Default;
        DoubleBuffered = true;
        Text = string.Empty;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.SupportsTransparentBackColor,
            true);

        _motionTimer.Interval = 16;
        _motionTimer.Tick += (_, _) => TickMotion();

        Session.Start();
    }

    public VisualRealitySession Session { get; }

    public bool IsNativeOverlay => true;

    public bool HasBrowserSurface => false;

    public bool DesktopVisiblePrepared => TransparencyKey == Color.Magenta &&
        BackColor == Color.Magenta &&
        FormBorderStyle == FormBorderStyle.None &&
        !ShowInTaskbar;

    public bool EscExitReady { get; private set; } = true;

    public bool SmokeVariantSwitchingOk { get; private set; }

    public bool SmokeSlowEmergenceOk { get; private set; }

    public bool SmokeLensLivingOk { get; private set; }

    public bool SmokeReferenceDirectionOk { get; private set; }

    public bool SmokeDigitalHandOk { get; private set; }

    public bool SmokeVectorResponseOk { get; private set; }

    public bool SmokeDiagonalVectorOk { get; private set; }

    public bool SmokeLensOpenOk { get; private set; }

    public bool SmokeMiniAblageOk { get; private set; }

    public bool SmokeGlideOk { get; private set; }

    public bool SmokeTargetGhostOk { get; private set; }

    public bool SmokeCheck()
    {
        var hiddenBeforePick = !Session.LensesVisible && Session.ThingVisible;
        SmokeVariantSwitchingOk = Session.LensVariants.Count >= 5 && Session.CheckVariantSwitching();
        SmokeLensLivingOk = Session.LensVariants.All(variant =>
            variant.UsesLivingMotion &&
            variant.UsesDepth &&
            variant.PreservesRealDesktop &&
            variant.AvoidsSpaceBackdrop &&
            variant.AvoidsGreenPointUi &&
            variant.AvoidsButtonShape);
        SmokeReferenceDirectionOk = Session.ReferenceBoardDirectionPrepared &&
            Session.SpaceBackdropRejected &&
            Session.DesktopRemainsVisible;

        Session.Start();
        Session.Pick();
        SmokeDigitalHandOk = Session.ThingCompact &&
            Session.ThingPartiallyOccluded &&
            Session.GripShadowVisible &&
            Session.OpticalHapticsPrepared;
        var emergenceStarted = Session.LensEmergenceProgress > 0 && Session.LensEmergenceProgress < 1;
        Session.AdvanceLensEmergence(Session.LensEmergenceDurationMs);
        SmokeSlowEmergenceOk = emergenceStarted &&
            Math.Abs(Session.LensEmergenceProgress - 1f) < 0.001f &&
            Session.LensEmergenceDurationMs is >= 1000 and <= 2000;
        Session.Carry(22, -18);
        SmokeVectorResponseOk = Math.Abs(Session.TiltX) > 0.01f && Math.Abs(Session.TiltY) > 0.01f;
        SmokeDiagonalVectorOk = Session.CheckDiagonalVectorResponses();
        Session.ApproachLens("monitor", 0.96f);
        Session.OpenLens();
        SmokeLensOpenOk = Session.LensOpen && Session.LensDepthVisible;
        SmokeMiniAblageOk = Session.MiniAblageVisible;
        Session.GlideIntoLens();
        SmokeGlideOk = Session.IsGlidingIntoLens && Session.SourceVisualProgress > 0 && Session.TargetVisualProgress > 0;
        SmokeTargetGhostOk = Session.TargetGhostVisible;
        Session.SetTargetPosition(0.62f, 0.44f);
        Session.Place();

        return IsNativeOverlay &&
            !HasBrowserSurface &&
            DesktopVisiblePrepared &&
            hiddenBeforePick &&
            SmokeVariantSwitchingOk &&
            SmokeSlowEmergenceOk &&
            SmokeLensLivingOk &&
            SmokeReferenceDirectionOk &&
            SmokeDigitalHandOk &&
            SmokeVectorResponseOk &&
            SmokeDiagonalVectorOk &&
            SmokeLensOpenOk &&
            SmokeMiniAblageOk &&
            SmokeGlideOk &&
            SmokeTargetGhostOk &&
            EscExitReady &&
            Session.GreenPointStyleRejected &&
            Session.ButtonTargetShapeRejected &&
            Session.FuturePhysicalHapticsMarked;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        _motionTimer.Start();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _motionTimer.Stop();
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
            Invalidate();
            return;
        }

        if (e.KeyCode is >= Keys.NumPad1 and <= Keys.NumPad5)
        {
            e.Handled = true;
            Session.SwitchVariant(e.KeyCode - Keys.NumPad1);
            Invalidate();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && GetThingBounds().Contains(e.Location))
        {
            _isHolding = true;
            _grabOffset = new PointF(e.Location.X - _visualCenter.X, e.Location.Y - _visualCenter.Y);
            Capture = true;
            Cursor = Cursors.SizeAll;
            if (Session.State is VisualRealityState.Listening or VisualRealityState.Cancelled)
            {
                Session.Pick();
                _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            }

            Invalidate();
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

        var lens = DetectLens(_targetCenter);
        if (lens is not null)
        {
            var distance = NormalizedLensNearness(_targetCenter, LensCenter(lens));
            Session.ApproachLens(lens.LensId, distance);
            if (distance >= 0.92f)
            {
                Session.OpenLens();
            }
        }

        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Carried, "HX-002");
        Invalidate();
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

        var lens = DetectLens(_targetCenter);
        if (lens is not null && Session.LensOpen)
        {
            Session.GlideIntoLens();
            _pendingPlaceLens = lens.LensId;
            _glideTicks = 20;
            _targetCenter = LensCenter(lens);
        }
        else
        {
            Session.Cancel();
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Cancelled, "HX-001");
        }

        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawLenses(graphics);
        DrawThing(graphics);
    }

    private void ActivatePickAt(Point point)
    {
        if (!ClientRectangle.Contains(point))
        {
            point = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        }

        _visualCenter = point;
        _targetCenter = point;
        _lastTargetCenter = point;
        _velocity = PointF.Empty;
        _grabOffset = PointF.Empty;
        _isHolding = true;
        Capture = true;
        Cursor = Cursors.SizeAll;
        Session.Pick();
        _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
        Invalidate();
    }

    private void TickMotion()
    {
        _phase += 0.022f;
        if (Session.State is not VisualRealityState.Listening and not VisualRealityState.Placed and not VisualRealityState.Cancelled)
        {
            Session.AdvanceLensEmergence(_motionTimer.Interval);
        }

        var dx = _targetCenter.X - _visualCenter.X;
        var dy = _targetCenter.Y - _visualCenter.Y;
        _velocity = new PointF(
            (_velocity.X * 0.68f) + (dx * 0.14f),
            (_velocity.Y * 0.68f) + (dy * 0.14f));
        _visualCenter = new PointF(_visualCenter.X + _velocity.X, _visualCenter.Y + _velocity.Y);

        if (_glideTicks > 0)
        {
            _glideTicks--;
            if (_glideTicks == 0 && !string.IsNullOrWhiteSpace(_pendingPlaceLens))
            {
                var relative = RelativePositionInLens(_pendingPlaceLens, _visualCenter);
                Session.SetTargetPosition(relative.X, relative.Y);
                Session.Place();
                _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Placed, "HX-002");
                _pendingPlaceLens = string.Empty;
            }
        }

        Invalidate();
    }

    private void DrawThing(Graphics graphics)
    {
        if (!Session.ThingVisible)
        {
            return;
        }

        var bounds = GetThingBounds();
        var previous = graphics.Transform;
        graphics.TranslateTransform(_visualCenter.X, _visualCenter.Y);
        graphics.RotateTransform(Session.TiltX * 0.55f);
        graphics.TranslateTransform(-_visualCenter.X, -_visualCenter.Y);

        using var shadowPath = RoundedRectangle(
            new Rectangle(
                (int)Math.Round(bounds.X + Session.ShadowX),
                (int)Math.Round(bounds.Y + Session.ShadowY),
                bounds.Width,
                bounds.Height),
            9);
        using var shadow = new SolidBrush(Color.FromArgb(Session.GripShadowVisible ? 78 : 40, 0, 0, 0));
        graphics.FillPath(shadow, shadowPath);

        using var cardPath = RoundedRectangle(bounds, 9);
        using var fill = new LinearGradientBrush(
            bounds,
            Color.FromArgb(244, 252, 252, 248),
            Color.FromArgb(230, 222, 234, 240),
            LinearGradientMode.Vertical);
        using var border = new Pen(Color.FromArgb(176, 126, 142, 150), 1.2f);
        graphics.FillPath(fill, cardPath);
        graphics.DrawPath(border, cardPath);

        if (Session.ThingPartiallyOccluded)
        {
            using var grip = new LinearGradientBrush(
                new Rectangle(bounds.X - 20, bounds.Y - 5, (int)(bounds.Width * 0.48f), bounds.Height + 10),
                Color.FromArgb(92, 24, 32, 34),
                Color.FromArgb(0, 24, 32, 34),
                LinearGradientMode.Horizontal);
            graphics.FillRectangle(grip, bounds.X - 20, bounds.Y - 5, (int)(bounds.Width * 0.48f), bounds.Height + 10);
        }

        using var labelFont = new Font(Font.FontFamily, 12, FontStyle.Bold);
        using var hintFont = new Font(Font.FontFamily, 10, FontStyle.Regular);
        TextRenderer.DrawText(
            graphics,
            Session.ThingContent,
            labelFont,
            new Rectangle(bounds.X + 22, bounds.Y + 28, bounds.Width - 44, 28),
            Color.FromArgb(38, 48, 58),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            Session.State == VisualRealityState.Placed ? "Liegt hier" : string.Empty,
            hintFont,
            new Rectangle(bounds.X + 22, bounds.Y + 58, bounds.Width - 44, 22),
            Color.FromArgb(84, 104, 118),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        graphics.Transform = previous;
        previous.Dispose();
    }

    private void DrawLenses(Graphics graphics)
    {
        if (!Session.LensesVisible)
        {
            return;
        }

        var emergence = EaseOut(Session.LensEmergenceProgress);
        foreach (var lens in Session.Lenses)
        {
            DrawLens(graphics, lens, emergence);
        }
    }

    private void DrawLens(Graphics graphics, VisualRealityLens lens, float emergence)
    {
        var center = LensCenter(lens);
        var active = string.Equals(Session.ActiveLensId, lens.LensId, StringComparison.Ordinal);
        var living = MathF.Sin(_phase + ((int)lens.Edge * 0.8f)) * 3.5f;
        var baseSize = active ? 132 : 90;
        var size = (int)Math.Round((baseSize + living) * Math.Max(0.18f, emergence));
        var bounds = new Rectangle(
            (int)Math.Round(center.X - (size / 2f)),
            (int)Math.Round(center.Y - (size / 2f)),
            size,
            size);

        switch (Session.ActiveVariant.Kind)
        {
            case VisualRealityLensKind.ReferenceLens:
                DrawReferenceLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.Glass:
                DrawGlassMaterialLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.GravityWell:
                DrawGravityWellLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.QuietPortal:
                DrawQuietPortalLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.MinimalRift:
                DrawMinimalRiftLens(graphics, bounds, active, emergence);
                break;
        }

        if (active && Session.MiniAblageVisible)
        {
            DrawMiniAblage(graphics, bounds);
        }

        DrawLensText(graphics, lens, bounds, active);
    }

    private void DrawReferenceLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        var oval = LensMaterialBounds(bounds);
        DrawLensAmbient(graphics, oval, active, emergence);
        DrawGlassBody(graphics, oval, active, emergence);
        DrawGravityWell(graphics, oval, active, emergence, active ? 0.86f : 0.34f);

        if (active)
        {
            DrawPortalOpening(graphics, oval, emergence, Session.LensOpen ? 1.0f : 0.46f);
        }
        else
        {
            DrawQuietDepth(graphics, oval, emergence);
        }
    }

    private void DrawGlassMaterialLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        var oval = LensMaterialBounds(bounds);
        DrawLensAmbient(graphics, oval, active, emergence);
        DrawGlassBody(graphics, oval, active, emergence);
        DrawQuietDepth(graphics, oval, emergence);
    }

    private void DrawGravityWellLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        var oval = LensMaterialBounds(bounds);
        DrawLensAmbient(graphics, oval, active, emergence);
        DrawGlassBody(graphics, oval, active, emergence);
        DrawGravityWell(graphics, oval, active, emergence, active ? 1.0f : 0.54f);

        if (active && Session.LensOpen)
        {
            DrawPortalOpening(graphics, oval, emergence, 0.72f);
        }
    }

    private void DrawQuietPortalLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        var oval = LensMaterialBounds(bounds);
        DrawLensAmbient(graphics, oval, active, emergence);
        DrawGlassBody(graphics, oval, active, emergence);
        DrawGravityWell(graphics, oval, active, emergence, 0.64f);
        DrawPortalOpening(graphics, oval, emergence, active ? 1.0f : 0.34f);
    }

    private void DrawLensAmbient(Graphics graphics, Rectangle oval, bool active, float emergence)
    {
        var ambient = Rectangle.Inflate(oval, active ? 34 : 20, active ? 24 : 14);
        using var ambientPath = new GraphicsPath();
        ambientPath.AddEllipse(ambient);
        using var ambientBrush = new PathGradientBrush(ambientPath)
        {
            CenterColor = Color.FromArgb(Alpha(active ? 48 : 24, emergence), 166, 194, 210),
            SurroundColors = [Color.FromArgb(0, 166, 194, 210)]
        };
        graphics.FillPath(ambientBrush, ambientPath);

        var floorShadow = new Rectangle(oval.X + (oval.Width / 9), oval.Bottom - Math.Max(7, oval.Height / 14), oval.Width - (oval.Width / 5), Math.Max(10, oval.Height / 7));
        using var floorBrush = new LinearGradientBrush(
            floorShadow,
            Color.FromArgb(Alpha(active ? 72 : 42, emergence), 8, 12, 14),
            Color.FromArgb(0, 8, 12, 14),
            LinearGradientMode.Vertical);
        graphics.FillEllipse(floorBrush, floorShadow);
    }

    private void DrawGlassBody(Graphics graphics, Rectangle oval, bool active, float emergence)
    {
        using var lensPath = new GraphicsPath();
        lensPath.AddEllipse(oval);
        using var body = new PathGradientBrush(lensPath)
        {
            CenterColor = Color.FromArgb(Alpha(active ? 88 : 58, emergence), 225, 236, 240),
            SurroundColors = [Color.FromArgb(Alpha(active ? 154 : 98, emergence), 92, 112, 122)],
            FocusScales = new PointF(0.34f, 0.22f)
        };
        graphics.FillPath(body, lensPath);

        DrawRefractionLines(graphics, oval, lensPath, active, emergence);

        using var lowerRim = new Pen(Color.FromArgb(Alpha(active ? 176 : 112, emergence), 30, 38, 42), active ? 2.4f : 1.4f);
        using var upperRim = new Pen(Color.FromArgb(Alpha(active ? 218 : 146, emergence), 248, 252, 255), active ? 2.2f : 1.3f);
        using var innerRim = new Pen(Color.FromArgb(Alpha(active ? 84 : 54, emergence), 248, 252, 255), 1.0f);
        graphics.DrawArc(upperRim, oval, 202, 146);
        graphics.DrawArc(lowerRim, oval, 26, 152);
        graphics.DrawEllipse(innerRim, Rectangle.Inflate(oval, -Math.Max(8, oval.Width / 13), -Math.Max(6, oval.Height / 12)));

        using var highlight = new LinearGradientBrush(
            new Rectangle(oval.X + oval.Width / 2, oval.Y + oval.Height / 7, oval.Width / 3, oval.Height / 3),
            Color.FromArgb(Alpha(active ? 138 : 88, emergence), 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillEllipse(highlight, oval.X + oval.Width / 2, oval.Y + oval.Height / 7, oval.Width / 3, oval.Height / 3);

        using var secondaryHighlight = new SolidBrush(Color.FromArgb(Alpha(active ? 70 : 42, emergence), 255, 255, 255));
        graphics.FillEllipse(secondaryHighlight, oval.X + oval.Width / 7, oval.Y + oval.Height / 3, oval.Width / 5, oval.Height / 7);
    }

    private void DrawRefractionLines(Graphics graphics, Rectangle oval, GraphicsPath clipPath, bool active, float emergence)
    {
        var previousClip = graphics.Clip;
        graphics.SetClip(clipPath, CombineMode.Intersect);

        using var coolLine = new Pen(Color.FromArgb(Alpha(active ? 52 : 32, emergence), 238, 248, 255), 1.0f);
        using var darkLine = new Pen(Color.FromArgb(Alpha(active ? 34 : 22, emergence), 34, 48, 56), 1.0f);
        for (var index = -2; index <= 2; index++)
        {
            var y = oval.Y + (oval.Height / 2) + (index * oval.Height / 9) + (int)(MathF.Sin(_phase + index) * (active ? 2.4f : 1.1f));
            graphics.DrawBezier(coolLine, oval.Left + 8, y, oval.Left + oval.Width / 3, y - 10, oval.Right - oval.Width / 3, y + 10, oval.Right - 8, y - 2);
        }

        graphics.DrawBezier(darkLine, oval.Left + oval.Width / 10, oval.Top + oval.Height / 5, oval.Left + oval.Width / 3, oval.Top + oval.Height / 7, oval.Right - oval.Width / 4, oval.Bottom - oval.Height / 4, oval.Right - oval.Width / 9, oval.Bottom - oval.Height / 5);

        graphics.Clip = previousClip;
        previousClip.Dispose();
    }

    private void DrawGravityWell(Graphics graphics, Rectangle oval, bool active, float emergence, float strength)
    {
        var center = new PointF(oval.X + (oval.Width * 0.52f), oval.Y + (oval.Height * 0.54f));
        var ringCount = active ? 6 : 4;
        for (var index = 0; index < ringCount; index++)
        {
            var factor = 0.76f - (index * 0.095f);
            var ringWidth = Math.Max(10, (int)(oval.Width * factor));
            var ringHeight = Math.Max(6, (int)(oval.Height * factor * 0.62f));
            var ring = new Rectangle(
                (int)Math.Round(center.X - (ringWidth / 2f)),
                (int)Math.Round(center.Y - (ringHeight / 2f) + (index * 2.2f * strength)),
                ringWidth,
                ringHeight);
            var alpha = Alpha((active ? 62 : 34) * strength * (1.0f - (index * 0.09f)), emergence);
            using var ringPen = new Pen(Color.FromArgb(alpha, 218, 238, 248), index == 0 ? 1.2f : 0.9f);
            graphics.DrawEllipse(ringPen, ring);
        }

        using var foldPen = new Pen(Color.FromArgb(Alpha(active ? 44 * strength : 24 * strength, emergence), 190, 220, 235), 0.9f);
        for (var angle = -60; angle <= 60; angle += 30)
        {
            var radians = angle * MathF.PI / 180f;
            var start = new PointF(
                center.X + MathF.Cos(radians) * oval.Width * 0.42f,
                center.Y + MathF.Sin(radians) * oval.Height * 0.28f);
            var end = new PointF(
                center.X + MathF.Cos(radians) * oval.Width * 0.12f,
                center.Y + MathF.Sin(radians) * oval.Height * 0.08f);
            graphics.DrawBezier(foldPen, start, new PointF((start.X + center.X) / 2, start.Y + 10), new PointF((end.X + center.X) / 2, center.Y - 4), end);
        }
    }

    private void DrawQuietDepth(Graphics graphics, Rectangle oval, float emergence)
    {
        var depth = new Rectangle(
            oval.X + oval.Width / 3,
            oval.Y + oval.Height / 3,
            oval.Width / 3,
            oval.Height / 4);
        using var depthPath = new GraphicsPath();
        depthPath.AddEllipse(depth);
        using var depthBrush = new PathGradientBrush(depthPath)
        {
            CenterColor = Color.FromArgb(Alpha(36, emergence), 30, 42, 48),
            SurroundColors = [Color.FromArgb(0, 30, 42, 48)]
        };
        graphics.FillPath(depthBrush, depthPath);
    }

    private void DrawPortalOpening(Graphics graphics, Rectangle oval, float emergence, float openness)
    {
        openness = Math.Clamp(openness, 0.18f, 1f);
        var portalWidth = (int)Math.Round(oval.Width * (0.24f + (0.25f * openness)));
        var portalHeight = (int)Math.Round(oval.Height * (0.18f + (0.22f * openness)));
        var portal = new Rectangle(
            oval.X + (oval.Width - portalWidth) / 2,
            oval.Y + (oval.Height - portalHeight) / 2 + (int)(oval.Height * 0.05f),
            portalWidth,
            portalHeight);
        using var portalPath = new GraphicsPath();
        portalPath.AddEllipse(portal);
        using var portalBrush = new PathGradientBrush(portalPath)
        {
            CenterColor = Color.FromArgb(Alpha(172 * openness, emergence), 8, 12, 18),
            SurroundColors = [Color.FromArgb(Alpha(86 * openness, emergence), 68, 96, 112)],
            FocusScales = new PointF(0.42f, 0.28f)
        };
        graphics.FillPath(portalBrush, portalPath);

        using var aperture = new Pen(Color.FromArgb(Alpha(186 * openness, emergence), 222, 244, 252), Math.Max(1.1f, 1.4f * openness));
        using var apertureShadow = new Pen(Color.FromArgb(Alpha(76 * openness, emergence), 12, 18, 24), 1.0f);
        graphics.DrawArc(aperture, portal, 198, 144);
        graphics.DrawArc(apertureShadow, portal, 20, 154);
    }

    private void DrawMinimalRiftLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        var narrow = new Rectangle(bounds.X + bounds.Width / 3, bounds.Y + bounds.Height / 9, bounds.Width / 3, bounds.Height - bounds.Height / 5);
        using var path = new GraphicsPath();
        path.AddEllipse(narrow);
        using var glow = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb((int)((active ? 95 : 52) * emergence), 250, 252, 255),
            SurroundColors = [Color.FromArgb(0, 250, 252, 255)]
        };
        graphics.FillPath(glow, path);
        using var rim = new Pen(Color.FromArgb((int)((active ? 132 : 72) * emergence), 236, 244, 250), active ? 1.8f : 1.0f);
        graphics.DrawArc(rim, narrow, 68, 224);
    }

    private void DrawMiniAblage(Graphics graphics, Rectangle lensBounds)
    {
        var material = LensMaterialBounds(lensBounds);
        var topY = material.Y + (int)(material.Height * 0.50f);
        var bottomY = material.Y + (int)(material.Height * 0.70f);
        var leftTop = material.X + (int)(material.Width * 0.36f);
        var rightTop = material.X + (int)(material.Width * 0.64f);
        var leftBottom = material.X + (int)(material.Width * 0.24f);
        var rightBottom = material.X + (int)(material.Width * 0.76f);
        var points = new[]
        {
            new Point(leftTop, topY),
            new Point(rightTop, topY),
            new Point(rightBottom, bottomY),
            new Point(leftBottom, bottomY)
        };

        using var path = new GraphicsPath();
        path.AddPolygon(points);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(96, 222, 235, 232),
            SurroundColors = [Color.FromArgb(38, 110, 134, 146)]
        };
        using var border = new Pen(Color.FromArgb(142, 226, 244, 240), 1.1f);
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);

        if (Session.TargetGhostVisible)
        {
            var ghost = new Rectangle(
                leftBottom + (int)((rightBottom - leftBottom) * Session.TargetPositionX * 0.58f),
                topY + (int)((bottomY - topY) * Session.TargetPositionY * 0.38f),
                Math.Max(18, (rightTop - leftTop) / 3),
                Math.Max(8, (bottomY - topY) / 3));
            using var ghostPath = RoundedRectangle(ghost, 5);
            using var ghostBrush = new SolidBrush(Color.FromArgb(124, 255, 255, 255));
            using var ghostBorder = new Pen(Color.FromArgb(110, 160, 190, 204), 0.8f);
            graphics.FillPath(ghostBrush, ghostPath);
            graphics.DrawPath(ghostBorder, ghostPath);
        }
    }

    private void DrawLensText(Graphics graphics, VisualRealityLens lens, Rectangle bounds, bool active)
    {
        if (!active || Session.NameVisibility == VisualRealityLensNameVisibility.Hidden)
        {
            return;
        }

        var alpha = Session.NameVisibility == VisualRealityLensNameVisibility.MicroText ? 96 : 220;
        var text = Session.NameVisibility == VisualRealityLensNameVisibility.ActionReadable
            ? $"{lens.Label}\nHier ablegen"
            : lens.Label;
        using var font = new Font(Font.FontFamily, Session.NameVisibility == VisualRealityLensNameVisibility.MicroText ? 7 : 8, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            text,
            font,
            new Rectangle(bounds.X - 44, bounds.Bottom + 4, bounds.Width + 88, 34),
            Color.FromArgb(alpha, 245, 248, 246),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private Rectangle GetThingBounds()
    {
        var scale = Session.ThingCompact ? 0.88f : 1.0f;
        var width = _baseThingSize.Width * scale;
        var height = _baseThingSize.Height * scale;
        if (Session.IsGlidingIntoLens)
        {
            width *= 0.76f;
            height *= 0.76f;
        }

        return new Rectangle(
            (int)Math.Round(_visualCenter.X - (width / 2)),
            (int)Math.Round(_visualCenter.Y - (height / 2)),
            (int)Math.Round(width),
            (int)Math.Round(height));
    }

    private VisualRealityLens? DetectLens(PointF point)
    {
        foreach (var lens in Session.Lenses)
        {
            if (Distance(point, LensCenter(lens)) < 108)
            {
                return lens;
            }
        }

        return null;
    }

    private VisualRealityLens LensById(string lensId)
    {
        return Session.Lenses.First(lens => string.Equals(lens.LensId, lensId, StringComparison.Ordinal));
    }

    private PointF LensCenter(VisualRealityLens lens)
    {
        return new PointF(ClientSize.Width * lens.X, ClientSize.Height * lens.Y);
    }

    private PointF RelativePositionInLens(string lensId, PointF point)
    {
        var lens = LensById(lensId);
        var center = LensCenter(lens);
        return new PointF(
            Math.Clamp(0.5f + ((point.X - center.X) / 180f), 0.08f, 0.92f),
            Math.Clamp(0.5f + ((point.Y - center.Y) / 180f), 0.08f, 0.92f));
    }

    private static float NormalizedLensNearness(PointF point, PointF center)
    {
        var distance = Distance(point, center);
        return Math.Clamp(1f - (distance / 180f), 0f, 1f);
    }

    private static float Distance(PointF left, PointF right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    private static float EaseOut(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return 1f - MathF.Pow(1f - clamped, 3f);
    }

    private static Rectangle LensMaterialBounds(Rectangle bounds)
    {
        var width = Math.Max(12, (int)Math.Round(bounds.Width * 1.28f));
        var height = Math.Max(8, (int)Math.Round(bounds.Height * 0.82f));
        return new Rectangle(
            bounds.X - ((width - bounds.Width) / 2),
            bounds.Y + ((bounds.Height - height) / 2),
            width,
            height);
    }

    private static int Alpha(float value, float emergence)
    {
        return Math.Clamp((int)Math.Round(value * Math.Clamp(emergence, 0f, 1f)), 0, 255);
    }

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
