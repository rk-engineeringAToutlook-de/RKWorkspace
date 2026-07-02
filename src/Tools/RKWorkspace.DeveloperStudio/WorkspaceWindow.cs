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
            ? "RK Arbeitsflaeche A"
            : "RK Arbeitsflaeche B";
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
        root.Controls.Add(Panel("Transferobjekte", BuildObjectPanel()), 0, 2);
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
        AddInfoRow(details, 1, "Typ", _type);
        AddInfoRow(details, 2, "Position", _position);
        AddInfoRow(details, 3, "Status", _status);
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
            Text = "WORKSPACE\r\nSCREEN",
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
        AddInfoRow(diagnostics, 0, "Core", _diagnostics);
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
                ? Color.FromArgb(246, 248, 251)
                : Color.FromArgb(216, 242, 225);
        _statusHint.ForeColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(20, 94, 58)
            : Color.FromArgb(48, 62, 78);
        BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(224, 244, 234)
            : SystemColors.Control;
        _objectPanel.BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(229, 246, 237)
            : _snapshot.IsSuccessPulseActive
                ? Color.FromArgb(223, 247, 232)
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
                Text = "Leere Arbeitsflaeche: keine Transferobjekte.",
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
        var grabbedHeight = GetGrabbedHeight(lab.GripVariantId);
        var card = new Panel
        {
            Width = Math.Max(460, _objectPanel.ClientSize.Width - (item.IsBeingDragged ? 34 : 42)),
            Height = item.IsBeingDragged ? grabbedHeight : 98,
            BackColor = item.IsBeingDragged ? grabbedBackColor : Color.White,
            BorderStyle = item.IsBeingDragged ? BorderStyle.Fixed3D : BorderStyle.FixedSingle,
            Margin = item.IsBeingDragged
                ? GetGrabbedMargin(lab.GripVariantId, pulse)
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
            Text = item.IsBeingDragged ? $"Gefasst: {item.DisplayName}" : item.DisplayName,
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
                ? $"{StudioUiText.Display(item.ObjectType)} | {GetGripDetail(lab.GripVariantId)} | {item.MimeType}"
                : $"{StudioUiText.Display(item.ObjectType)} | Status: {StudioUiText.Display(item.State)} | {item.MimeType}",
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
            ? $"Dieses Objekt greifen, an den Rand schieben und nach {targetName} fuehren."
            : "Dieses Objekt liegt nicht in dieser Arbeitsflaeche und kann hier nicht gegriffen werden.";
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
        _animationCard.Size = dropVariant switch
        {
            "drop-grow" or "drop-combo" => new Size(214, 36),
            "drop-snap" or "drop-align" => new Size(196, 30),
            _ => new Size(190, 30)
        };
        _animationCard.BackColor = dropVariant switch
        {
            "drop-glow" or "drop-combo" => Color.FromArgb(222, 255, 236),
            "drop-bounce" => Color.FromArgb(255, 249, 222),
            "drop-snap" => Color.FromArgb(226, 238, 255),
            _ => Color.White
        };
    }

    private static string BuildPreviewText(MultiWindowWorkspaceSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.SuggestedWorkspacePreview))
        {
            return string.Empty;
        }

        return snapshot.ExperienceLab.PreviewVariantId switch
        {
            "preview-none" => string.Empty,
            "preview-ghost" => string.Empty,
            "preview-arrow" => snapshot.SuggestedWorkspaceId.EndsWith("-B", StringComparison.OrdinalIgnoreCase)
                ? "->"
                : "<-",
            "preview-miniature" => $"Miniatur\r\n{snapshot.SuggestedWorkspaceName}",
            _ => $"Vorschau\r\n{snapshot.SuggestedWorkspacePreview}"
        };
    }

    private static bool ShouldShowPreview(MultiWindowWorkspaceSnapshot snapshot)
    {
        return !string.IsNullOrWhiteSpace(snapshot.SuggestedWorkspacePreview) &&
            snapshot.ExperienceLab.PreviewVariantId is not "preview-none" and not "preview-ghost";
    }

    private static Size GetPreviewSize(string previewVariantId)
    {
        return previewVariantId switch
        {
            "preview-arrow" => new Size(70, 54),
            "preview-miniature" => new Size(196, 54),
            _ => new Size(248, 54)
        };
    }

    private static Color GetGrabbedBackColor(string gripVariantId, bool pulse)
    {
        return gripVariantId switch
        {
            "grip-normal" => Color.White,
            "grip-float" => Color.FromArgb(240, 248, 255),
            "grip-shrink" => Color.FromArgb(244, 247, 250),
            "grip-inertia" => Color.FromArgb(237, 235, 255),
            "grip-pulse" => pulse ? Color.FromArgb(216, 237, 255) : Color.FromArgb(236, 247, 255),
            "grip-collected" => Color.FromArgb(255, 247, 226),
            "grip-combo" => pulse ? Color.FromArgb(214, 240, 255) : Color.FromArgb(236, 246, 255),
            _ => pulse ? Color.FromArgb(218, 235, 255) : Color.FromArgb(232, 243, 255)
        };
    }

    private static int GetGrabbedHeight(string gripVariantId)
    {
        return gripVariantId switch
        {
            "grip-normal" => 98,
            "grip-shrink" or "grip-collected" => 88,
            "grip-float" => 106,
            "grip-inertia" => 116,
            "grip-combo" => 118,
            _ => 112
        };
    }

    private static Padding GetGrabbedMargin(string gripVariantId, bool pulse)
    {
        return gripVariantId switch
        {
            "grip-normal" => new Padding(8),
            "grip-shrink" => new Padding(18, 12, 14, 12),
            "grip-collected" => new Padding(20, 10, 16, 12),
            "grip-inertia" => new Padding(14, 8, 4, 12),
            "grip-combo" => new Padding(pulse ? 16 : 12, 6, 4, 12),
            _ => new Padding(pulse ? 12 : 10, 7, 5, 11)
        };
    }

    private static string GetGripDetail(string gripVariantId)
    {
        return gripVariantId switch
        {
            "grip-normal" => "normal gegriffen",
            "grip-float" => "schwebt am Cursor",
            "grip-shrink" => "eingezogen",
            "grip-inertia" => "traegt Gewicht",
            "grip-pulse" => "pulsiert",
            "grip-collected" => "eingesammelt",
            "grip-combo" => "genommen und getragen",
            _ => "hebt sich"
        };
    }

    private static Color GetCandidateEdgeColor(string edgeVariantId, bool pulse)
    {
        return edgeVariantId switch
        {
            "edge-glow" => Color.FromArgb(222, 242, 255),
            "edge-runner" => pulse ? Color.FromArgb(203, 231, 255) : Color.FromArgb(231, 243, 255),
            "edge-opening" => Color.FromArgb(232, 245, 255),
            "edge-magnetic" => Color.FromArgb(219, 231, 255),
            "edge-large" => Color.FromArgb(226, 240, 255),
            "edge-combo" => pulse ? Color.FromArgb(199, 230, 255) : Color.FromArgb(224, 241, 255),
            _ => pulse ? Color.FromArgb(213, 232, 255) : Color.FromArgb(230, 241, 255)
        };
    }

    private static Color GetLockedEdgeColor(string edgeVariantId, bool pulse)
    {
        return edgeVariantId switch
        {
            "edge-glow" => Color.FromArgb(203, 246, 224),
            "edge-runner" => pulse ? Color.FromArgb(159, 235, 196) : Color.FromArgb(211, 250, 230),
            "edge-opening" => Color.FromArgb(217, 255, 235),
            "edge-magnetic" => Color.FromArgb(191, 239, 214),
            "edge-large" => Color.FromArgb(205, 247, 225),
            "edge-combo" => pulse ? Color.FromArgb(159, 235, 196) : Color.FromArgb(209, 249, 229),
            _ => pulse ? Color.FromArgb(176, 236, 205) : Color.FromArgb(201, 246, 223)
        };
    }

    private static int GetEdgeWidth(string edgeVariantId)
    {
        return edgeVariantId switch
        {
            "edge-opening" => 132,
            "edge-large" => 156,
            "edge-combo" => 144,
            "edge-invisible" => 1,
            _ => 96
        };
    }

    private static string BuildEdgeLabelText(string hint, string edgeVariantId)
    {
        if (edgeVariantId == "edge-runner")
        {
            return $">>>\r\n{hint.Replace(" ", "\r\n", StringComparison.Ordinal)}";
        }

        if (edgeVariantId == "edge-magnetic")
        {
            return $"MAGNET\r\n{hint.Replace(" ", "\r\n", StringComparison.Ordinal)}";
        }

        return hint.Replace(" ", "\r\n", StringComparison.Ordinal);
    }

    private static bool ShouldShowGhost(string transitionVariantId, string previewVariantId)
    {
        return transitionVariantId is not "transition-vanish" &&
            previewVariantId is not "preview-none";
    }

    private static string BuildGhostText(string objectName, string transitionVariantId)
    {
        return transitionVariantId switch
        {
            "transition-takeover" => $"{objectName}\r\nwird uebernommen",
            "transition-continuous" => $"{objectName}\r\ntraegt weiter",
            "transition-fade" => $"{objectName}\r\nsoft fade",
            "transition-half" => $"{objectName}\r\nhalb im Rand",
            "transition-slide" => $"{objectName}\r\ngleitet",
            "transition-combo" => $"{objectName}\r\nUebergang",
            _ => $"{objectName}\r\nim Rand"
        };
    }

    private static Size GetGhostSize(string transitionVariantId)
    {
        return transitionVariantId switch
        {
            "transition-half" or "transition-continuous" or "transition-combo" => new Size(226, 38),
            "transition-fade" => new Size(176, 30),
            "transition-vanish" => new Size(1, 1),
            _ => new Size(192, 32)
        };
    }

    private static Color GetGhostBackColor(string transitionVariantId, bool locked)
    {
        if (locked)
        {
            return transitionVariantId switch
            {
                "transition-takeover" or "transition-combo" => Color.FromArgb(215, 255, 232),
                "transition-fade" => Color.FromArgb(242, 255, 247),
                _ => Color.FromArgb(232, 255, 241)
            };
        }

        return transitionVariantId switch
        {
            "transition-fade" => Color.FromArgb(255, 253, 244),
            "transition-slide" => Color.FromArgb(238, 247, 255),
            "transition-combo" => Color.FromArgb(255, 247, 222),
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
        return dropVariantId switch
        {
            "drop-bounce" or "drop-combo" => 7 + arc + (ratio > 0.72 ? (int)(Math.Sin(ratio * Math.PI * 4) * 4) : 0),
            "drop-snap" => ratio > 0.85 ? 7 : 7 + arc,
            "drop-soft" => 7 + (int)(Math.Sin(ratio * Math.PI) * -3),
            "drop-grow" => 7 + (int)(Math.Sin(ratio * Math.PI) * -6),
            _ => 7 + arc
        };
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
        panel.Visible = visible && activeEdge && !string.Equals(edgeVariantId, "edge-invisible", StringComparison.Ordinal);
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
            ? "kein Kandidat"
            : $"{candidate.SourceWorkspaceId} -> {candidate.TargetWorkspaceId}";
        var grabbedAt = diagnostics.LastGrabbedAt?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
        var edgeLockedAt = diagnostics.LastEdgeLockedAt?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
        return $"Greifen: {diagnostics.DragStartCount} um {grabbedAt} | Edge-Lock: {edgeLockedAt} | Richtung: {diagnostics.ActiveDirection}\r\n" +
            $"Candidate: {diagnostics.CandidateStatus} | Ziel erkannt: {diagnostics.TargetDetectedCount} | Drop: {diagnostics.DropCount}\r\n" +
            $"Uebergang: {diagnostics.LastTransitionDurationMs} ms | Transfer: {diagnostics.LastTransferDurationMs} ms | Erfolg: {diagnostics.SuccessRatePercent}%\r\n" +
            $"Ruecktransfer: {diagnostics.ReturnTransferCount} | Fehlversuche: {diagnostics.FailedAttempts} | {candidateText}";
    }

    private void ConfigureToolTips()
    {
        _toolTip.AutoPopDelay = 12000;
        _toolTip.InitialDelay = 350;
        _toolTip.ReshowDelay = 150;
        _toolTip.SetToolTip(
            _objectPanel,
            "Transferobjekte in dieser Arbeitsflaeche. Karten greifen und ueber den Rand zur naechsten Arbeitsflaeche schieben.");
        _toolTip.SetToolTip(
            _historyGrid,
            "Core-Verlauf der Objekte, die in dieser Arbeitsflaeche liegen.");
        _toolTip.SetToolTip(
            _logGrid,
            "Lokale Ereignisse dieses Fensters und globale Multi-Window-Ereignisse.");
        _toolTip.SetToolTip(
            _diagnostics,
            "Diagnose des gemeinsamen Core-Kontexts fuer beide Fenster.");
        _toolTip.SetToolTip(
            _statusHint,
            "Statushinweis: zeigt Drag-Zustand, Randvorschlag, Drop-Ziel oder Transferergebnis.");
        _toolTip.SetToolTip(
            _workspacePreview,
            "Workspace Preview: zeigt Zielname, Objektanzahl und Status beim magnetischen Randvorschlag.");
        _toolTip.SetToolTip(
            _leftEdgeZone,
            "Linke Randzone: zeigt, ob ein Objekt nach links hinaus- oder von links hineingeschoben wird.");
        _toolTip.SetToolTip(
            _rightEdgeZone,
            "Rechte Randzone: zeigt, ob ein Objekt nach rechts hinaus- oder von rechts hineingeschoben wird.");
        _toolTip.SetToolTip(
            _transitionGhost,
            "Ghost-Objekt: zeigt den visuellen Uebergang durch den Arbeitsflaechenrand.");
        _toolTip.SetToolTip(
            _uxDiagnostics,
            "Lokale UX-Diagnose fuer Greifen, Edge-Lock, Uebergang, Transferzeit und Erfolgsquote.");
    }
}
