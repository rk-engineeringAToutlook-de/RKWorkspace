using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class MainWindow : Form
{
    private readonly StudioViewModel _viewModel = new();
    private readonly DataGridView _workspaceGrid = CreateGrid();
    private readonly DataGridView _transferGrid = CreateGrid();
    private readonly DataGridView _agentGrid = CreateGrid();
    private readonly DataGridView _logGrid = CreateGrid();
    private readonly DataGridView _historyGrid = CreateGrid();
    private readonly InteractiveWorkspaceSurface _interactiveSurface = new();
    private readonly List<WorkspaceWindow> _workspaceWindows = new();
    private readonly ToolTip _toolTip = new();
    private readonly Label _runtimeState = ValueLabel();
    private readonly Label _pluginCount = ValueLabel();
    private readonly Label _workspaceCount = ValueLabel();
    private readonly Label _transferObjectCount = ValueLabel();
    private readonly Label _capabilities = ValueLabel();
    private readonly Label _lastResult = ValueLabel();
    private readonly Label _lastError = ValueLabel();
    private MultiWindowWorkspaceContext? _multiWindowContext;

    public MainWindow()
    {
        Text = "RK Workspace Entwickler-Studio";
        MinimumSize = new Size(1120, 720);
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        _interactiveSurface.DragStarted = () =>
        {
            var success = _viewModel.BeginInteractiveDrag();
            RefreshUi();
            return success;
        };
        _interactiveSurface.TargetHighlightChanged = highlighted =>
        {
            var success = _viewModel.SetInteractiveTargetHighlighted(highlighted);
            RefreshUi();
            return success;
        };
        _interactiveSurface.ObjectDropped = overTarget =>
        {
            var success = overTarget
                ? _viewModel.CompleteInteractiveDropOnTarget()
                : _viewModel.CancelInteractiveDrag();
            RefreshUi();
            return success;
        };

        Controls.Add(BuildLayout());
        ConfigureToolTips();
        RefreshUi();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
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
            RowCount = 4,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(BuildInteractivePanel(), 0, 1);
        root.Controls.Add(BuildMainGrid(), 0, 2);
        root.Controls.Add(BuildLogPanel(), 0, 3);

        return root;
    }

    private Control BuildToolbar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        panel.Controls.Add(Button(
            "Runtime starten",
            _viewModel.StartRuntime,
            "Startet die zentrale Core Runtime Engine und bereitet die Manager vor."));
        panel.Controls.Add(Button(
            "Demo-Arbeitsflaechen",
            _viewModel.AddDemoWorkspaces,
            "Legt Workspace A und Workspace B fuer den lokalen Demo-Transfer an."));
        panel.Controls.Add(Button(
            "Textobjekt erstellen",
            _viewModel.CreateTextObject,
            "Erzeugt ein logisches Text-Transferobjekt im Core."));
        panel.Controls.Add(Button(
            "Nach rechts uebertragen",
            _viewModel.TransferRight,
            "Fuehrt den Transfer von Workspace A nach Workspace B ueber die Transfer Engine aus."));
        panel.Controls.Add(Button(
            "Zwei Agenten starten",
            _viewModel.StartDualAgents,
            "Startet zwei lokale Agent-Runtimes fuer Diagnose und Vergleich."));
        panel.Controls.Add(Button(
            "Zwei Agenten stoppen",
            _viewModel.StopDualAgents,
            "Stoppt die beiden lokalen Agent-Runtimes sauber."));
        panel.Controls.Add(Button(
            "Zuruecksetzen",
            _viewModel.Reset,
            "Setzt Runtime, Objekte, Agenten, Log und Diagnoseansicht zurueck."));
        panel.Controls.Add(Button(
            "Voll-Demo ausfuehren",
            _viewModel.RunFullDemo,
            "Fuehrt Runtime-Start, Workspaces, Textobjekt, Transfer und Agentenstart in einem Ablauf aus."));
        panel.Controls.Add(Button(
            "Interaktive Demo zuruecksetzen",
            _viewModel.ResetInteractiveDemo,
            "Initialisiert den Drag-and-Drop-Prototyp neu."));
        panel.Controls.Add(Button(
            "Interaktive Demo ausfuehren",
            _viewModel.RunFullInteractiveDemo,
            "Simuliert Drag, Zielmarkierung und Drop ueber den Core-Pfad."));
        panel.Controls.Add(Button(
            "Multi-Window-Prototyp oeffnen",
            OpenMultiWindowPrototype,
            "Oeffnet zwei echte Fenster fuer Window A und Window B mit gemeinsamem Core-Kontext."));

        return panel;
    }

    private bool OpenMultiWindowPrototype()
    {
        _workspaceWindows.RemoveAll(window => window.IsDisposed);
        if (_workspaceWindows.Count == 2)
        {
            foreach (var window in _workspaceWindows)
            {
                window.Show();
                window.WindowState = FormWindowState.Normal;
                window.BringToFront();
            }

            return true;
        }

        foreach (var window in _workspaceWindows.ToArray())
        {
            window.Close();
        }

        _workspaceWindows.Clear();
        _multiWindowContext = new MultiWindowWorkspaceContext();
        var baseLocation = PointToScreen(new Point(20, 120));
        var workspaceA = new WorkspaceWindow(
            _multiWindowContext,
            _multiWindowContext.SourceWorkspaceId.ToString())
        {
            Location = baseLocation
        };
        var workspaceB = new WorkspaceWindow(
            _multiWindowContext,
            _multiWindowContext.TargetWorkspaceId.ToString())
        {
            Location = new Point(baseLocation.X + workspaceA.Width + 24, baseLocation.Y)
        };
        workspaceA.FormClosed += (_, _) => _workspaceWindows.Remove(workspaceA);
        workspaceB.FormClosed += (_, _) => _workspaceWindows.Remove(workspaceB);
        _workspaceWindows.Add(workspaceA);
        _workspaceWindows.Add(workspaceB);
        workspaceA.Show(this);
        workspaceB.Show(this);
        return true;
    }

    private Control BuildInteractivePanel()
    {
        return Panel("Interaktiver Workspace-Prototyp", _interactiveSurface);
    }

    private Control BuildMainGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));

        grid.Controls.Add(Panel("Arbeitsflaechen", _workspaceGrid), 0, 0);
        grid.Controls.Add(Panel("Transferobjekte", _transferGrid), 1, 0);
        grid.Controls.Add(Panel("Agenten", _agentGrid), 2, 0);
        grid.Controls.Add(Panel("Diagnose", BuildDiagnostics()), 3, 0);

        return grid;
    }

    private Control BuildDiagnostics()
    {
        var diagnostics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(4)
        };
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddDiagnosticRow(diagnostics, 0, "Runtime-Status", _runtimeState);
        AddDiagnosticRow(diagnostics, 1, "Plugins", _pluginCount);
        AddDiagnosticRow(diagnostics, 2, "Arbeitsflaechen", _workspaceCount);
        AddDiagnosticRow(diagnostics, 3, "Transferobjekte", _transferObjectCount);
        AddDiagnosticRow(diagnostics, 4, "Faehigkeiten", _capabilities);
        AddDiagnosticRow(diagnostics, 5, "Letztes Ergebnis", _lastResult);
        AddDiagnosticRow(diagnostics, 6, "Letzter Fehler", _lastError);

        return diagnostics;
    }

    private Control BuildLogPanel()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        grid.Controls.Add(Panel("Log", _logGrid), 0, 0);
        grid.Controls.Add(Panel("Verlauf", _historyGrid), 1, 0);

        return grid;
    }

    private static void AddDiagnosticRow(TableLayoutPanel panel, int row, string label, Label value)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, row is 4 or 6 ? 76 : 36));
        panel.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        }, 0, row);
        panel.Controls.Add(value, 1, row);
    }

    private Button Button(string text, Func<bool> action, string tooltip)
    {
        var preferredWidth = Math.Max(150, TextRenderer.MeasureText(text, SystemFonts.DefaultFont).Width + 28);
        var button = new Button
        {
            Text = text,
            Width = preferredWidth,
            Height = 32,
            Margin = new Padding(4, 6, 4, 4)
        };
        _toolTip.SetToolTip(button, tooltip);
        button.Click += (_, _) =>
        {
            action();
            RefreshUi();
        };

        return button;
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
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
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

    private void RefreshUi()
    {
        _workspaceGrid.DataSource = _viewModel.Workspaces.ToArray();
        _transferGrid.DataSource = _viewModel.TransferObjects.ToArray();
        _agentGrid.DataSource = _viewModel.Agents.ToArray();
        _logGrid.DataSource = _viewModel.LogEntries.ToArray();
        _historyGrid.DataSource = _viewModel.TransferHistory.ToArray();
        _interactiveSurface.SetSnapshot(_viewModel.InteractiveWorkspace);

        var diagnostics = _viewModel.Diagnostics;
        _runtimeState.Text = StudioUiText.Display(diagnostics.RuntimeState.ToString());
        _pluginCount.Text = diagnostics.PluginCount.ToString();
        _workspaceCount.Text = diagnostics.WorkspaceCount.ToString();
        _transferObjectCount.Text = diagnostics.TransferObjectCount.ToString();
        _capabilities.Text = StudioUiText.Display(diagnostics.Capabilities);
        _lastResult.Text = StudioUiText.Display(diagnostics.LastResult);
        _lastError.Text = StudioUiText.Display(diagnostics.LastError);

        ResizeColumns(_workspaceGrid);
        ResizeColumns(_transferGrid);
        ResizeColumns(_agentGrid);
        ResizeColumns(_logGrid);
        ResizeColumns(_historyGrid);
        ApplyColumnHeaders(_workspaceGrid);
        ApplyColumnHeaders(_transferGrid);
        ApplyColumnHeaders(_agentGrid);
        ApplyColumnHeaders(_logGrid);
        ApplyColumnHeaders(_historyGrid);
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

    private static Label ValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private static void ResizeColumns(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.MinimumWidth = 80;
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
            _interactiveSurface,
            "Ein sichtbarer Bedienprototyp: Textkarte mit der Maus von links nach rechts ziehen und loslassen.");
        _toolTip.SetToolTip(
            _workspaceGrid,
            "Zeigt die im Core registrierten Arbeitsflaechen mit Typ, Position und Vertrauensstatus.");
        _toolTip.SetToolTip(
            _transferGrid,
            "Zeigt logische Transferobjekte aus dem Core, keine echten Dateien.");
        _toolTip.SetToolTip(
            _agentGrid,
            "Zeigt lokale Agent-Runtimes, wenn die Dual-Agent-Demo gestartet wurde.");
        _toolTip.SetToolTip(
            _logGrid,
            "Zeigt die zuletzt ausgefuehrten Studio-Aktionen und ihre Ergebnisse.");
        _toolTip.SetToolTip(
            _historyGrid,
            "Zeigt den Core-Verlauf des aktuellen Transferobjekts.");
        _toolTip.SetToolTip(
            _capabilities,
            "Faehigkeiten sind die vom Core erkannten Moeglichkeiten der aktuellen Arbeitsflaechen.");
    }
}
