using System.Drawing.Drawing2D;
using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class FirstContactSurface : Control
{
    private static readonly Size CardSize = new(238, 96);
    private readonly System.Windows.Forms.Timer _carryTimer = new();
    private readonly FirstContactSession _session = new();
    private WorkspaceExperienceLabSnapshot _experienceLab = WorkspaceExperienceLabSnapshot.Default;
    private bool _dragging;
    private Point _dragOffset;
    private Point _dragLocation;
    private Point _targetDragLocation;
    private PointF _carryVelocity;

    public FirstContactSurface()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(247, 248, 249);
        MinimumSize = new Size(740, 360);
        Cursor = Cursors.Default;
        _session.Changed += OnSessionChanged;
        _carryTimer.Interval = 16;
        _carryTimer.Tick += (_, _) =>
        {
            if (!_dragging)
            {
                _carryTimer.Stop();
                return;
            }

            AdvanceCarryPhysics();
            UpdateTargetState();
            Invalidate();
        };
    }

    public event EventHandler? SessionChanged;

    public FirstContactSession Session => _session;

    public void ResetFirstContact()
    {
        _dragging = false;
        _carryVelocity = PointF.Empty;
        Capture = false;
        Cursor = Cursors.Default;
        _carryTimer.Stop();
        _session.Reset();
        Invalidate();
    }

    public void SetExperienceLab(WorkspaceExperienceLabSnapshot snapshot)
    {
        _experienceLab = snapshot;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _session.Changed -= OnSessionChanged;
            _carryTimer.Dispose();
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

        var snapshot = _session.GetSnapshot();
        var objectBounds = GetObjectBounds(CalculateLayout(), snapshot);
        if (snapshot.IsCompleted || !objectBounds.Contains(e.Location))
        {
            _session.RecordUnnecessaryClick();
            return;
        }

        if (!_session.RecordGrip())
        {
            return;
        }

        _dragging = true;
        _dragLocation = objectBounds.Location;
        _targetDragLocation = objectBounds.Location;
        _carryVelocity = PointF.Empty;
        _dragOffset = new Point(e.X - objectBounds.Left, e.Y - objectBounds.Top);
        Capture = true;
        Cursor = Cursors.Hand;
        _carryTimer.Start();
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
        {
            _targetDragLocation = new Point(
                e.X - _dragOffset.X,
                e.Y - _dragOffset.Y);
            AdvanceCarryPhysics();
            UpdateTargetState();
            Invalidate();
            return;
        }

        var snapshot = _session.GetSnapshot();
        Cursor = !snapshot.IsCompleted && GetObjectBounds(CalculateLayout(), snapshot).Contains(e.Location)
            ? Cursors.Hand
            : Cursors.Default;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_dragging)
        {
            Cursor = Cursors.Default;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_dragging)
        {
            return;
        }

        Capture = false;
        _carryTimer.Stop();
        _dragLocation = _targetDragLocation;
        var overTarget = IsOverTarget();
        _dragging = false;
        Cursor = Cursors.Default;
        _session.RecordPlace(overTarget);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(BackColor);

        var snapshot = _session.GetSnapshot();
        var layout = CalculateLayout();
        DrawHint(graphics, snapshot);
        DrawWorkspace(
            graphics,
            layout.SourceWorkspace,
            "Mein Arbeitsplatz",
            snapshot.IsGrabbed ? "ruhig" : "bereit",
            highlighted: false,
            quiet: snapshot.IsGrabbed);
        DrawWorkspace(
            graphics,
            layout.TargetWorkspace,
            "Weiterarbeiten",
            snapshot.IsCompleted ? "liegt hier" : "frei",
            highlighted: snapshot.IsOverTarget || snapshot.IsCompleted,
            quiet: snapshot.IsGrabbed && !snapshot.IsOverTarget);
        DrawPassage(graphics, layout, snapshot);
        DrawObject(graphics, GetObjectBounds(layout, snapshot), snapshot);
        DrawCompletionMetrics(graphics, layout, snapshot);
    }

    private void OnSessionChanged(object? sender, EventArgs args)
    {
        Invalidate();
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    private SurfaceLayout CalculateLayout()
    {
        var bounds = ClientRectangle;
        var margin = 28;
        var gap = 32;
        var top = 64;
        var availableWidth = Math.Max(660, bounds.Width - margin * 2);
        var workspaceWidth = Math.Max(300, (availableWidth - gap) / 2);
        var workspaceHeight = Math.Max(240, bounds.Height - top - 38);
        var left = margin;
        var source = new Rectangle(left, top, workspaceWidth, workspaceHeight);
        var target = new Rectangle(left + workspaceWidth + gap, top, workspaceWidth, workspaceHeight);
        var passage = new Rectangle(source.Right + 4, top + 14, Math.Max(12, gap - 8), workspaceHeight - 28);
        return new SurfaceLayout(source, target, passage);
    }

    private Rectangle GetObjectBounds(SurfaceLayout layout, FirstContactSnapshot snapshot)
    {
        if (_dragging)
        {
            return new Rectangle(_dragLocation, GetActiveCardSize());
        }

        var workspace = string.Equals(snapshot.ObjectLocation, "Right", StringComparison.Ordinal)
            ? layout.TargetWorkspace
            : layout.SourceWorkspace;

        return new Rectangle(
            workspace.Left + 42,
            workspace.Top + 112,
            CardSize.Width,
            CardSize.Height);
    }

    private void AdvanceCarryPhysics()
    {
        var style = GetCarryStyle(_experienceLab.CarryVariantId);
        var speedFactor = 0.72 + Math.Clamp(_experienceLab.Speed, 1, 10) * 0.065;
        var response = GetCarryResponse(style) * speedFactor;
        var damping = GetCarryDamping(style);
        var dx = _targetDragLocation.X - _dragLocation.X;
        var dy = _targetDragLocation.Y - _dragLocation.Y;
        _carryVelocity = new PointF(
            (float)((_carryVelocity.X + dx * response) * damping),
            (float)((_carryVelocity.Y + dy * response) * damping));

        if (Math.Abs(dx) < 1 &&
            Math.Abs(dy) < 1 &&
            Math.Abs(_carryVelocity.X) < 0.5F &&
            Math.Abs(_carryVelocity.Y) < 0.5F)
        {
            _dragLocation = _targetDragLocation;
            _carryVelocity = PointF.Empty;
            return;
        }

        _dragLocation = new Point(
            _dragLocation.X + (int)Math.Round(_carryVelocity.X),
            _dragLocation.Y + (int)Math.Round(_carryVelocity.Y));
    }

    private void UpdateTargetState()
    {
        if (!_dragging)
        {
            return;
        }

        _session.RecordHoverTarget(IsOverTarget());
    }

    private bool IsOverTarget()
    {
        var layout = CalculateLayout();
        var cardBounds = new Rectangle(_dragLocation, GetActiveCardSize());
        var cardCenter = new Point(
            cardBounds.Left + cardBounds.Width / 2,
            cardBounds.Top + cardBounds.Height / 2);
        return layout.TargetWorkspace.Contains(cardCenter);
    }

    private static void DrawHint(Graphics graphics, FirstContactSnapshot snapshot)
    {
        var hintBounds = new Rectangle(28, 16, 520, 34);
        using var font = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            snapshot.Hint,
            font,
            hintBounds,
            Color.FromArgb(24, 32, 42),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawWorkspace(
        Graphics graphics,
        Rectangle bounds,
        string title,
        string state,
        bool highlighted,
        bool quiet)
    {
        var fillColor = highlighted
            ? Color.FromArgb(232, 246, 238)
            : quiet
                ? Color.FromArgb(237, 239, 242)
                : Color.FromArgb(242, 245, 248);
        var borderColor = highlighted
            ? Color.FromArgb(37, 140, 86)
            : Color.FromArgb(127, 140, 156);

        using var fill = new SolidBrush(fillColor);
        using var border = new Pen(borderColor, highlighted ? 3 : 1);
        FillRoundedRectangle(graphics, fill, bounds, 8);
        DrawRoundedRectangle(graphics, border, bounds, 8);

        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
        var titleBounds = new Rectangle(bounds.Left + 22, bounds.Top + 20, bounds.Width - 44, 30);
        var stateBounds = new Rectangle(bounds.Left + 22, bounds.Top + 52, bounds.Width - 44, 28);
        TextRenderer.DrawText(
            graphics,
            title,
            titleFont,
            titleBounds,
            quiet && !highlighted ? Color.FromArgb(70, 78, 90) : Color.FromArgb(27, 36, 48),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            state,
            SystemFonts.DefaultFont,
            stateBounds,
            quiet && !highlighted ? Color.FromArgb(105, 113, 124) : Color.FromArgb(70, 82, 96),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (highlighted)
        {
            var dropBounds = new Rectangle(bounds.Left + 22, bounds.Bottom - 58, bounds.Width - 44, 34);
            using var dropFont = new Font(SystemFonts.DefaultFont.FontFamily, 11, FontStyle.Bold);
            TextRenderer.DrawText(
                graphics,
                "Hier ablegen",
                dropFont,
                dropBounds,
                Color.FromArgb(28, 118, 76),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    private static void DrawPassage(Graphics graphics, SurfaceLayout layout, FirstContactSnapshot snapshot)
    {
        if (!snapshot.IsGrabbed && !snapshot.IsCompleted)
        {
            return;
        }

        var alpha = snapshot.IsOverTarget || snapshot.IsCompleted ? 118 : 58;
        using var fill = new SolidBrush(Color.FromArgb(alpha, 88, 154, 118));
        FillRoundedRectangle(graphics, fill, layout.Passage, 8);

        if (snapshot.IsGrabbed)
        {
            using var line = new Pen(Color.FromArgb(120, 88, 154, 118), 2);
            graphics.DrawLine(
                line,
                layout.Passage.Left + layout.Passage.Width / 2,
                layout.Passage.Top + 18,
                layout.Passage.Left + layout.Passage.Width / 2,
                layout.Passage.Bottom - 18);
        }
    }

    private void DrawObject(Graphics graphics, Rectangle bounds, FirstContactSnapshot snapshot)
    {
        var active = snapshot.IsGrabbed || _dragging;
        if (active)
        {
            var offset = GetCarryShadowOffset(GetCarryStyle(_experienceLab.CarryVariantId));
            using var activeShadow = new SolidBrush(Color.FromArgb(58, 0, 0, 0));
            FillRoundedRectangle(
                graphics,
                activeShadow,
                new Rectangle(bounds.Left + offset.Width, bounds.Top + offset.Height, bounds.Width, bounds.Height),
                8);
        }
        else
        {
            using var shadow = new SolidBrush(Color.FromArgb(26, 0, 0, 0));
            FillRoundedRectangle(
                graphics,
                shadow,
                new Rectangle(bounds.Left + 5, bounds.Top + 7, bounds.Width, bounds.Height),
                8);
        }

        using var fill = new SolidBrush(active ? GetActiveObjectColor() : Color.White);
        using var border = new Pen(active ? Color.FromArgb(22, 95, 168) : Color.FromArgb(76, 88, 104), active ? 3 : 1);
        FillRoundedRectangle(graphics, fill, bounds, 8);
        DrawRoundedRectangle(graphics, border, bounds, 8);

        var iconBounds = new Rectangle(bounds.Left + 14, bounds.Top + 16, 48, 48);
        using var iconFill = new SolidBrush(Color.FromArgb(232, 238, 247));
        using var iconBorder = new Pen(Color.FromArgb(116, 131, 150), 1);
        FillRoundedRectangle(graphics, iconFill, iconBounds, 8);
        DrawRoundedRectangle(graphics, iconBorder, iconBounds, 8);
        using var iconFont = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            "RK",
            iconFont,
            iconBounds,
            Color.FromArgb(40, 53, 70),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 11, FontStyle.Bold);
        var titleBounds = new Rectangle(bounds.Left + 76, bounds.Top + 15, bounds.Width - 92, 26);
        var detailBounds = new Rectangle(bounds.Left + 76, bounds.Top + 42, bounds.Width - 92, 24);
        var statusBounds = new Rectangle(bounds.Left + 14, bounds.Bottom - 28, bounds.Width - 28, 20);
        TextRenderer.DrawText(
            graphics,
            active ? "Gegriffen: Projektidee" : "Projektidee",
            titleFont,
            titleBounds,
            Color.FromArgb(25, 34, 46),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            "Entwurf fuer morgen",
            SystemFonts.DefaultFont,
            detailBounds,
            Color.FromArgb(55, 67, 82),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            active ? GetCarrySentence() : "bereit zum Nehmen",
            SystemFonts.DefaultFont,
            statusBounds,
            Color.FromArgb(89, 101, 116),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawCompletionMetrics(Graphics graphics, SurfaceLayout layout, FirstContactSnapshot snapshot)
    {
        if (!snapshot.IsCompleted)
        {
            return;
        }

        var bounds = new Rectangle(
            layout.TargetWorkspace.Left + 22,
            layout.TargetWorkspace.Bottom - 96,
            layout.TargetWorkspace.Width - 44,
            46);
        using var fill = new SolidBrush(Color.FromArgb(235, 248, 240));
        using var border = new Pen(Color.FromArgb(58, 147, 95), 1);
        FillRoundedRectangle(graphics, fill, bounds, 8);
        DrawRoundedRectangle(graphics, border, bounds, 8);

        var result = snapshot.IsSuccessWithinThirtySeconds
            ? "Erstkontakt: unter 30 Sekunden"
            : "Erstkontakt: abgeschlossen";
        using var resultFont = new Font(SystemFonts.DefaultFont.FontFamily, 9, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            result,
            resultFont,
            new Rectangle(bounds.Left + 12, bounds.Top + 4, bounds.Width - 24, 18),
            Color.FromArgb(28, 104, 67),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            snapshot.ResultSummary,
            SystemFonts.DefaultFont,
            new Rectangle(bounds.Left + 12, bounds.Top + 22, bounds.Width - 24, 18),
            Color.FromArgb(54, 72, 88),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private Size GetActiveCardSize()
    {
        return GetGripStyle(_experienceLab.GripVariantId) switch
        {
            2 or 6 => new Size(228, 90),
            4 or 7 => new Size(248, 102),
            _ => CardSize
        };
    }

    private Color GetActiveObjectColor()
    {
        return GetGripStyle(_experienceLab.GripVariantId) switch
        {
            1 => Color.FromArgb(246, 251, 255),
            2 => Color.FromArgb(248, 250, 252),
            4 => Color.FromArgb(243, 241, 255),
            6 => Color.FromArgb(255, 249, 235),
            _ => Color.White
        };
    }

    private string GetCarrySentence()
    {
        return GetCarryStyle(_experienceLab.CarryVariantId) switch
        {
            0 => "liegt in der Hand",
            1 => "folgt mit Nachlauf",
            2 => "hat leichtes Gewicht",
            3 => "federt sanft",
            4 => "traegt Masse",
            5 => "bleibt ruhig",
            6 => "pendelt leise",
            7 => "schwebt getragen",
            8 => "haelt magnetisch",
            9 => "schwer und praezise",
            10 => "hat Grip",
            _ => "getragen, nicht gezogen"
        };
    }

    private static int GetGripStyle(string variantId)
    {
        return GetGenerationStyle(variantId, 8);
    }

    private static int GetCarryStyle(string variantId)
    {
        return GetGenerationStyle(variantId, 12);
    }

    private static int GetGenerationStyle(string variantId, int styleCount)
    {
        var lastDash = variantId.LastIndexOf('-');
        if (lastDash >= 0 &&
            lastDash < variantId.Length - 1 &&
            int.TryParse(variantId[(lastDash + 1)..], out var generation))
        {
            return (Math.Max(1, generation) - 1) % styleCount;
        }

        return 0;
    }

    private static double GetCarryResponse(int style)
    {
        return style switch
        {
            0 => 0.58,
            1 => 0.34,
            2 => 0.24,
            3 => 0.30,
            4 => 0.18,
            5 => 0.28,
            6 => 0.36,
            7 => 0.26,
            8 => 0.40,
            9 => 0.16,
            10 => 0.46,
            _ => 0.24
        };
    }

    private static double GetCarryDamping(int style)
    {
        return style switch
        {
            0 => 0.62,
            1 => 0.72,
            2 => 0.80,
            3 => 0.76,
            4 => 0.84,
            5 => 0.70,
            6 => 0.78,
            7 => 0.74,
            8 => 0.68,
            9 => 0.86,
            10 => 0.66,
            _ => 0.78
        };
    }

    private static Size GetCarryShadowOffset(int style)
    {
        return style switch
        {
            2 or 4 or 9 => new Size(10, 12),
            3 or 6 => new Size(8, 10),
            7 => new Size(6, 11),
            _ => new Size(7, 8)
        };
    }

    private static void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        using var path = RoundedRectangle(bounds, radius);
        graphics.FillPath(brush, path);
    }

    private static void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        using var path = RoundedRectangle(bounds, radius);
        graphics.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
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

    private sealed record SurfaceLayout(Rectangle SourceWorkspace, Rectangle TargetWorkspace, Rectangle Passage);
}
