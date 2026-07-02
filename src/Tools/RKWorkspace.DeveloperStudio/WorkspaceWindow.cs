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
    private readonly Label _animationCard = new();
    private readonly Label _uxDiagnostics = ValueLabel();
    private readonly System.Windows.Forms.Timer _animationTimer = new();
    private MultiWindowWorkspaceSnapshot? _snapshot;
    private int _animationStep;

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
        Height = 720;
        MinimumSize = new Size(520, 620);
        StartPosition = FormStartPosition.Manual;
        AllowDrop = true;

        _context.Changed += OnContextChanged;
        _animationTimer.Interval = 28;
        _animationTimer.Tick += (_, _) => AdvanceAnimation();

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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
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

        _workspacePreview.AutoSize = false;
        _workspacePreview.TextAlign = ContentAlignment.MiddleLeft;
        _workspacePreview.Padding = new Padding(8, 0, 8, 0);
        _workspacePreview.BackColor = Color.White;
        _workspacePreview.ForeColor = Color.FromArgb(34, 49, 66);
        _workspacePreview.BorderStyle = BorderStyle.FixedSingle;
        _workspacePreview.Size = new Size(248, 54);
        _workspacePreview.Visible = false;
        _animationLayer.Controls.Add(_workspacePreview);
        _animationLayer.Resize += (_, _) => UpdatePreviewCardLayout();

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
        _workspacePreview.Text = string.IsNullOrWhiteSpace(_snapshot.SuggestedWorkspacePreview)
            ? string.Empty
            : $"Vorschau\r\n{_snapshot.SuggestedWorkspacePreview}";
        _workspacePreview.Visible = !string.IsNullOrWhiteSpace(_snapshot.SuggestedWorkspacePreview);
        UpdatePreviewCardLayout();
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
        var card = new Panel
        {
            Width = Math.Max(460, _objectPanel.ClientSize.Width - 42),
            Height = item.IsBeingDragged ? 104 : 98,
            BackColor = item.IsBeingDragged ? Color.FromArgb(226, 238, 255) : Color.White,
            BorderStyle = item.IsBeingDragged ? BorderStyle.Fixed3D : BorderStyle.FixedSingle,
            Margin = item.IsBeingDragged ? new Padding(10, 8, 6, 10) : new Padding(8),
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
            Text = item.DisplayName,
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
            Text = $"{StudioUiText.Display(item.ObjectType)} | Status: {StudioUiText.Display(item.State)} | {item.MimeType}",
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
            ? "Arbeitsflaeche B"
            : "Arbeitsflaeche A";
        var tooltip = canDrag
            ? $"Dieses Objekt kann nach {targetName} gezogen werden. Beim Loslassen wird der Core-Transfer ausgefuehrt."
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
            Cursor.Current = edge == MultiWindowEdge.Right
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
        _animationCard.Left = 8;
        _animationCard.Top = 7;
        _animationLayer.Visible = true;
        _animationCard.Visible = true;
        _animationCard.BringToFront();
        _animationTimer.Stop();
        _animationTimer.Start();
    }

    private void UpdatePreviewCardLayout()
    {
        _workspacePreview.Left = Math.Max(8, _animationLayer.ClientSize.Width - _workspacePreview.Width - 10);
        _workspacePreview.Top = 9;
    }

    private void AdvanceAnimation()
    {
        _animationStep++;
        var maxLeft = Math.Max(8, _animationLayer.ClientSize.Width - _animationCard.Width - 8);
        var ratio = Math.Min(1.0, _animationStep / 18.0);
        _animationCard.Left = 8 + (int)((maxLeft - 8) * ratio);
        _animationCard.Top = 7 + (int)(Math.Sin(ratio * Math.PI) * -5);
        if (_animationStep >= 18)
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
        return $"Drag Start: {diagnostics.DragStartCount} | Ziel erkannt: {diagnostics.TargetDetectedCount} | Drop: {diagnostics.DropCount}\r\n" +
            $"Drag Dauer: {diagnostics.LastDragDurationMs} ms | Transferzeit: {diagnostics.LastTransferDurationMs} ms | Erfolgsquote: {diagnostics.SuccessRatePercent}%\r\n" +
            $"Erfolgreich: {diagnostics.SuccessfulTransfers} | Fehler: {diagnostics.FailedTransfers} | SessionCandidate: {candidateText}";
    }

    private void ConfigureToolTips()
    {
        _toolTip.AutoPopDelay = 12000;
        _toolTip.InitialDelay = 350;
        _toolTip.ReshowDelay = 150;
        _toolTip.SetToolTip(
            _objectPanel,
            "Transferobjekte in dieser Arbeitsflaeche. In Window A koennen Karten nach Window B gezogen werden.");
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
            _uxDiagnostics,
            "Lokale UX-Diagnose fuer Drag-Start, Zielerkennung, Drop, Transferzeit und Erfolgsquote.");
    }
}
