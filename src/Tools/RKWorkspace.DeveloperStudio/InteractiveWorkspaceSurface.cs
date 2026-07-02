using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class InteractiveWorkspaceSurface : Control
{
    private const string TargetLocation = "Workspace B";
    private static readonly Size CardSize = new(220, 84);
    private InteractiveWorkspaceSnapshot _snapshot = EmptySnapshot();
    private bool _dragging;
    private bool _dragOverTarget;
    private Point _dragOffset;
    private Point _dragLocation;

    public InteractiveWorkspaceSurface()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(246, 248, 251);
        MinimumSize = new Size(680, 250);
        Cursor = Cursors.Default;
    }

    public Func<bool>? DragStarted { get; set; }

    public Func<bool, bool>? TargetHighlightChanged { get; set; }

    public Func<bool, bool>? ObjectDropped { get; set; }

    public void SetSnapshot(InteractiveWorkspaceSnapshot snapshot)
    {
        _snapshot = snapshot;
        if (!snapshot.IsDragging)
        {
            _dragging = false;
            _dragOverTarget = false;
        }

        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || !_snapshot.IsInitialized)
        {
            return;
        }

        var layout = CalculateLayout();
        var objectBounds = GetObjectBounds(layout);
        if (!objectBounds.Contains(e.Location) ||
            string.Equals(_snapshot.ObjectLocation, TargetLocation, StringComparison.Ordinal))
        {
            return;
        }

        if (DragStarted?.Invoke() == false)
        {
            return;
        }

        _dragging = true;
        _dragOverTarget = false;
        _dragLocation = objectBounds.Location;
        _dragOffset = new Point(e.X - objectBounds.Left, e.Y - objectBounds.Top);
        Capture = true;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging)
        {
            return;
        }

        var layout = CalculateLayout();
        _dragLocation = new Point(
            e.X - _dragOffset.X,
            e.Y - _dragOffset.Y);

        var cardBounds = new Rectangle(_dragLocation, CardSize);
        var cardCenter = new Point(
            cardBounds.Left + cardBounds.Width / 2,
            cardBounds.Top + cardBounds.Height / 2);
        var overTarget = layout.TargetWorkspace.Contains(cardCenter);
        if (overTarget != _dragOverTarget)
        {
            _dragOverTarget = overTarget;
            TargetHighlightChanged?.Invoke(overTarget);
        }

        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_dragging)
        {
            return;
        }

        Capture = false;
        _dragging = false;
        var overTarget = _dragOverTarget;
        _dragOverTarget = false;
        ObjectDropped?.Invoke(overTarget);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.Clear(BackColor);

        var layout = CalculateLayout();
        DrawWorkspace(
            graphics,
            layout.SourceWorkspace,
            _snapshot.SourceName,
            _snapshot.SourcePosition,
            _snapshot.SourceState,
            highlighted: false);
        DrawWorkspace(
            graphics,
            layout.TargetWorkspace,
            _snapshot.TargetName,
            _snapshot.TargetPosition,
            _snapshot.TargetState,
            highlighted: _snapshot.IsTargetHighlighted || _dragOverTarget);
        DrawObject(graphics, GetObjectBounds(layout), _dragging || _snapshot.IsDragging);
    }

    private SurfaceLayout CalculateLayout()
    {
        var bounds = ClientRectangle;
        var top = 18;
        var gap = 18;
        var sidePadding = 18;
        var availableWidth = Math.Max(620, bounds.Width - sidePadding * 2);
        var workspaceWidth = Math.Max(290, (availableWidth - gap) / 2);
        var workspaceHeight = Math.Max(205, bounds.Height - 36);
        var left = sidePadding;
        var source = new Rectangle(left, top, workspaceWidth, workspaceHeight);
        var target = new Rectangle(left + workspaceWidth + gap, top, workspaceWidth, workspaceHeight);

        return new SurfaceLayout(source, target);
    }

    private Rectangle GetObjectBounds(SurfaceLayout layout)
    {
        if (_dragging)
        {
            return new Rectangle(_dragLocation, CardSize);
        }

        var workspace = string.Equals(_snapshot.ObjectLocation, TargetLocation, StringComparison.Ordinal)
            ? layout.TargetWorkspace
            : layout.SourceWorkspace;

        return new Rectangle(
            workspace.Left + 28,
            workspace.Top + 104,
            CardSize.Width,
            CardSize.Height);
    }

    private static void DrawWorkspace(
        Graphics graphics,
        Rectangle bounds,
        string title,
        string position,
        string state,
        bool highlighted)
    {
        using var fill = new SolidBrush(highlighted
            ? Color.FromArgb(229, 246, 237)
            : Color.FromArgb(239, 243, 248));
        using var border = new Pen(highlighted
            ? Color.FromArgb(32, 142, 88)
            : Color.FromArgb(96, 113, 133), highlighted ? 3 : 1);
        graphics.FillRectangle(fill, bounds);
        graphics.DrawRectangle(border, bounds);

        var titleBounds = new Rectangle(bounds.Left + 18, bounds.Top + 18, bounds.Width - 36, 28);
        var detailBounds = new Rectangle(bounds.Left + 18, bounds.Top + 52, bounds.Width - 36, 58);
        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            title,
            titleFont,
            titleBounds,
            Color.FromArgb(28, 36, 48),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            $"Position: {StudioUiText.Display(position)}\r\nStatus: {StudioUiText.Display(state)}",
            SystemFonts.DefaultFont,
            detailBounds,
            Color.FromArgb(52, 65, 82),
            TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
    }

    private void DrawObject(Graphics graphics, Rectangle bounds, bool active)
    {
        if (!_snapshot.IsInitialized)
        {
            return;
        }

        if (active)
        {
            using var shadow = new SolidBrush(Color.FromArgb(55, 0, 0, 0));
            graphics.FillRectangle(shadow, new Rectangle(bounds.Left + 6, bounds.Top + 7, bounds.Width, bounds.Height));
        }

        using var fill = new SolidBrush(Color.White);
        using var border = new Pen(active
            ? Color.FromArgb(17, 94, 168)
            : Color.FromArgb(68, 80, 96), active ? 3 : 1);
        graphics.FillRectangle(fill, bounds);
        graphics.DrawRectangle(border, bounds);

        var titleBounds = new Rectangle(bounds.Left + 14, bounds.Top + 10, bounds.Width - 28, 24);
        var textBounds = new Rectangle(bounds.Left + 14, bounds.Top + 38, bounds.Width - 28, 22);
        var stateBounds = new Rectangle(bounds.Left + 14, bounds.Top + 61, bounds.Width - 28, 18);
        using var titleFont = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            StudioUiText.Display(_snapshot.ObjectTitle),
            titleFont,
            titleBounds,
            Color.FromArgb(23, 31, 42),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            $"\"{_snapshot.ObjectText}\"",
            SystemFonts.DefaultFont,
            textBounds,
            Color.FromArgb(43, 54, 68),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            graphics,
            $"Status: {StudioUiText.Display(_snapshot.ObjectState)}",
            SystemFonts.DefaultFont,
            stateBounds,
            Color.FromArgb(88, 101, 116),
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static InteractiveWorkspaceSnapshot EmptySnapshot()
    {
        return new InteractiveWorkspaceSnapshot
        {
            IsInitialized = false,
            SourceName = "Arbeitsflaeche A / Laptop",
            SourcePosition = "Center",
            SourceState = "Missing",
            TargetName = "Arbeitsflaeche B / Anzeige rechts",
            TargetPosition = "Right",
            TargetState = "Missing",
            ObjectTitle = "Text Object",
            ObjectText = "Hallo von RK Workspace",
            ObjectState = "Missing",
            ObjectLocation = "Workspace A",
            IsDragging = false,
            IsTargetHighlighted = false
        };
    }

    private sealed record SurfaceLayout(Rectangle SourceWorkspace, Rectangle TargetWorkspace);
}
