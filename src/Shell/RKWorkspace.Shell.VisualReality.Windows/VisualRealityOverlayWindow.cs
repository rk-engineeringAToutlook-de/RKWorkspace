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
            variant.AvoidsGreenPointUi &&
            variant.AvoidsButtonShape);

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
            case VisualRealityLensKind.SoapBubble:
                DrawSoapBubbleLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.Water:
                DrawWaterLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.Glass:
                DrawGlassLens(graphics, bounds, active, emergence);
                break;
            case VisualRealityLensKind.Portal:
                DrawPortalLens(graphics, bounds, active, emergence);
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

    private void DrawSoapBubbleLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb((int)(112 * emergence), 252, 255, 252),
            SurroundColors = [Color.FromArgb((int)((active ? 48 : 32) * emergence), 178, 214, 238)]
        };
        graphics.FillPath(fill, path);
        using var rim = new Pen(Color.FromArgb((int)((active ? 170 : 96) * emergence), 245, 252, 255), active ? 2.3f : 1.2f);
        graphics.DrawPath(rim, path);
        using var shine = new SolidBrush(Color.FromArgb((int)(86 * emergence), 255, 255, 255));
        graphics.FillEllipse(shine, bounds.X + bounds.Width / 5, bounds.Y + bounds.Height / 6, bounds.Width / 3, bounds.Height / 5);
    }

    private void DrawWaterLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var fill = new LinearGradientBrush(
            bounds,
            Color.FromArgb((int)(92 * emergence), 200, 226, 246),
            Color.FromArgb((int)(34 * emergence), 250, 252, 255),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillPath(fill, path);
        using var rim = new Pen(Color.FromArgb((int)((active ? 150 : 76) * emergence), 224, 244, 255), active ? 2.1f : 1.1f);
        graphics.DrawPath(rim, path);
        using var wavePen = new Pen(Color.FromArgb((int)(72 * emergence), 250, 255, 255), 1.2f);
        for (var offset = -1; offset <= 1; offset++)
        {
            var y = bounds.Y + (bounds.Height / 2) + (offset * bounds.Height / 8) + (int)(MathF.Sin(_phase + offset) * 3);
            graphics.DrawBezier(wavePen, bounds.Left + 14, y, bounds.Left + bounds.Width / 3, y - 8, bounds.Right - bounds.Width / 3, y + 8, bounds.Right - 14, y);
        }
    }

    private void DrawGlassLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb((int)(70 * emergence), 238, 246, 252),
            SurroundColors = [Color.FromArgb((int)((active ? 72 : 44) * emergence), 158, 184, 214)]
        };
        graphics.FillPath(fill, path);
        using var rimOuter = new Pen(Color.FromArgb((int)(142 * emergence), 238, 246, 255), active ? 2.4f : 1.4f);
        using var rimInner = new Pen(Color.FromArgb((int)(72 * emergence), 110, 140, 170), 1.0f);
        graphics.DrawPath(rimOuter, path);
        graphics.DrawEllipse(rimInner, Rectangle.Inflate(bounds, -bounds.Width / 8, -bounds.Height / 8));
    }

    private void DrawPortalLens(Graphics graphics, Rectangle bounds, bool active, float emergence)
    {
        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb((int)((active ? 118 : 70) * emergence), 36, 48, 66),
            SurroundColors = [Color.FromArgb((int)(64 * emergence), 224, 240, 250)]
        };
        graphics.FillPath(fill, path);
        using var rim = new Pen(Color.FromArgb((int)((active ? 168 : 84) * emergence), 232, 246, 255), active ? 2.6f : 1.2f);
        graphics.DrawPath(rim, path);
        using var inner = new Pen(Color.FromArgb((int)(92 * emergence), 168, 204, 228), 1.1f);
        graphics.DrawEllipse(inner, Rectangle.Inflate(bounds, -bounds.Width / 5, -bounds.Height / 5));
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
        var mini = new Rectangle(
            lensBounds.X + (lensBounds.Width / 4),
            lensBounds.Y + (lensBounds.Height / 3),
            lensBounds.Width / 2,
            lensBounds.Height / 3);
        using var path = RoundedRectangle(mini, 8);
        using var fill = new LinearGradientBrush(
            mini,
            Color.FromArgb(116, 244, 248, 246),
            Color.FromArgb(72, 186, 206, 214),
            LinearGradientMode.Vertical);
        using var border = new Pen(Color.FromArgb(142, 230, 242, 238), 1.2f);
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);

        if (Session.TargetGhostVisible)
        {
            var ghost = new Rectangle(
                mini.X + (int)(mini.Width * Session.TargetPositionX * 0.45f),
                mini.Y + (int)(mini.Height * Session.TargetPositionY * 0.35f),
                Math.Max(16, mini.Width / 3),
                Math.Max(9, mini.Height / 4));
            using var ghostBrush = new SolidBrush(Color.FromArgb(112, 255, 255, 255));
            graphics.FillRectangle(ghostBrush, ghost);
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
