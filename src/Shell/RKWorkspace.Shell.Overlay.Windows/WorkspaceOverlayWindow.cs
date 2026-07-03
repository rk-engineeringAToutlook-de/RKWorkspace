using System.Drawing.Drawing2D;
using RKWorkspace.Shell;

namespace RKWorkspace.Shell.Overlay.Windows;

public sealed class WorkspaceOverlayWindow : Form
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly System.Windows.Forms.Timer _motionTimer = new();
    private readonly Rectangle _screenBounds;
    private readonly SizeF _thingSize = new(190, 96);
    private PointF _homeCenter;
    private PointF _visualCenter;
    private PointF _targetCenter;
    private PointF _velocity;
    private PointF _grabOffset;
    private bool _isHolding;
    private float _angle;

    public WorkspaceOverlayWindow(WorkspaceShellRuntime runtime)
    {
        _runtime = runtime;
        Session = new WorkspaceOverlayDemoSession();
        _screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(100, 100, 1280, 720);
        _homeCenter = new PointF(280, 240);
        _visualCenter = _homeCenter;
        _targetCenter = _homeCenter;

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

        _motionTimer.Interval = 16;
        _motionTimer.Tick += (_, _) => TickMotion();

        Session.Start();
    }

    public WorkspaceOverlayDemoSession Session { get; }

    public bool SmokeCheck()
    {
        return FormBorderStyle == FormBorderStyle.None &&
            TopMost &&
            !ShowInTaskbar &&
            TransparencyKey == Color.Magenta &&
            Bounds.Width > 0 &&
            Bounds.Height > 0 &&
            Session.SmokeCheck();
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
            Close();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var point = e.Location;
        if (!_isHolding && GetThingBounds().Contains(point))
        {
            Session.MarkCandidate();
            Cursor = Cursors.Hand;
            Invalidate();
            return;
        }

        if (!_isHolding)
        {
            Cursor = Cursors.Default;
            if (Session.State == WorkspaceOverlayState.CarryCandidate)
            {
                Session.Start();
                Invalidate();
            }

            return;
        }

        _targetCenter = new PointF(point.X - _grabOffset.X, point.Y - _grabOffset.Y);
        var moved = Distance(_targetCenter, _homeCenter) > 14;
        var nearAblage = DetectAblage(_targetCenter);
        if (!string.IsNullOrWhiteSpace(nearAblage))
        {
            Session.NearAblage(nearAblage);
        }
        else if (moved && Session.State != WorkspaceOverlayState.Carried)
        {
            Session.Carry();
        }

        Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !GetThingBounds().Contains(e.Location))
        {
            base.OnMouseDown(e);
            return;
        }

        _isHolding = true;
        _grabOffset = new PointF(e.Location.X - _visualCenter.X, e.Location.Y - _visualCenter.Y);
        Capture = true;
        Cursor = Cursors.SizeAll;
        Session.Pick();
        _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-001A");
        Invalidate();
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
        var ablage = DetectAblage(_targetCenter);
        if (!string.IsNullOrWhiteSpace(ablage))
        {
            Session.NearAblage(ablage);
            Session.Place();
            _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-002");
            _targetCenter = CenterOfAblage(ablage);
        }
        else
        {
            Session.Cancel();
            _runtime.Shell.UpdateCarryState(Session.CarryState, "HX-001");
            _targetCenter = _homeCenter;
        }

        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawDimmedWorkRoom(graphics);
        DrawAblage(graphics, "links", GetLeftAblage(), "Ablage links");
        DrawAblage(graphics, "rechts", GetRightAblage(), "Ablage rechts");
        DrawThing(graphics);
        DrawMessage(graphics);
        DrawExitHint(graphics);
    }

    private void TickMotion()
    {
        var dx = _targetCenter.X - _visualCenter.X;
        var dy = _targetCenter.Y - _visualCenter.Y;
        _velocity = new PointF(
            (_velocity.X * 0.70f) + (dx * 0.16f),
            (_velocity.Y * 0.70f) + (dy * 0.16f));
        _visualCenter = new PointF(_visualCenter.X + _velocity.X, _visualCenter.Y + _velocity.Y);
        _angle = Math.Clamp(_velocity.X * 0.08f, -4.5f, 4.5f);

        if (!_isHolding &&
            Session.State == WorkspaceOverlayState.Cancelled &&
            Distance(_visualCenter, _homeCenter) < 2)
        {
            _visualCenter = _homeCenter;
            _targetCenter = _homeCenter;
            _velocity = PointF.Empty;
            Session.Start();
        }

        Invalidate();
    }

    private void DrawDimmedWorkRoom(Graphics graphics)
    {
        using var brush = new SolidBrush(Color.FromArgb(18, 10, 16, 22));
        graphics.FillRectangle(brush, ClientRectangle);
    }

    private void DrawAblage(Graphics graphics, string id, Rectangle bounds, string label)
    {
        var active = string.Equals(Session.ActiveAblage, id, StringComparison.Ordinal) ||
            (Session.State == WorkspaceOverlayState.Placed && string.Equals(id, "rechts", StringComparison.Ordinal));
        var borderColor = active ? Color.FromArgb(210, 88, 220, 170) : Color.FromArgb(110, 208, 216, 226);
        var fillColor = active ? Color.FromArgb(54, 82, 196, 150) : Color.FromArgb(24, 232, 236, 240);
        using var fill = new SolidBrush(fillColor);
        using var border = new Pen(borderColor, active ? 3f : 1.5f);
        using var path = RoundedRectangle(bounds, 8);
        graphics.FillPath(fill, path);
        graphics.DrawPath(border, path);

        using var titleFont = new Font(Font.FontFamily, 15, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            label,
            titleFont,
            bounds,
            active ? Color.FromArgb(240, 242, 248, 244) : Color.FromArgb(185, 242, 248, 244),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawThing(Graphics graphics)
    {
        var bounds = GetThingBounds();
        var stateLift = Session.State is WorkspaceOverlayState.Picked or WorkspaceOverlayState.Carried or WorkspaceOverlayState.NearAblage;
        var shadowOffset = stateLift ? 18 : 8;
        var previous = graphics.Transform;
        graphics.TranslateTransform(_visualCenter.X, _visualCenter.Y);
        graphics.RotateTransform(_angle);
        graphics.TranslateTransform(-_visualCenter.X, -_visualCenter.Y);

        using var shadowPath = RoundedRectangle(
            new Rectangle(bounds.X + shadowOffset, bounds.Y + shadowOffset, bounds.Width, bounds.Height),
            8);
        using var shadow = new SolidBrush(Color.FromArgb(stateLift ? 78 : 42, 0, 0, 0));
        graphics.FillPath(shadow, shadowPath);

        using var cardPath = RoundedRectangle(bounds, 8);
        using var fill = new LinearGradientBrush(
            bounds,
            Color.FromArgb(246, 252, 252, 248),
            Color.FromArgb(232, 226, 238, 248),
            LinearGradientMode.Vertical);
        using var border = new Pen(
            stateLift ? Color.FromArgb(235, 88, 164, 230) : Color.FromArgb(185, 120, 134, 150),
            stateLift ? 2f : 1.2f);
        graphics.FillPath(fill, cardPath);
        graphics.DrawPath(border, cardPath);

        using var labelFont = new Font(Font.FontFamily, 12, FontStyle.Bold);
        using var contentFont = new Font(Font.FontFamily, 10, FontStyle.Regular);
        using var hintFont = new Font(Font.FontFamily, 9, FontStyle.Regular);
        TextRenderer.DrawText(
            graphics,
            Session.ThingLabel,
            labelFont,
            new Rectangle(bounds.X + 18, bounds.Y + 16, bounds.Width - 36, 24),
            Color.FromArgb(38, 48, 58),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            Session.ThingContent,
            contentFont,
            new Rectangle(bounds.X + 18, bounds.Y + 42, bounds.Width - 36, 22),
            Color.FromArgb(70, 82, 92),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            Session.Message,
            hintFont,
            new Rectangle(bounds.X + 18, bounds.Y + 66, bounds.Width - 36, 20),
            Color.FromArgb(84, 104, 118),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        graphics.Transform = previous;
    }

    private void DrawMessage(Graphics graphics)
    {
        if (string.IsNullOrWhiteSpace(Session.Message))
        {
            return;
        }

        var bounds = new Rectangle((ClientSize.Width - 280) / 2, 54, 280, 44);
        using var path = RoundedRectangle(bounds, 8);
        using var fill = new SolidBrush(Color.FromArgb(118, 20, 30, 42));
        graphics.FillPath(fill, path);
        using var font = new Font(Font.FontFamily, 12, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            Session.Message,
            font,
            bounds,
            Color.FromArgb(235, 244, 248, 250),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawExitHint(Graphics graphics)
    {
        var bounds = new Rectangle(ClientSize.Width - 230, ClientSize.Height - 48, 210, 28);
        using var font = new Font(Font.FontFamily, 9, FontStyle.Regular);
        TextRenderer.DrawText(
            graphics,
            "Esc beendet die Ebene",
            font,
            bounds,
            Color.FromArgb(160, 244, 248, 250),
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private Rectangle GetThingBounds()
    {
        return new Rectangle(
            (int)Math.Round(_visualCenter.X - (_thingSize.Width / 2)),
            (int)Math.Round(_visualCenter.Y - (_thingSize.Height / 2)),
            (int)Math.Round(_thingSize.Width),
            (int)Math.Round(_thingSize.Height));
    }

    private Rectangle GetLeftAblage()
    {
        return new Rectangle(22, 120, 120, Math.Max(260, ClientSize.Height - 240));
    }

    private Rectangle GetRightAblage()
    {
        return new Rectangle(ClientSize.Width - 142, 120, 120, Math.Max(260, ClientSize.Height - 240));
    }

    private string DetectAblage(PointF center)
    {
        var point = Point.Round(center);
        if (GetLeftAblage().Contains(point))
        {
            return "links";
        }

        if (GetRightAblage().Contains(point))
        {
            return "rechts";
        }

        return string.Empty;
    }

    private PointF CenterOfAblage(string ablage)
    {
        var bounds = string.Equals(ablage, "links", StringComparison.Ordinal)
            ? GetLeftAblage()
            : GetRightAblage();
        return new PointF(bounds.Left + (bounds.Width / 2f), bounds.Top + (bounds.Height / 2f));
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
