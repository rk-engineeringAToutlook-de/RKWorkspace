using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class WorkspaceWindow : Form
{
    private const string DragFormat = "RKWorkspace.TransferObjectId";
    private const int EdgeThresholdPixels = 44;
    private readonly MultiWindowWorkspaceContext _context;
    private readonly string _workspaceId;
    private readonly Label _name = ValueLabel();
    private readonly Label _type = ValueLabel();
    private readonly Label _position = ValueLabel();
    private readonly Label _status = ValueLabel();
    private readonly Label _diagnostics = ValueLabel();
    private readonly Label _lastResult = ValueLabel();
    private readonly Label _lastError = ValueLabel();
    private readonly FlowLayoutPanel _objectPanel = new();
    private readonly DataGridView _historyGrid = CreateGrid();
    private readonly DataGridView _logGrid = CreateGrid();
    private readonly ToolTip _toolTip = new();
    private readonly Panel _animationLayer = new();
    private readonly Label _statusHint = new();
    private readonly Label _workspacePreview = new();
    private readonly Panel _leftEdgeZone = new();
    private readonly Panel _rightEdgeZone = new();
    private readonly Label _leftEdgeLabel = new();
    private readonly Label _rightEdgeLabel = new();
    private readonly Label _transitionGhost = new();
    private readonly Label _animationCard = new();
    private readonly Label _uxDiagnostics = ValueLabel();
    private readonly System.Windows.Forms.Timer _animationTimer = new();
    private readonly System.Windows.Forms.Timer _pulseTimer = new();
    private MultiWindowWorkspaceSnapshot? _snapshot;
    private int _animationStep;
    private int _pulseStep;

    public WorkspaceWindow(
        MultiWindowWorkspaceContext context,
        string workspaceId)
    {
        _context = context;
        _workspaceId = workspaceId;
        Text = workspaceId.EndsWith("-A", StringComparison.OrdinalIgnoreCase)
            ? "Mein Arbeitsplatz links"
            : "Mein Arbeitsplatz rechts";
        Width = 620;
        Height = 744;
        MinimumSize = new Size(520, 660);
        StartPosition = FormStartPosition.Manual;
        AllowDrop = true;

        _context.Changed += OnContextChanged;
        _animationTimer.Interval = 28;
        _animationTimer.Tick += (_, _) => AdvanceAnimation();
        _pulseTimer.Interval = 120;
        _pulseTimer.Tick += (_, _) =>
        {
            _pulseStep++;
            RefreshFromContext();
        };

        Controls.Add(BuildLayout());
        ConfigureToolTips();
        WireDropTarget(this);
        RefreshFromContext();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Changed -= OnContextChanged;
            _animationTimer.Dispose();
            _pulseTimer.Dispose();
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildAnimationLayer(), 0, 1);
        root.Controls.Add(Panel("Dinge auf dieser Arbeitsflaeche", BuildObjectPanel()), 0, 2);
        root.Controls.Add(BuildHistoryLogPanel(), 0, 3);
        root.Controls.Add(Panel("Diagnose", BuildDiagnostics()), 0, 4);
        root.Controls.Add(Panel("UX-Diagnose", BuildUxDiagnostics()), 0, 5);

        return root;
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(230, 235, 242)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.Controls.Add(BuildMonitorBadge(), 0, 0);

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(8, 4, 4, 4)
        };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddInfoRow(details, 0, "Name", _name);
        AddInfoRow(details, 1, "Art", _type);
        AddInfoRow(details, 2, "Lage", _position);
        AddInfoRow(details, 3, "Zustand", _status);
        header.Controls.Add(details, 1, 0);
        return header;
    }

    private Control BuildMonitorBadge()
    {
        var badge = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 36, 48),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(8)
        };
        var screen = new Label
        {
            Dock = DockStyle.Fill,
            Text = "ARBEITS-\r\nFLAECHE",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(47, 65, 88),
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 9, FontStyle.Bold)
        };
        var stand = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 8,
            BackColor = Color.FromArgb(17, 24, 34)
        };
        badge.Controls.Add(screen);
        badge.Controls.Add(stand);
        return badge;
    }

    private Control BuildUxDiagnostics()
    {
        _uxDiagnostics.Dock = DockStyle.Fill;
        _uxDiagnostics.TextAlign = ContentAlignment.TopLeft;
        _uxDiagnostics.Padding = new Padding(6);
        return _uxDiagnostics;
    }

    private Control BuildAnimationLayer()
    {
        _animationLayer.Dock = DockStyle.Fill;
        _animationLayer.BackColor = Color.FromArgb(246, 248, 251);
        _animationLayer.Visible = true;

        _statusHint.Dock = DockStyle.Fill;
        _statusHint.TextAlign = ContentAlignment.MiddleLeft;
        _statusHint.Padding = new Padding(10, 0, 10, 0);
        _statusHint.AutoEllipsis = true;
        _statusHint.ForeColor = Color.FromArgb(48, 62, 78);
        _animationLayer.Controls.Add(_statusHint);

        ConfigureEdgeZone(_leftEdgeZone, _leftEdgeLabel);
        ConfigureEdgeZone(_rightEdgeZone, _rightEdgeLabel);
        _animationLayer.Controls.Add(_leftEdgeZone);
        _animationLayer.Controls.Add(_rightEdgeZone);

        _transitionGhost.AutoSize = false;
        _transitionGhost.TextAlign = ContentAlignment.MiddleCenter;
        _transitionGhost.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        _transitionGhost.BackColor = Color.FromArgb(255, 252, 240);
        _transitionGhost.ForeColor = Color.FromArgb(68, 54, 20);
        _transitionGhost.BorderStyle = BorderStyle.FixedSingle;
        _transitionGhost.Size = new Size(192, 32);
        _transitionGhost.Visible = false;
        _animationLayer.Controls.Add(_transitionGhost);

        _workspacePreview.AutoSize = false;
        _workspacePreview.TextAlign = ContentAlignment.MiddleLeft;
        _workspacePreview.Padding = new Padding(8, 0, 8, 0);
        _workspacePreview.BackColor = Color.White;
        _workspacePreview.ForeColor = Color.FromArgb(34, 49, 66);
        _workspacePreview.BorderStyle = BorderStyle.FixedSingle;
        _workspacePreview.Size = new Size(248, 54);
        _workspacePreview.Visible = false;
        _animationLayer.Controls.Add(_workspacePreview);
        _animationLayer.Resize += (_, _) => UpdateIllusionLayerLayout();

        _animationCard.AutoSize = false;
        _animationCard.TextAlign = ContentAlignment.MiddleCenter;
        _animationCard.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        _animationCard.BackColor = Color.White;
        _animationCard.BorderStyle = BorderStyle.FixedSingle;
        _animationCard.Size = new Size(190, 30);
        _animationCard.Visible = false;
        _animationLayer.Controls.Add(_animationCard);

        return _animationLayer;
    }

    private static void ConfigureEdgeZone(Panel panel, Label label)
    {
        panel.AutoSize = false;
        panel.Width = 96;
        panel.Visible = false;
        panel.BorderStyle = BorderStyle.FixedSingle;
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8, FontStyle.Bold);
        label.Padding = new Padding(4);
        label.AutoEllipsis = true;
        panel.Controls.Add(label);
    }

    private Control BuildObjectPanel()
    {
        _objectPanel.Dock = DockStyle.Fill;
        _objectPanel.AutoScroll = true;
        _objectPanel.AllowDrop = true;
        _objectPanel.BackColor = Color.FromArgb(245, 247, 250);
        _objectPanel.Padding = new Padding(8);
        WireDropTarget(_objectPanel);
        return _objectPanel;
    }

    private Control BuildHistoryLogPanel()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.Controls.Add(Panel("Verlauf", _historyGrid), 0, 0);
        grid.Controls.Add(Panel("Log", _logGrid), 1, 0);
        return grid;
    }

    private Control BuildDiagnostics()
    {
        var diagnostics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(6)
        };
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddInfoRow(diagnostics, 0, "Zustand", _diagnostics);
        AddInfoRow(diagnostics, 1, "Ergebnis", _lastResult);
        AddInfoRow(diagnostics, 2, "Fehler", _lastError);
        return diagnostics;
    }

    private void RefreshFromContext()
    {
        if (IsDisposed)
        {
            return;
        }

        _snapshot = _context.GetSnapshot(_workspaceId);
        Text = _snapshot.WorkspaceName;
        _name.Text = _snapshot.WorkspaceName;
        _type.Text = StudioUiText.Display(_snapshot.WorkspaceType);
        _position.Text = StudioUiText.Display(_snapshot.WorkspacePosition);
        _status.Text = StudioUiText.Display(_snapshot.WorkspaceStatus);
        _diagnostics.Text = StudioUiText.Display(_snapshot.Diagnostics);
        _lastResult.Text = StudioUiText.Display(_snapshot.LastResult);
        _lastError.Text = StudioUiText.Display(_snapshot.LastError);
        _statusHint.Text = StudioUiText.Display(_snapshot.StatusHint);
        _workspacePreview.Text = BuildPreviewText(_snapshot);
        _workspacePreview.Visible = ShouldShowPreview(_snapshot);
        _workspacePreview.Size = GetPreviewSize(_snapshot.ExperienceLab.PreviewVariantId);
        UpdateIllusionLayer();
        _uxDiagnostics.Text = FormatUxDiagnostics(_snapshot.UxDiagnostics);
        _statusHint.BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(198, 239, 219)
            : string.IsNullOrWhiteSpace(_snapshot.SuccessHint)
                ? _snapshot.IsObjectGrabbed
                    ? Color.FromArgb(238, 241, 246)
                    : Color.FromArgb(246, 248, 251)
                : Color.FromArgb(216, 242, 225);
        _statusHint.ForeColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(20, 94, 58)
            : Color.FromArgb(48, 62, 78);
        BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(224, 244, 234)
            : _snapshot.IsObjectGrabbed
                ? Color.FromArgb(234, 237, 242)
            : SystemColors.Control;
        _objectPanel.BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(229, 246, 237)
            : _snapshot.IsSuccessPulseActive
                ? Color.FromArgb(223, 247, 232)
                : _snapshot.IsObjectGrabbed
                    ? Color.FromArgb(236, 239, 244)
                    : Color.FromArgb(245, 247, 250);
        _historyGrid.DataSource = _snapshot.History.ToArray();
        _logGrid.DataSource = _snapshot.Log.ToArray();

        RebuildObjectCards(_snapshot.TransferObjects);
        ResizeColumns(_historyGrid);
        ResizeColumns(_logGrid);
        ApplyColumnHeaders(_historyGrid);
        ApplyColumnHeaders(_logGrid);
        UpdatePulseTimer();
    }

    private void RebuildObjectCards(IReadOnlyCollection<MultiWindowTransferObjectRow> objects)
    {
        _objectPanel.SuspendLayout();
        _objectPanel.Controls.Clear();
        foreach (var item in objects)
        {
            _objectPanel.Controls.Add(CreateObjectCard(item));
        }

        if (objects.Count == 0)
        {
            _objectPanel.Controls.Add(new Label
            {
                Text = "Hier liegt gerade nichts.",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = Math.Max(420, _objectPanel.ClientSize.Width - 28),
                Height = 52,
                Margin = new Padding(8),
                ForeColor = Color.FromArgb(82, 94, 110)
            });
        }

        _objectPanel.ResumeLayout();
    }

    private Control CreateObjectCard(MultiWindowTransferObjectRow item)
    {
        var canDrag = _context.CanDrag(item.ObjectId, _workspaceId);
        var lab = _snapshot?.ExperienceLab ?? WorkspaceExperienceLabSnapshot.Default;
        var pulse = lab.AnimationEnabled && item.IsBeingDragged && (_pulseStep % 6) < 3;
        var grabbedBackColor = GetGrabbedBackColor(lab.GripVariantId, pulse);
        var grabbedHeight = GetGrabbedHeight(lab.GripVariantId) + GetCarryHeightOffset(lab.CarryVariantId);
        var card = new Panel
        {
            Width = Math.Max(460, _objectPanel.ClientSize.Width - (item.IsBeingDragged ? GetCarryWidthInset(lab.CarryVariantId) : 42)),
            Height = item.IsBeingDragged ? grabbedHeight : 98,
            BackColor = item.IsBeingDragged ? grabbedBackColor : Color.White,
            BorderStyle = item.IsBeingDragged ? BorderStyle.Fixed3D : BorderStyle.FixedSingle,
            Margin = item.IsBeingDragged
                ? GetCarriedMargin(lab.GripVariantId, lab.CarryVariantId, pulse)
                : new Padding(8),
            Tag = item.ObjectId,
            Cursor = canDrag
                ? Cursors.Hand
                : Cursors.Default
        };
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(8)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var symbol = new Label
        {
            Text = item.Symbol,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = item.IsBeingDragged ? Color.FromArgb(44, 102, 174) : Color.FromArgb(236, 241, 247),
            ForeColor = item.IsBeingDragged ? Color.White : Color.FromArgb(36, 52, 70),
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold)
        };
        var title = new Label
        {
            Text = item.IsBeingDragged ? $"Genommen: {item.DisplayName}" : item.DisplayName,
            Dock = DockStyle.Fill,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            AutoEllipsis = true
        };
        var preview = new Label
        {
            Text = item.Preview,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(64, 76, 92),
            AutoEllipsis = true
        };
        var detail = new Label
        {
            Text = item.IsBeingDragged
                ? $"{StudioUiText.Display(item.ObjectType)} | {GetGripDetail(lab.GripVariantId)} | {GetCarryDetail(lab.CarryVariantId)}"
                : $"{StudioUiText.Display(item.ObjectType)} | Zustand: {StudioUiText.Display(item.State)}",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(87, 98, 112),
            AutoEllipsis = true
        };
        grid.Controls.Add(symbol, 0, 0);
        grid.SetRowSpan(symbol, 3);
        grid.Controls.Add(title, 1, 0);
        grid.Controls.Add(preview, 1, 1);
        grid.Controls.Add(detail, 1, 2);
        card.Controls.Add(grid);
        WireDragSource(card, item.ObjectId);
        WireDragSource(grid, item.ObjectId);
        WireDragSource(symbol, item.ObjectId);
        WireDragSource(title, item.ObjectId);
        WireDragSource(preview, item.ObjectId);
        WireDragSource(detail, item.ObjectId);
        var targetName = _workspaceId == _context.SourceWorkspaceId.ToString()
            ? "Arbeitsflaeche rechts"
            : "Arbeitsflaeche links";
        var tooltip = canDrag
            ? $"Dieses Ding nehmen, ruhig tragen und am Durchgang zu {targetName} ablegen."
            : "Dieses Ding liegt nicht hier und kann in dieser Arbeitsflaeche nicht genommen werden.";
        _toolTip.SetToolTip(card, tooltip);
        _toolTip.SetToolTip(grid, tooltip);
        _toolTip.SetToolTip(symbol, tooltip);
        _toolTip.SetToolTip(title, tooltip);
        _toolTip.SetToolTip(preview, tooltip);
        _toolTip.SetToolTip(detail, tooltip);
        return card;
    }

    private void WireDragSource(Control control, string objectId)
    {
        control.MouseDown += (_, args) =>
        {
            if (args.Button != MouseButtons.Left || !_context.CanDrag(objectId, _workspaceId))
            {
                return;
            }

            _context.BeginDrag(objectId, _workspaceId);
            var data = new DataObject();
            data.SetData(DragFormat, objectId);
            var effect = control.DoDragDrop(data, DragDropEffects.Move);
            if (effect == DragDropEffects.None)
            {
                _context.CancelDrag(objectId, _workspaceId);
            }

            _context.SetTargetHighlighted(_context.SourceWorkspaceId.ToString(), highlighted: false);
            _context.SetTargetHighlighted(_context.TargetWorkspaceId.ToString(), highlighted: false);
        };
        control.GiveFeedback += (_, args) =>
        {
            if (!_context.CanDrag(objectId, _workspaceId))
            {
                return;
            }

            var edge = DetectCurrentEdge();
            _context.UpdateEdgeSuggestion(_workspaceId, edge);
            args.UseDefaultCursors = false;
            Cursor.Current = edge == MultiWindowEdge.Right || edge == MultiWindowEdge.Left
                ? Cursors.Hand
                : Cursors.SizeAll;
        };
    }

    private void WireDropTarget(Control control)
    {
        control.DragEnter += (_, args) => HandleDragEnter(args);
        control.DragOver += (_, args) => HandleDragEnter(args);
        control.DragLeave += (_, _) => _context.SetTargetHighlighted(_workspaceId, highlighted: false);
        control.DragDrop += (_, args) => HandleDragDrop(args);
    }

    private void HandleDragEnter(DragEventArgs args)
    {
        var objectId = GetDraggedObjectId(args);
        if (!string.IsNullOrWhiteSpace(objectId) && _context.CanDrop(objectId, _workspaceId))
        {
            args.Effect = DragDropEffects.Move;
            _context.SetTargetHighlighted(_workspaceId, highlighted: true);
            Cursor.Current = Cursors.Hand;
            return;
        }

        args.Effect = DragDropEffects.None;
    }

    private void HandleDragDrop(DragEventArgs args)
    {
        var objectId = GetDraggedObjectId(args);
        if (string.IsNullOrWhiteSpace(objectId))
        {
            return;
        }

        var objectName = _context.GetObjectDisplayName(objectId);
        var success = _context.CompleteDrop(objectId, _workspaceId);
        if (success)
        {
            StartTransferAnimation(objectName);
        }
    }

    private static string GetDraggedObjectId(DragEventArgs args)
    {
        return args.Data is not null && args.Data.GetDataPresent(DragFormat)
            ? args.Data.GetData(DragFormat)?.ToString() ?? string.Empty
            : string.Empty;
    }

    private void StartTransferAnimation(string objectName)
    {
        _animationStep = 0;
        _animationCard.Text = objectName;
        ApplyDropVariantToAnimationCard();
        _animationCard.Left = 8;
        _animationCard.Top = 7;
        _animationLayer.Visible = true;
        _animationCard.Visible = true;
        _animationCard.BringToFront();
        _animationTimer.Stop();
        _animationTimer.Start();
    }

    private void ApplyDropVariantToAnimationCard()
    {
        var dropVariant = _snapshot?.ExperienceLab.DropVariantId ?? WorkspaceExperienceLabSnapshot.Default.DropVariantId;
        _animationCard.Size = GetDropStyle(dropVariant) switch
        {
            4 or 7 => new Size(214, 36),
            3 or 6 => new Size(196, 30),
            _ => new Size(190, 30)
        };
        _animationCard.BackColor = GetDropStyle(dropVariant) switch
        {
            5 or 7 => Color.FromArgb(222, 255, 236),
            2 => Color.FromArgb(255, 249, 222),
            3 => Color.FromArgb(226, 238, 255),
            _ => Color.White
        };
    }

    private static string BuildPreviewText(MultiWindowWorkspaceSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.SuggestedWorkspacePreview))
        {
            return string.Empty;
        }

        var previewStyle = GetPreviewStyle(snapshot.ExperienceLab.PreviewVariantId);
        return previewStyle switch
        {
            0 => string.Empty,
            1 => string.Empty,
            4 => snapshot.SuggestedWorkspaceId.EndsWith("-B", StringComparison.OrdinalIgnoreCase)
                ? "->"
                : "<-",
            3 => $"Ablage\r\n{snapshot.SuggestedWorkspaceName}",
            5 => $"Dorthin\r\n{snapshot.SuggestedWorkspaceName}",
            6 => snapshot.SuggestedWorkspaceId.EndsWith("-B", StringComparison.OrdinalIgnoreCase)
                ? $"-> {snapshot.SuggestedWorkspaceName}"
                : $"<- {snapshot.SuggestedWorkspaceName}",
            7 => $"Durchgang\r\n{snapshot.SuggestedWorkspaceName}",
            _ => $"Weitertragen\r\n{snapshot.SuggestedWorkspacePreview}"
        };
    }

    private static bool ShouldShowPreview(MultiWindowWorkspaceSnapshot snapshot)
    {
        var previewStyle = GetPreviewStyle(snapshot.ExperienceLab.PreviewVariantId);
        return !string.IsNullOrWhiteSpace(snapshot.SuggestedWorkspacePreview) &&
            previewStyle is not 0 and not 1;
    }

    private static Size GetPreviewSize(string previewVariantId)
    {
        return GetPreviewStyle(previewVariantId) switch
        {
            4 => new Size(70, 54),
            3 => new Size(196, 54),
            6 => new Size(208, 54),
            7 => new Size(144, 54),
            _ => new Size(248, 54)
        };
    }

    private static Color GetGrabbedBackColor(string gripVariantId, bool pulse)
    {
        return GetGripStyle(gripVariantId) switch
        {
            0 => Color.White,
            1 => Color.FromArgb(240, 248, 255),
            2 => Color.FromArgb(244, 247, 250),
            4 => Color.FromArgb(237, 235, 255),
            5 => pulse ? Color.FromArgb(216, 237, 255) : Color.FromArgb(236, 247, 255),
            6 => Color.FromArgb(255, 247, 226),
            7 => pulse ? Color.FromArgb(214, 240, 255) : Color.FromArgb(236, 246, 255),
            _ => pulse ? Color.FromArgb(218, 235, 255) : Color.FromArgb(232, 243, 255)
        };
    }

    private static int GetGrabbedHeight(string gripVariantId)
    {
        return GetGripStyle(gripVariantId) switch
        {
            0 => 98,
            2 or 6 => 88,
            1 => 106,
            4 => 116,
            7 => 118,
            _ => 112
        };
    }

    private static Padding GetGrabbedMargin(string gripVariantId, bool pulse)
    {
        return GetGripStyle(gripVariantId) switch
        {
            0 => new Padding(8),
            2 => new Padding(18, 12, 14, 12),
            6 => new Padding(20, 10, 16, 12),
            4 => new Padding(14, 8, 4, 12),
            7 => new Padding(pulse ? 16 : 12, 6, 4, 12),
            _ => new Padding(pulse ? 12 : 10, 7, 5, 11)
        };
    }

    private static Padding GetCarriedMargin(string gripVariantId, string carryVariantId, bool pulse)
    {
        var baseMargin = GetGrabbedMargin(gripVariantId, pulse);
        return GetCarryStyle(carryVariantId) switch
        {
            2 or 4 or 9 => new Padding(baseMargin.Left + 2, baseMargin.Top + 1, Math.Max(2, baseMargin.Right - 4), baseMargin.Bottom + 2),
            3 or 6 => new Padding(baseMargin.Left + (pulse ? 4 : 1), baseMargin.Top, baseMargin.Right + 2, baseMargin.Bottom),
            7 => new Padding(baseMargin.Left + 1, Math.Max(2, baseMargin.Top - 2), baseMargin.Right + 1, baseMargin.Bottom + 3),
            _ => baseMargin
        };
    }

    private static int GetCarryHeightOffset(string carryVariantId)
    {
        return GetCarryStyle(carryVariantId) switch
        {
            2 or 4 or 9 => 6,
            3 or 6 or 11 => 4,
            7 => 8,
            _ => 0
        };
    }

    private static int GetCarryWidthInset(string carryVariantId)
    {
        return GetCarryStyle(carryVariantId) switch
        {
            4 or 9 => 24,
            2 or 7 or 11 => 28,
            _ => 34
        };
    }

    private static string GetGripDetail(string gripVariantId)
    {
        return GetGripStyle(gripVariantId) switch
        {
            0 => "normal gegriffen",
            1 => "schwebt am Cursor",
            2 => "eingezogen",
            4 => "traegt Gewicht",
            5 => "pulsiert",
            6 => "eingesammelt",
            7 => "genommen und getragen",
            _ => "hebt sich"
        };
    }

    private static string GetCarryDetail(string carryVariantId)
    {
        return GetCarryStyle(carryVariantId) switch
        {
            0 => "direkt getragen",
            1 => "leichter Nachlauf",
            2 => "traegt Gewicht",
            3 => "weiche Feder",
            4 => "spuerbare Masse",
            5 => "ruhige Hand",
            6 => "kleine Gegenbewegung",
            7 => "schwebt getragen",
            8 => "magnetisch gehalten",
            9 => "schwer und praezise",
            10 => "leichter Grip",
            _ => "natuerliche Tragphysik"
        };
    }

    private static Color GetCandidateEdgeColor(string edgeVariantId, bool pulse)
    {
        return GetEdgeStyle(edgeVariantId) switch
        {
            0 => Color.FromArgb(222, 242, 255),
            2 => pulse ? Color.FromArgb(203, 231, 255) : Color.FromArgb(231, 243, 255),
            3 => Color.FromArgb(232, 245, 255),
            4 => Color.FromArgb(219, 231, 255),
            6 => Color.FromArgb(226, 240, 255),
            7 => pulse ? Color.FromArgb(199, 230, 255) : Color.FromArgb(224, 241, 255),
            _ => pulse ? Color.FromArgb(213, 232, 255) : Color.FromArgb(230, 241, 255)
        };
    }

    private static Color GetLockedEdgeColor(string edgeVariantId, bool pulse)
    {
        return GetEdgeStyle(edgeVariantId) switch
        {
            0 => Color.FromArgb(203, 246, 224),
            2 => pulse ? Color.FromArgb(159, 235, 196) : Color.FromArgb(211, 250, 230),
            3 => Color.FromArgb(217, 255, 235),
            4 => Color.FromArgb(191, 239, 214),
            6 => Color.FromArgb(205, 247, 225),
            7 => pulse ? Color.FromArgb(159, 235, 196) : Color.FromArgb(209, 249, 229),
            _ => pulse ? Color.FromArgb(176, 236, 205) : Color.FromArgb(201, 246, 223)
        };
    }

    private static int GetEdgeWidth(string edgeVariantId)
    {
        return GetEdgeStyle(edgeVariantId) switch
        {
            3 => 132,
            6 => 156,
            7 => 144,
            5 => 1,
            _ => 96
        };
    }

    private static string BuildEdgeLabelText(string hint, string edgeVariantId)
    {
        var edgeStyle = GetEdgeStyle(edgeVariantId);
        if (edgeStyle == 2)
        {
            return $">>>\r\n{hint.Replace(" ", "\r\n", StringComparison.Ordinal)}";
        }

        if (edgeStyle == 4)
        {
            return $"SOG\r\n{hint.Replace(" ", "\r\n", StringComparison.Ordinal)}";
        }

        return hint.Replace(" ", "\r\n", StringComparison.Ordinal);
    }

    private static bool ShouldShowGhost(string transitionVariantId, string previewVariantId)
    {
        return GetTransitionStyle(transitionVariantId) is not 0 &&
            GetPreviewStyle(previewVariantId) is not 0;
    }

    private static string BuildGhostText(string objectName, string transitionVariantId)
    {
        return GetTransitionStyle(transitionVariantId) switch
        {
            4 => $"{objectName}\r\nwird angenommen",
            5 => $"{objectName}\r\ntraegt weiter",
            6 => $"{objectName}\r\nsoft fade",
            3 => $"{objectName}\r\nhalb im Durchgang",
            1 => $"{objectName}\r\ngleitet",
            7 => $"{objectName}\r\nkontinuierlich",
            _ => $"{objectName}\r\nim Durchgang"
        };
    }

    private static Size GetGhostSize(string transitionVariantId)
    {
        return GetTransitionStyle(transitionVariantId) switch
        {
            3 or 5 or 7 => new Size(226, 38),
            6 => new Size(176, 30),
            0 => new Size(1, 1),
            _ => new Size(192, 32)
        };
    }

    private static Color GetGhostBackColor(string transitionVariantId, bool locked)
    {
        if (locked)
        {
            return GetTransitionStyle(transitionVariantId) switch
            {
                4 or 7 => Color.FromArgb(215, 255, 232),
                6 => Color.FromArgb(242, 255, 247),
                _ => Color.FromArgb(232, 255, 241)
            };
        }

        return GetTransitionStyle(transitionVariantId) switch
        {
            6 => Color.FromArgb(255, 253, 244),
            1 => Color.FromArgb(238, 247, 255),
            7 => Color.FromArgb(255, 247, 222),
            _ => Color.FromArgb(255, 252, 234)
        };
    }

    private static int GetPulseInterval(int speed)
    {
        return Math.Clamp(220 - (speed * 14), 60, 220);
    }

    private static int GetAnimationSteps(int speed)
    {
        return Math.Clamp(28 - (speed * 2), 8, 28);
    }

    private static int GetDropAnimationTop(string dropVariantId, double ratio)
    {
        var arc = (int)(Math.Sin(ratio * Math.PI) * -5);
        return GetDropStyle(dropVariantId) switch
        {
            2 or 7 => 7 + arc + (ratio > 0.72 ? (int)(Math.Sin(ratio * Math.PI * 4) * 4) : 0),
            3 => ratio > 0.85 ? 7 : 7 + arc,
            1 => 7 + (int)(Math.Sin(ratio * Math.PI) * -3),
            4 => 7 + (int)(Math.Sin(ratio * Math.PI) * -6),
            _ => 7 + arc
        };
    }

    private static int GetGripStyle(string variantId)
    {
        return variantId switch
        {
            "grip-normal" => 0,
            "grip-float" => 1,
            "grip-shrink" => 2,
            "grip-lift" => 3,
            "grip-inertia" => 4,
            "grip-pulse" => 5,
            "grip-collected" => 6,
            "grip-combo" => 7,
            _ => GetGenerationStyle(variantId, 8)
        };
    }

    private static int GetEdgeStyle(string variantId)
    {
        return variantId switch
        {
            "edge-glow" => 0,
            "edge-pulse" => 1,
            "edge-runner" => 2,
            "edge-opening" => 3,
            "edge-magnetic" => 4,
            "edge-invisible" => 5,
            "edge-large" => 6,
            "edge-combo" => 7,
            _ => GetGenerationStyle(variantId, 8)
        };
    }

    private static int GetCarryStyle(string variantId)
    {
        return variantId switch
        {
            _ => GetGenerationStyle(variantId, 12)
        };
    }

    private static int GetTransitionStyle(string variantId)
    {
        return variantId switch
        {
            "transition-vanish" => 0,
            "transition-slide" => 1,
            "transition-ghost" => 2,
            "transition-half" => 3,
            "transition-takeover" => 4,
            "transition-continuous" => 5,
            "transition-fade" => 6,
            "transition-combo" => 7,
            _ => GetGenerationStyle(variantId, 8)
        };
    }

    private static int GetDropStyle(string variantId)
    {
        return variantId switch
        {
            "drop-normal" => 0,
            "drop-soft" => 1,
            "drop-bounce" => 2,
            "drop-snap" => 3,
            "drop-grow" => 4,
            "drop-glow" => 5,
            "drop-align" => 6,
            "drop-combo" => 7,
            _ => GetGenerationStyle(variantId, 8)
        };
    }

    private static int GetPreviewStyle(string variantId)
    {
        return variantId switch
        {
            "preview-none" => 0,
            "preview-ghost" => 1,
            "preview-workspace" => 2,
            "preview-miniature" => 3,
            "preview-arrow" => 4,
            _ => GetGenerationStyle(variantId, 8)
        };
    }

    private static int GetGenerationStyle(string variantId, int styleCount)
    {
        return (GetGenerationNumber(variantId) - 1) % styleCount;
    }

    private static int GetGenerationNumber(string variantId)
    {
        var lastDash = variantId.LastIndexOf('-');
        if (lastDash >= 0 &&
            lastDash < variantId.Length - 1 &&
            int.TryParse(variantId[(lastDash + 1)..], out var generation))
        {
            return Math.Max(1, generation);
        }

        return 1;
    }

    private void UpdateIllusionLayer()
    {
        if (_snapshot is null)
        {
            return;
        }

        var lab = _snapshot.ExperienceLab;
        var isActive = _snapshot.IsObjectGrabbed || _snapshot.IsEdgeCandidateActive || _snapshot.IsEdgeLocked;
        var pulse = lab.AnimationEnabled && (_pulseStep % 6) < 3;
        var activeBackColor = _snapshot.IsEdgeLocked
            ? GetLockedEdgeColor(lab.EdgeVariantId, pulse)
            : GetCandidateEdgeColor(lab.EdgeVariantId, pulse);
        var inactiveBackColor = Color.FromArgb(239, 244, 249);
        _leftEdgeZone.Width = GetEdgeWidth(lab.EdgeVariantId);
        _rightEdgeZone.Width = GetEdgeWidth(lab.EdgeVariantId);
        ConfigureEdgeZoneState(
            _leftEdgeZone,
            _leftEdgeLabel,
            _snapshot.ActiveEdge == MultiWindowEdge.Left,
            isActive,
            _snapshot.EdgeHotZoneHint,
            activeBackColor,
            inactiveBackColor,
            lab.EdgeVariantId);
        ConfigureEdgeZoneState(
            _rightEdgeZone,
            _rightEdgeLabel,
            _snapshot.ActiveEdge == MultiWindowEdge.Right,
            isActive,
            _snapshot.EdgeHotZoneHint,
            activeBackColor,
            inactiveBackColor,
            lab.EdgeVariantId);

        _transitionGhost.Text = string.IsNullOrWhiteSpace(_snapshot.EdgeGhostObjectName)
            ? string.Empty
            : BuildGhostText(_snapshot.EdgeGhostObjectName, lab.TransitionVariantId);
        _transitionGhost.Visible = !string.IsNullOrWhiteSpace(_snapshot.EdgeGhostObjectName) &&
            _snapshot.ActiveEdge != MultiWindowEdge.None &&
            ShouldShowGhost(lab.TransitionVariantId, lab.PreviewVariantId);
        _transitionGhost.Size = GetGhostSize(lab.TransitionVariantId);
        _transitionGhost.BackColor = GetGhostBackColor(lab.TransitionVariantId, _snapshot.IsEdgeLocked);
        _transitionGhost.ForeColor = _snapshot.IsEdgeLocked
            ? Color.FromArgb(21, 91, 56)
            : Color.FromArgb(94, 71, 18);
        UpdateIllusionLayerLayout();
    }

    private static void ConfigureEdgeZoneState(
        Panel panel,
        Label label,
        bool activeEdge,
        bool visible,
        string hint,
        Color activeBackColor,
        Color inactiveBackColor,
        string edgeVariantId)
    {
        panel.Visible = visible && activeEdge && GetEdgeStyle(edgeVariantId) != 5;
        panel.BackColor = activeEdge ? activeBackColor : inactiveBackColor;
        label.BackColor = panel.BackColor;
        label.ForeColor = activeEdge
            ? Color.FromArgb(20, 75, 118)
            : Color.FromArgb(74, 90, 108);
        label.Text = string.IsNullOrWhiteSpace(hint)
            ? string.Empty
            : BuildEdgeLabelText(hint, edgeVariantId);
    }

    private void UpdateIllusionLayerLayout()
    {
        _workspacePreview.Left = Math.Max(8, _animationLayer.ClientSize.Width - _workspacePreview.Width - 10);
        _workspacePreview.Top = 9;
        var height = Math.Max(1, _animationLayer.ClientSize.Height);
        _leftEdgeZone.Bounds = new Rectangle(0, 0, _leftEdgeZone.Width, height);
        _rightEdgeZone.Bounds = new Rectangle(
            Math.Max(0, _animationLayer.ClientSize.Width - _rightEdgeZone.Width),
            0,
            _rightEdgeZone.Width,
            height);

        if (_snapshot is null)
        {
            return;
        }

        _transitionGhost.Top = Math.Max(8, (height - _transitionGhost.Height) / 2);
        _transitionGhost.Left = _snapshot.ActiveEdge switch
        {
            MultiWindowEdge.Left => -(_transitionGhost.Width / 2),
            MultiWindowEdge.Right => Math.Max(0, _animationLayer.ClientSize.Width - (_transitionGhost.Width / 2)),
            _ => 8
        };
        _transitionGhost.BringToFront();
        _workspacePreview.BringToFront();
        _animationCard.BringToFront();
    }

    private void UpdatePulseTimer()
    {
        var shouldPulse = _snapshot is not null &&
            _snapshot.ExperienceLab.AnimationEnabled &&
            (_snapshot.IsObjectGrabbed ||
                _snapshot.IsEdgeCandidateActive ||
                _snapshot.IsEdgeLocked ||
                _snapshot.IsSuccessPulseActive);
        if (_snapshot is not null)
        {
            _pulseTimer.Interval = GetPulseInterval(_snapshot.ExperienceLab.Speed);
        }

        if (shouldPulse && !_pulseTimer.Enabled)
        {
            _pulseTimer.Start();
        }
        else if (!shouldPulse && _pulseTimer.Enabled)
        {
            _pulseTimer.Stop();
            _pulseStep = 0;
        }
    }

    private void AdvanceAnimation()
    {
        _animationStep++;
        var maxLeft = Math.Max(8, _animationLayer.ClientSize.Width - _animationCard.Width - 8);
        var lab = _snapshot?.ExperienceLab ?? WorkspaceExperienceLabSnapshot.Default;
        var totalSteps = GetAnimationSteps(lab.Speed);
        var ratio = Math.Min(1.0, _animationStep / (double)totalSteps);
        _animationCard.Left = 8 + (int)((maxLeft - 8) * ratio);
        _animationCard.Top = GetDropAnimationTop(lab.DropVariantId, ratio);
        if (_animationStep >= totalSteps)
        {
            _animationTimer.Stop();
            _animationCard.Visible = false;
        }
    }

    private MultiWindowEdge DetectCurrentEdge()
    {
        return _context.DetectWindowEdge(
            Cursor.Position.X,
            Bounds.Left,
            Bounds.Right,
            EdgeThresholdPixels);
    }

    private void OnContextChanged(object? sender, EventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(RefreshFromContext);
            return;
        }

        RefreshFromContext();
    }

    private static void AddInfoRow(TableLayoutPanel panel, int row, string label, Label value)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        }, 0, row);
        panel.Controls.Add(value, 1, row);
    }

    private static Control Panel(string title, Control content)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(6)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        }, 0, 0);
        panel.Controls.Add(content, 0, 1);
        return panel;
    }

    private static Label ValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        grid.CellFormatting += (_, args) =>
        {
            if (args.Value is string value)
            {
                args.Value = StudioUiText.Display(value);
                args.FormattingApplied = true;
            }
        };

        return grid;
    }

    private static void ResizeColumns(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.MinimumWidth = 70;
        }
    }

    private static void ApplyColumnHeaders(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.HeaderText = StudioUiText.Header(column.DataPropertyName);
        }
    }

    private static string FormatUxDiagnostics(MultiWindowUxDiagnosticsSnapshot diagnostics)
    {
        var candidate = diagnostics.SessionCandidate;
        var candidateText = string.IsNullOrWhiteSpace(candidate.TargetWorkspaceId)
            ? "kein Durchgang aktiv"
            : $"{StudioUiText.Display(candidate.SourceWorkspaceId)} -> {StudioUiText.Display(candidate.TargetWorkspaceId)}";
        var grabbedAt = diagnostics.LastGrabbedAt?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
        var edgeLockedAt = diagnostics.LastEdgeLockedAt?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
        return $"Greifen: {diagnostics.DragStartCount} um {grabbedAt} | Durchgang: {edgeLockedAt} | Richtung: {StudioUiText.Display(diagnostics.ActiveDirection)}\r\n" +
            $"Weitertragen: {StudioUiText.Display(diagnostics.CandidateStatus.ToString())} | Durchgang erkannt: {diagnostics.TargetDetectedCount} | Ablegen: {diagnostics.DropCount}\r\n" +
            $"Kontinuitaet: {diagnostics.LastTransitionDurationMs} ms | Absetzen: {diagnostics.LastTransferDurationMs} ms | Erfolg: {diagnostics.SuccessRatePercent}%\r\n" +
            $"Zurueckgetragen: {diagnostics.ReturnTransferCount} | Fehlversuche: {diagnostics.FailedAttempts} | {candidateText}";
    }

    private void ConfigureToolTips()
    {
        _toolTip.AutoPopDelay = 12000;
        _toolTip.InitialDelay = 350;
        _toolTip.ReshowDelay = 150;
        _toolTip.SetToolTip(
            _objectPanel,
            "Dinge auf dieser Arbeitsflaeche. Karte nehmen, tragen und an anderer Stelle ablegen.");
        _toolTip.SetToolTip(
            _historyGrid,
            "Core-Verlauf der Objekte, die in dieser Arbeitsflaeche liegen.");
        _toolTip.SetToolTip(
            _logGrid,
            "Lokale Ereignisse dieses Fensters und globale Multi-Window-Ereignisse.");
        _toolTip.SetToolTip(
            _diagnostics,
            "Lokaler Zustand der beiden Arbeitsflaechen.");
        _toolTip.SetToolTip(
            _statusHint,
            "Statushinweis: zeigt Greifen, Tragen, Durchgang und Ablegen.");
        _toolTip.SetToolTip(
            _workspacePreview,
            "Vorschau der Arbeitsflaeche, auf der das Ding abgelegt werden kann.");
        _toolTip.SetToolTip(
            _leftEdgeZone,
            "Linker Durchgang: hier kann das Objekt nach links weitergetragen werden.");
        _toolTip.SetToolTip(
            _rightEdgeZone,
            "Rechter Durchgang: hier kann das Objekt nach rechts weitergetragen werden.");
        _toolTip.SetToolTip(
            _transitionGhost,
            "Kontinuitaet: zeigt, dass das Objekt nicht verschwindet, sondern durch den Rand getragen wird.");
        _toolTip.SetToolTip(
            _uxDiagnostics,
            "Lokaler Gefuehlstest fuer Greifen, Tragen, Durchgang und Ablegen.");
    }
}
