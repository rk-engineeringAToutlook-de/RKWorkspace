using System.Drawing.Drawing2D;
using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class HumanExperiencePlaygroundSurface : Control
{
    private static readonly SizeF ObjectSize = new(260, 112);
    private readonly System.Windows.Forms.Timer _timer = new();
    private HumanExperiencePlaygroundHypothesis _hypothesis = HumanExperiencePlaygroundState.Hypotheses[0];
    private bool _holding;
    private bool _hasObjectCenter;
    private Point _grabOffset;
    private PointF _objectCenter;
    private PointF _targetCenter;
    private PointF _velocity;

    public HumanExperiencePlaygroundSurface()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(246, 247, 249);
        MinimumSize = new Size(640, 420);
        Cursor = Cursors.Default;
        _timer.Interval = 16;
        _timer.Tick += (_, _) =>
        {
            if (!_holding)
            {
                _timer.Stop();
                return;
            }

            AdvanceMoment();
            Invalidate();
        };
    }

    public void SetHypothesis(HumanExperiencePlaygroundHypothesis hypothesis)
    {
        _hypothesis = hypothesis;
        ResetMoment();
    }

    public void ResetMoment()
    {
        _holding = false;
        _velocity = PointF.Empty;
        _hasObjectCenter = false;
        Capture = false;
        Cursor = Cursors.Default;
        _timer.Stop();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        EnsureObjectCenter();
        var bounds = GetObjectBounds();
        if (!bounds.Contains(e.Location))
        {
            return;
        }

        _holding = true;
        _grabOffset = new Point((int)(e.X - bounds.Left), (int)(e.Y - bounds.Top));
        _targetCenter = new PointF(e.X - _grabOffset.X + ObjectSize.Width / 2F, e.Y - _grabOffset.Y + ObjectSize.Height / 2F);
        Capture = true;
        Cursor = Cursors.Hand;
        _timer.Start();
        AdvanceMoment();
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        EnsureObjectCenter();
        if (_holding)
        {
            _targetCenter = new PointF(e.X - _grabOffset.X + ObjectSize.Width / 2F, e.Y - _grabOffset.Y + ObjectSize.Height / 2F);
            AdvanceMoment();
            Invalidate();
            return;
        }

        Cursor = GetObjectBounds().Contains(e.Location) ? Cursors.Hand : Cursors.Default;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_holding)
        {
            Cursor = Cursors.Default;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_holding)
        {
            return;
        }

        _holding = false;
        _velocity = PointF.Empty;
        Capture = false;
        Cursor = Cursors.Default;
        _timer.Stop();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackColor);
        EnsureObjectCenter();

        var layout = CalculateLayout();
        DrawStage(graphics, layout);
        DrawObjectForHypothesis(graphics);
        DrawPrompt(graphics);
    }

    private void AdvanceMoment()
    {
        if (!_holding)
        {
            return;
        }

        if (string.Equals(_hypothesis.Id, "D", StringComparison.Ordinal))
        {
            var dx = _targetCenter.X - _objectCenter.X;
            var dy = _targetCenter.Y - _objectCenter.Y;
            _velocity = new PointF(
                (float)((_velocity.X + dx * 0.19F) * 0.76F),
                (float)((_velocity.Y + dy * 0.19F) * 0.76F));
            _objectCenter = new PointF(
                _objectCenter.X + _velocity.X,
                _objectCenter.Y + _velocity.Y);
            return;
        }

        _objectCenter = _targetCenter;
    }

    private void DrawStage(Graphics graphics, StageLayout layout)
    {
        var worldResponds = _holding && string.Equals(_hypothesis.Id, "E", StringComparison.Ordinal);
        var background = worldResponds ? Color.FromArgb(235, 238, 241) : Color.FromArgb(242, 245, 248);
        var border = worldResponds ? Color.FromArgb(142, 153, 164) : Color.FromArgb(120, 134, 150);
        using var stageFill = new SolidBrush(background);
        using var stageBorder = new Pen(border, 1);
        FillRoundedRectangle(graphics, stageFill, layout.Workspace, 8);
        DrawRoundedRectangle(graphics, stageBorder, layout.Workspace, 8);

        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            "Arbeitsflaeche",
            titleFont,
            new Rectangle(layout.Workspace.Left + 22, layout.Workspace.Top + 18, layout.Workspace.Width - 44, 28),
            worldResponds ? Color.FromArgb(56, 65, 76) : Color.FromArgb(26, 35, 48),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            worldResponds ? "alles tritt leiser zurueck" : "halte das Ding kurz",
            SystemFonts.DefaultFont,
            new Rectangle(layout.Workspace.Left + 22, layout.Workspace.Top + 48, layout.Workspace.Width - 44, 24),
            worldResponds ? Color.FromArgb(94, 104, 115) : Color.FromArgb(72, 84, 98),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (worldResponds)
        {
            DrawReceivingSurface(graphics, layout);
        }
    }

    private static void DrawReceivingSurface(Graphics graphics, StageLayout layout)
    {
        var bounds = new Rectangle(
            layout.Workspace.Right - 210,
            layout.Workspace.Bottom - 104,
            160,
            52);
        using var fill = new SolidBrush(Color.FromArgb(238, 245, 240));
        using var border = new Pen(Color.FromArgb(103, 148, 121), 1);
        FillRoundedRectangle(graphics, fill, bounds, 8);
        DrawRoundedRectangle(graphics, border, bounds, 8);
        TextRenderer.DrawText(
            graphics,
            "bereit",
            SystemFonts.DefaultFont,
            bounds,
            Color.FromArgb(55, 107, 78),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawObjectForHypothesis(Graphics graphics)
    {
        var bounds = GetObjectBounds();
        var hypothesisId = _hypothesis.Id;
        var angle = hypothesisId switch
        {
            "A" when _holding => -2.8F,
            "D" when _holding => Math.Clamp(_velocity.X * 0.12F, -4.2F, 4.2F),
            _ => 0F
        };

        DrawObjectCard(graphics, bounds, angle);

        if (!_holding)
        {
            return;
        }

        switch (hypothesisId)
        {
            case "A":
                DrawLiftedCorner(graphics, bounds, angle);
                break;
            case "B":
                DrawDigitalGrip(graphics, bounds);
                break;
            case "C":
                DrawPartialOcclusion(graphics, bounds);
                break;
            case "D":
                DrawStabilizationMark(graphics, bounds);
                break;
        }
    }

    private static void DrawObjectCard(Graphics graphics, RectangleF bounds, float angle)
    {
        var state = graphics.Save();
        graphics.TranslateTransform(bounds.Left + bounds.Width / 2F, bounds.Top + bounds.Height / 2F);
        graphics.RotateTransform(angle);
        var local = new RectangleF(-bounds.Width / 2F, -bounds.Height / 2F, bounds.Width, bounds.Height);
        using var shadow = new SolidBrush(Color.FromArgb(34, 0, 0, 0));
        FillRoundedRectangle(
            graphics,
            shadow,
            new RectangleF(local.Left + 8, local.Top + 10, local.Width, local.Height),
            8);
        using var fill = new SolidBrush(Color.White);
        using var border = new Pen(Color.FromArgb(70, 84, 100), 1.4F);
        FillRoundedRectangle(graphics, fill, local, 8);
        DrawRoundedRectangle(graphics, border, local, 8);

        var icon = new RectangleF(local.Left + 16, local.Top + 18, 50, 50);
        using var iconFill = new SolidBrush(Color.FromArgb(232, 238, 247));
        using var iconBorder = new Pen(Color.FromArgb(111, 128, 148), 1);
        FillRoundedRectangle(graphics, iconFill, icon, 8);
        DrawRoundedRectangle(graphics, iconBorder, icon, 8);
        graphics.Restore(state);

        var textBounds = new Rectangle((int)bounds.Left + 80, (int)bounds.Top + 19, (int)bounds.Width - 96, 54);
        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 11, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            "Projektidee",
            titleFont,
            textBounds,
            Color.FromArgb(27, 36, 48),
            TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            "fuer spaeter",
            SystemFonts.DefaultFont,
            new Rectangle(textBounds.Left, textBounds.Top + 28, textBounds.Width, 22),
            Color.FromArgb(77, 90, 106),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawLiftedCorner(Graphics graphics, RectangleF bounds, float angle)
    {
        var state = graphics.Save();
        graphics.TranslateTransform(bounds.Left + bounds.Width / 2F, bounds.Top + bounds.Height / 2F);
        graphics.RotateTransform(angle);
        using var cornerShadow = new SolidBrush(Color.FromArgb(38, 0, 0, 0));
        var shadow = new PointF[]
        {
            new(-bounds.Width / 2F + 10, -bounds.Height / 2F + 16),
            new(-bounds.Width / 2F + 58, -bounds.Height / 2F + 6),
            new(-bounds.Width / 2F + 28, -bounds.Height / 2F + 48)
        };
        graphics.FillPolygon(cornerShadow, shadow);
        using var cornerFill = new SolidBrush(Color.FromArgb(250, 253, 255));
        using var cornerBorder = new Pen(Color.FromArgb(130, 145, 162), 1);
        var corner = new PointF[]
        {
            new(-bounds.Width / 2F + 8, -bounds.Height / 2F + 8),
            new(-bounds.Width / 2F + 62, -bounds.Height / 2F + 8),
            new(-bounds.Width / 2F + 14, -bounds.Height / 2F + 56)
        };
        graphics.FillPolygon(cornerFill, corner);
        graphics.DrawPolygon(cornerBorder, corner);
        graphics.Restore(state);
    }

    private static void DrawDigitalGrip(Graphics graphics, RectangleF bounds)
    {
        using var gripPen = new Pen(Color.FromArgb(78, 42, 55, 70), 4)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        graphics.DrawArc(gripPen, bounds.Left - 24, bounds.Top + 12, 48, bounds.Height - 24, 100, 160);
        graphics.DrawArc(gripPen, bounds.Right - 24, bounds.Top + 12, 48, bounds.Height - 24, -80, 160);
        using var fingertip = new SolidBrush(Color.FromArgb(34, 42, 55, 70));
        graphics.FillEllipse(fingertip, bounds.Left - 6, bounds.Top + 24, 12, 12);
        graphics.FillEllipse(fingertip, bounds.Right - 6, bounds.Top + 60, 12, 12);
    }

    private static void DrawPartialOcclusion(Graphics graphics, RectangleF bounds)
    {
        using var softCover = new SolidBrush(Color.FromArgb(92, 226, 228, 224));
        using var edge = new Pen(Color.FromArgb(70, 146, 150, 150), 1);
        var left = new RectangleF(bounds.Left - 8, bounds.Top + 18, 54, bounds.Height - 24);
        var right = new RectangleF(bounds.Right - 44, bounds.Top + 12, 56, bounds.Height - 32);
        FillRoundedRectangle(graphics, softCover, left, 22);
        FillRoundedRectangle(graphics, softCover, right, 22);
        DrawRoundedRectangle(graphics, edge, left, 22);
        DrawRoundedRectangle(graphics, edge, right, 22);
    }

    private static void DrawStabilizationMark(Graphics graphics, RectangleF bounds)
    {
        using var pen = new Pen(Color.FromArgb(95, 86, 96, 108), 1.2F);
        var y = bounds.Bottom + 18;
        graphics.DrawLine(pen, bounds.Left + 42, y, bounds.Right - 42, y);
        using var textBrush = new SolidBrush(Color.FromArgb(95, 86, 96, 108));
        using var font = new Font(SystemFonts.DefaultFont.FontFamily, 8);
        graphics.DrawString("stabilisiert sich", font, textBrush, bounds.Left + 78, y + 4);
    }

    private void DrawPrompt(Graphics graphics)
    {
        var title = "Halte das Ding kurz.";
        var detail = _holding
            ? GetHoldingSentence()
            : "Teste genau eine Hypothese. Danach bewertest du nur das Gefuehl.";
        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 13, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            title,
            titleFont,
            new Rectangle(24, 18, Width - 48, 28),
            Color.FromArgb(24, 32, 42),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            detail,
            SystemFonts.DefaultFont,
            new Rectangle(24, 48, Width - 48, 28),
            Color.FromArgb(70, 82, 96),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private string GetHoldingSentence()
    {
        return _hypothesis.Id switch
        {
            "A" => "Es loest sich vom Untergrund.",
            "B" => "Die Hand wird nur angedeutet.",
            "C" => "Ein Teil wird umfasst.",
            "D" => "Es antwortet auf deine Bewegung.",
            "E" => "Die Welt wird leiser.",
            _ => "Der Moment ist aktiv."
        };
    }

    private RectangleF GetObjectBounds()
    {
        EnsureObjectCenter();
        return new RectangleF(
            _objectCenter.X - ObjectSize.Width / 2F,
            _objectCenter.Y - ObjectSize.Height / 2F,
            ObjectSize.Width,
            ObjectSize.Height);
    }

    private void EnsureObjectCenter()
    {
        if (_hasObjectCenter)
        {
            return;
        }

        var layout = CalculateLayout();
        _objectCenter = new PointF(layout.Workspace.Left + 180, layout.Workspace.Top + layout.Workspace.Height / 2F + 20);
        _targetCenter = _objectCenter;
        _hasObjectCenter = true;
    }

    private StageLayout CalculateLayout()
    {
        var margin = 28;
        var top = 88;
        var workspace = new Rectangle(
            margin,
            top,
            Math.Max(520, Width - margin * 2),
            Math.Max(280, Height - top - 32));
        return new StageLayout(workspace);
    }

    private static void FillRoundedRectangle(Graphics graphics, Brush brush, RectangleF bounds, int radius)
    {
        using var path = RoundedRectangle(bounds, radius);
        graphics.FillPath(brush, path);
    }

    private static void DrawRoundedRectangle(Graphics graphics, Pen pen, RectangleF bounds, int radius)
    {
        using var path = RoundedRectangle(bounds, radius);
        graphics.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedRectangle(RectangleF bounds, int radius)
    {
        var diameter = Math.Max(2, radius * 2);
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private sealed record StageLayout(Rectangle Workspace);
}
