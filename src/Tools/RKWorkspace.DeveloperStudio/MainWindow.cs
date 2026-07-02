using RKWorkspace.DeveloperStudio.ViewModels;

namespace RKWorkspace.DeveloperStudio;

internal sealed class MainWindow : Form
{
    private readonly StudioViewModel _viewModel = new();
    private readonly DataGridView _workspaceGrid = CreateGrid();
    private readonly DataGridView _transferGrid = CreateGrid();
    private readonly DataGridView _agentGrid = CreateGrid();
    private readonly DataGridView _logGrid = CreateGrid();
    private readonly Label _runtimeState = ValueLabel();
    private readonly Label _pluginCount = ValueLabel();
    private readonly Label _workspaceCount = ValueLabel();
    private readonly Label _transferObjectCount = ValueLabel();
    private readonly Label _capabilities = ValueLabel();
    private readonly Label _lastResult = ValueLabel();
    private readonly Label _lastError = ValueLabel();

    public MainWindow()
    {
        Text = "RK Workspace Developer Studio";
        MinimumSize = new Size(1120, 720);
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(BuildLayout());
        RefreshUi();
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(BuildMainGrid(), 0, 1);
        root.Controls.Add(BuildLogPanel(), 0, 2);

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

        panel.Controls.Add(Button("Start Runtime", _viewModel.StartRuntime));
        panel.Controls.Add(Button("Add Demo Workspaces", _viewModel.AddDemoWorkspaces));
        panel.Controls.Add(Button("Create Text Object", _viewModel.CreateTextObject));
        panel.Controls.Add(Button("Transfer Right", _viewModel.TransferRight));
        panel.Controls.Add(Button("Start Dual Agents", _viewModel.StartDualAgents));
        panel.Controls.Add(Button("Stop Dual Agents", _viewModel.StopDualAgents));
        panel.Controls.Add(Button("Reset", _viewModel.Reset));
        panel.Controls.Add(Button("Run Full Demo", _viewModel.RunFullDemo));

        return panel;
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

        grid.Controls.Add(Panel("Workspaces", _workspaceGrid), 0, 0);
        grid.Controls.Add(Panel("Transfer Objects", _transferGrid), 1, 0);
        grid.Controls.Add(Panel("Agents", _agentGrid), 2, 0);
        grid.Controls.Add(Panel("Diagnostics", BuildDiagnostics()), 3, 0);

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

        AddDiagnosticRow(diagnostics, 0, "Runtime State", _runtimeState);
        AddDiagnosticRow(diagnostics, 1, "Plugin Count", _pluginCount);
        AddDiagnosticRow(diagnostics, 2, "Workspace Count", _workspaceCount);
        AddDiagnosticRow(diagnostics, 3, "Transfer Objects", _transferObjectCount);
        AddDiagnosticRow(diagnostics, 4, "Capabilities", _capabilities);
        AddDiagnosticRow(diagnostics, 5, "Last Result", _lastResult);
        AddDiagnosticRow(diagnostics, 6, "Last Error", _lastError);

        return diagnostics;
    }

    private Control BuildLogPanel()
    {
        return Panel("Log", _logGrid);
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

    private Button Button(string text, Func<bool> action)
    {
        var button = new Button
        {
            Text = text,
            Width = 150,
            Height = 32,
            Margin = new Padding(4, 6, 4, 4)
        };
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

        var diagnostics = _viewModel.Diagnostics;
        _runtimeState.Text = diagnostics.RuntimeState.ToString();
        _pluginCount.Text = diagnostics.PluginCount.ToString();
        _workspaceCount.Text = diagnostics.WorkspaceCount.ToString();
        _transferObjectCount.Text = diagnostics.TransferObjectCount.ToString();
        _capabilities.Text = diagnostics.Capabilities;
        _lastResult.Text = diagnostics.LastResult;
        _lastError.Text = diagnostics.LastError;

        ResizeColumns(_workspaceGrid);
        ResizeColumns(_transferGrid);
        ResizeColumns(_agentGrid);
        ResizeColumns(_logGrid);
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
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
}
