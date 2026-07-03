using System.Drawing.Drawing2D;
using RKWorkspace.Shell;

namespace RKWorkspace.Shell.NativeOverlay.Windows;

public sealed class NativeSpatialOverlayWindow : Form
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
    private bool _isHolding;
    private int _glideTicks;
    private string _pendingPlaceBubble = string.Empty;

    public NativeSpatialOverlayWindow(WorkspaceShellRuntime runtime)
    {
        _runtime = runtime;
        Session = new NativeSpatialOverlaySession();
        _screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(100, 100, 1280, 720);
        _visualCenter = new PointF(_screenBounds.Width * 0.38f, _screenBounds.Height * 0.46f);
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

    public NativeSpatialOverlaySession Session { get; }

    public bool IsNativeOverlay => true;

    public bool HasBrowserSurface => false;

    public bool IsBorderless => FormBorderStyle == FormBorderStyle.None && !ShowInTaskbar;

    public bool IsTransparentDesktopOverlay => TransparencyKey == Color.Magenta && BackColor == Color.Magenta;

    public bool EscExitReady { get; private set; } = true;

    public bool SmokeDigitalHandOk { get; private set; }

    public bool SmokeDiagonalVectorOk { get; private set; }

    public bool SmokeBubblesOnlyOnCarryOk { get; private set; }

    public bool SmokeBubbleLensOk { get; private set; }

    public bool SmokePortalOk { get; private set; }

    public bool SmokeMiniAblageOk { get; private set; }

    public bool SmokeGlideOk { get; private set; }

    public bool SmokeTargetPositionOk { get; private set; }

    public bool SmokeCheck()
    {
        var emptyBubblesHidden = !Session.BubblesVisible;
        Session.ActivatePick();
        SmokeDigitalHandOk = Session.ThingCompact &&
            Session.ThingPartiallyOccluded &&
            Session.GripShadowVisible &&
            Session.OpticalHapticsPrepared;
        var carryBubblesVisible = Session.BubblesVisible;
        SmokeBubblesOnlyOnCarryOk = emptyBubblesHidden && carryBubblesVisible;
        SmokeDiagonalVectorOk = Session.CheckDiagonalVectorResponses();
        SmokeBubbleLensOk = Session.BubbleLensStyle && Session.GreenPointStyleRejected;
        Session.NearPortal("monitor");
        SmokePortalOk = Session.PortalOpen;
        SmokeMiniAblageOk = Session.MiniAblageVisible;
        Session.GlideIntoPortal();
        SmokeGlideOk = Session.IsGlidingIntoPortal &&
            Session.TargetGhostVisible &&
            Session.SourceVisualProgress > 0 &&
            Session.TargetVisualProgress > 0;
        Session.SetTargetPosition(0.64f, 0.42f);
        Session.Place();
        SmokeTargetPositionOk = Math.Abs(Session.TargetPositionX - 0.64f) < 0.001f &&
            Math.Abs(Session.TargetPositionY - 0.42f) < 0.001f;

        return IsNativeOverlay &&
            !HasBrowserSurface &&
            IsBorderless &&
            TopMost &&
            IsTransparentDesktopOverlay &&
            Session.SmokeCheck() &&
            SmokeDigitalHandOk &&
            SmokeDiagonalVectorOk &&
            SmokeBubblesOnlyOnCarryOk &&
            SmokeBubbleLensOk &&
            SmokePortalOk &&
            SmokeMiniAblageOk &&
            SmokeGlideOk &&
            SmokeTargetPositionOk &&
            EscExitReady;
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
            if (Session.State is WorkspaceOverlayState.Listening or WorkspaceOverlayState.Inactive)
            {
                Session.ActivatePick();
                _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-001A");
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

        var bubbleId = DetectBubble(_targetCenter);
        if (!string.IsNullOrWhiteSpace(bubbleId))
        {
            Session.NearPortal(bubbleId);
        }

        _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-002");
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
        var bubbleId = DetectBubble(_targetCenter);
        if (!string.IsNullOrWhiteSpace(bubbleId))
        {
            Session.NearPortal(bubbleId);
            Session.GlideIntoPortal();
            _pendingPlaceBubble = bubbleId;
            _glideTicks = 18;
            _targetCenter = BubbleCenter(BubbleById(bubbleId));
        }
        else
        {
            Session.Cancel();
            _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-001");
        }

        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawBubbles(graphics);
        DrawThing(graphics);
    }

    private void ActivatePickAt(Point point)
    {
        if (point.X <= 0 || point.Y <= 0 || !ClientRectangle.Contains(point))
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
        Session.ActivatePick();
        _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-001A");
        Invalidate();
    }

    private void TickMotion()
    {
        var dx = _targetCenter.X - _visualCenter.X;
        var dy = _targetCenter.Y - _visualCenter.Y;
        _velocity = new PointF(
            (_velocity.X * 0.68f) + (dx * 0.14f),
            (_velocity.Y * 0.68f) + (dy * 0.14f));
        _visualCenter = new PointF(_visualCenter.X + _velocity.X, _visualCenter.Y + _velocity.Y);

        if (_glideTicks > 0)
        {
            _glideTicks--;
            if (_glideTicks == 0 && !string.IsNullOrWhiteSpace(_pendingPlaceBubble))
            {
                var relative = RelativePositionInBubble(_pendingPlaceBubble, _visualCenter);
                Session.SetTargetPosition(relative.X, relative.Y);
                Session.Place();
                _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-002");
                _pendingPlaceBubble = string.Empty;
            }
        }

        Invalidate();
    }

    private void DrawThing(Graphics graphics)
    {
        if (!Session.DemoThingCreated && Session.State == WorkspaceOverlayState.Listening)
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
            8);
        using var shadow = new SolidBrush(Color.FromArgb(Session.GripShadowVisible ? 72 : 36, 0, 0, 0));
        graphics.FillPath(shadow, shadowPath);

        using var cardPath = RoundedRectangle(bounds, 8);
        using var fill = new LinearGradientBrush(
            bounds,
            Color.FromArgb(240, 252, 252, 248),
            Color.FromArgb(228, 225, 236, 242),
            LinearGradientMode.Vertical);
        using var border = new Pen(Color.FromArgb(176, 126, 142, 150), 1.3f);
        graphics.FillPath(fill, cardPath);
        graphics.DrawPath(border, cardPath);

        if (Session.ThingPartiallyOccluded)
        {
            using var grip = new LinearGradientBrush(
                new Rectangle(bounds.X - 18, bounds.Y - 4, (int)(bounds.Width * 0.48f), bounds.Height + 8),
                Color.FromArgb(88, 28, 36, 34),
                Color.FromArgb(0, 28, 36, 34),
                LinearGradientMode.Horizontal);
            graphics.FillRectangle(grip, bounds.X - 18, bounds.Y - 4, (int)(bounds.Width * 0.48f), bounds.Height + 8);
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
            Session.State == WorkspaceOverlayState.Placed ? "Liegt hier" : string.Empty,
            hintFont,
            new Rectangle(bounds.X + 22, bounds.Y + 58, bounds.Width - 44, 22),
            Color.FromArgb(84, 104, 118),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        graphics.Transform = previous;
    }

    private void DrawBubbles(Graphics graphics)
    {
        if (!Session.BubblesVisible)
        {
            return;
        }

        foreach (var bubble in Session.Bubbles)
        {
            DrawBubble(graphics, bubble);
        }
    }

    private void DrawBubble(Graphics graphics, NativeSpatialBubble bubble)
    {
        var center = BubbleCenter(bubble);
        var active = string.Equals(Session.ActiveBubbleId, bubble.BubbleId, StringComparison.Ordinal);
        var size = active ? 116 : 82;
        var bounds = new Rectangle(
            (int)Math.Round(center.X - (size / 2f)),
            (int)Math.Round(center.Y - (size / 2f)),
            size,
            size);

        using var path = new GraphicsPath();
        path.AddEllipse(bounds);
        using var fill = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(active ? 138 : 92, 250, 255, 250),
            SurroundColors = [Color.FromArgb(active ? 42 : 28, 190, 220, 230)]
        };
        graphics.FillPath(fill, path);
        using var rim = new Pen(Color.FromArgb(active ? 150 : 82, 236, 250, 248), active ? 2.2f : 1.3f);
        graphics.DrawPath(rim, path);

        using var shine = new SolidBrush(Color.FromArgb(active ? 96 : 54, 255, 255, 255));
        graphics.FillEllipse(shine, bounds.X + (bounds.Width / 5), bounds.Y + (bounds.Height / 6), bounds.Width / 3, bounds.Height / 5);

        if (active)
        {
            DrawMiniAblage(graphics, bounds, bubble.Label);
        }
    }

    private void DrawMiniAblage(Graphics graphics, Rectangle bubbleBounds, string label)
    {
        var mini = new Rectangle(
            bubbleBounds.X + (bubbleBounds.Width / 4),
            bubbleBounds.Y + (bubbleBounds.Height / 3),
            bubbleBounds.Width / 2,
            bubbleBounds.Height / 3);
        using var path = RoundedRectangle(mini, 7);
        using var fill = new SolidBrush(Color.FromArgb(96, 245, 250, 246));
        using var border = new Pen(Color.FromArgb(132, 220, 236, 228), 1.2f);
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);

        using var font = new Font(Font.FontFamily, 8, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            label,
            font,
            new Rectangle(bubbleBounds.X - 34, bubbleBounds.Bottom + 4, bubbleBounds.Width + 68, 18),
            Color.FromArgb(218, 245, 248, 246),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private Rectangle GetThingBounds()
    {
        var scale = Session.ThingCompact ? 0.88f : 1.0f;
        var width = _baseThingSize.Width * scale;
        var height = _baseThingSize.Height * scale;
        if (Session.IsGlidingIntoPortal)
        {
            width *= 0.78f;
            height *= 0.78f;
        }

        return new Rectangle(
            (int)Math.Round(_visualCenter.X - (width / 2)),
            (int)Math.Round(_visualCenter.Y - (height / 2)),
            (int)Math.Round(width),
            (int)Math.Round(height));
    }

    private string DetectBubble(PointF point)
    {
        foreach (var bubble in Session.Bubbles)
        {
            if (Distance(point, BubbleCenter(bubble)) < 92)
            {
                return bubble.BubbleId;
            }
        }

        return string.Empty;
    }

    private NativeSpatialBubble BubbleById(string bubbleId)
    {
        return Session.Bubbles.First(bubble => string.Equals(bubble.BubbleId, bubbleId, StringComparison.Ordinal));
    }

    private PointF BubbleCenter(NativeSpatialBubble bubble)
    {
        return new PointF(ClientSize.Width * bubble.X, ClientSize.Height * bubble.Y);
    }

    private PointF RelativePositionInBubble(string bubbleId, PointF point)
    {
        var bubble = BubbleById(bubbleId);
        var center = BubbleCenter(bubble);
        return new PointF(
            Math.Clamp(0.5f + ((point.X - center.X) / 180f), 0.08f, 0.92f),
            Math.Clamp(0.5f + ((point.Y - center.Y) / 180f), 0.08f, 0.92f));
    }

    private static float Distance(PointF left, PointF right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
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
