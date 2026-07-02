using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class WorkspaceWindow : Form
{
    private const string DragFormat = "RKWorkspace.TransferObjectId";
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
    private readonly Label _animationCard = new();
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
            RowCount = 5,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 43));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildAnimationLayer(), 0, 1);
        root.Controls.Add(Panel("Transferobjekte", BuildObjectPanel()), 0, 2);
        root.Controls.Add(BuildHistoryLogPanel(), 0, 3);
        root.Controls.Add(Panel("Diagnose", BuildDiagnostics()), 0, 4);

        return root;
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(8)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddInfoRow(header, 0, "Name", _name);
        AddInfoRow(header, 1, "Typ", _type);
        AddInfoRow(header, 2, "Position", _position);
        AddInfoRow(header, 3, "Status", _status);
        return header;
    }

    private Control BuildAnimationLayer()
    {
        _animationLayer.Dock = DockStyle.Fill;
        _animationLayer.BackColor = Color.FromArgb(246, 248, 251);
        _animationLayer.Visible = false;

        _animationCard.AutoSize = false;
        _animationCard.TextAlign = ContentAlignment.MiddleCenter;
        _animationCard.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        _animationCard.BackColor = Color.White;
        _animationCard.BorderStyle = BorderStyle.FixedSingle;
        _animationCard.Size = new Size(190, 30);
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
        BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(224, 244, 234)
            : SystemColors.Control;
        _objectPanel.BackColor = _snapshot.IsDropTargetHighlighted
            ? Color.FromArgb(229, 246, 237)
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
                Text = "Keine Transferobjekte in dieser Arbeitsflaeche.",
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
        var card = new Panel
        {
            Width = Math.Max(460, _objectPanel.ClientSize.Width - 42),
            Height = 78,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(8),
            Tag = item.ObjectId,
            Cursor = _context.CanDrag(item.ObjectId, _workspaceId)
                ? Cursors.Hand
                : Cursors.Default
        };
        var title = new Label
        {
            Text = $"{StudioUiText.Display(item.ObjectType)}: {item.DisplayName}",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Padding = new Padding(8, 6, 8, 0),
            AutoEllipsis = true
        };
        var detail = new Label
        {
            Text = $"Status: {StudioUiText.Display(item.State)} | MIME: {item.MimeType}",
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 0, 8, 4),
            AutoEllipsis = true
        };
        card.Controls.Add(detail);
        card.Controls.Add(title);
        WireDragSource(card, item.ObjectId);
        WireDragSource(title, item.ObjectId);
        WireDragSource(detail, item.ObjectId);
        var tooltip = _context.CanDrag(item.ObjectId, _workspaceId)
            ? "Dieses Objekt kann in Window B gezogen werden. Beim Loslassen wird der Core-Transfer ausgefuehrt."
            : "Dieses Objekt liegt in dieser Arbeitsflaeche. Bereits uebertragene Objekte sind hier nur sichtbar.";
        _toolTip.SetToolTip(card, tooltip);
        _toolTip.SetToolTip(title, tooltip);
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
            DoDragDrop(data, DragDropEffects.Move);
            _context.SetTargetHighlighted(_context.TargetWorkspaceId.ToString(), highlighted: false);
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
        _animationCard.BringToFront();
        _animationTimer.Stop();
        _animationTimer.Start();
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
            _animationLayer.Visible = false;
        }
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
    }
}
