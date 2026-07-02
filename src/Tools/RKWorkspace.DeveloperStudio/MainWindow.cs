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
    private readonly WorkspaceExperienceLabState _experienceLab = WorkspaceExperienceLabState.Load();
    private readonly List<WorkspaceWindow> _workspaceWindows = new();
    private readonly ToolTip _toolTip = new();
    private readonly Dictionary<string, ComboBox> _labVariantCombos = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Label> _labDescriptionLabels = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RadioButton[]> _labRatingButtons = new(StringComparer.Ordinal);
    private readonly Label _runtimeState = ValueLabel();
    private readonly Label _pluginCount = ValueLabel();
    private readonly Label _workspaceCount = ValueLabel();
    private readonly Label _transferObjectCount = ValueLabel();
    private readonly Label _capabilities = ValueLabel();
    private readonly Label _lastResult = ValueLabel();
    private readonly Label _lastError = ValueLabel();
    private readonly Label _labSummary = ValueLabel();
    private readonly Label _labSpeedLabel = ValueLabel();
    private readonly CheckBox _labAnimationEnabled = new();
    private readonly TrackBar _labSpeed = new();
    private MultiWindowWorkspaceContext? _multiWindowContext;
    private bool _syncingLabControls;

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
        _experienceLab.Changed += OnExperienceLabChanged;
        ConfigureToolTips();
        RefreshUi();
        RefreshLabControls();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _experienceLab.Changed -= OnExperienceLabChanged;
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildLayout()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };
        var studioPage = new TabPage("Developer Studio");
        studioPage.Controls.Add(BuildDeveloperStudioLayout());
        var labPage = new TabPage("Workspace Experience Lab");
        labPage.Controls.Add(BuildExperienceLab());
        tabs.TabPages.Add(studioPage);
        tabs.TabPages.Add(labPage);
        return tabs;
    }

    private Control BuildDeveloperStudioLayout()
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

    private Control BuildExperienceLab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));

        var variants = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            AutoScroll = true
        };
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        variants.Controls.Add(BuildLabVariantGroup("grip", "Greifen", WorkspaceExperienceLabState.GripVariants), 0, 0);
        variants.Controls.Add(BuildLabVariantGroup("edge", "Rand", WorkspaceExperienceLabState.EdgeVariants), 0, 1);
        variants.Controls.Add(BuildLabVariantGroup("transition", "Uebergang", WorkspaceExperienceLabState.TransitionVariants), 0, 2);
        variants.Controls.Add(BuildLabVariantGroup("drop", "Ablegen", WorkspaceExperienceLabState.DropVariants), 0, 3);
        variants.Controls.Add(BuildLabVariantGroup("preview", "Preview", WorkspaceExperienceLabState.PreviewVariants), 0, 4);

        root.Controls.Add(variants, 0, 0);
        root.Controls.Add(BuildLabDashboard(), 1, 0);
        return root;
    }

    private Control BuildLabVariantGroup(
        string category,
        string title,
        IReadOnlyList<WorkspaceExperienceLabOption> options)
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = title,
            Padding = new Padding(10)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(WorkspaceExperienceLabOption.DisplayName),
            ValueMember = nameof(WorkspaceExperienceLabOption.Id),
            DataSource = options.ToArray()
        };
        combo.SelectedIndexChanged += (_, _) =>
        {
            if (_syncingLabControls || combo.SelectedItem is not WorkspaceExperienceLabOption option)
            {
                return;
            }

            _experienceLab.SetVariant(category, option.Id);
        };

        var description = ValueLabel();
        description.Padding = new Padding(0, 4, 0, 4);

        var ratingPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var like = RatingButton(category, WorkspaceExperienceLabRating.Like, "Gefaellt mir");
        var neutral = RatingButton(category, WorkspaceExperienceLabRating.Neutral, "Neutral");
        var dislike = RatingButton(category, WorkspaceExperienceLabRating.Dislike, "Gefaellt mir nicht");
        ratingPanel.Controls.Add(like);
        ratingPanel.Controls.Add(neutral);
        ratingPanel.Controls.Add(dislike);

        _labVariantCombos[category] = combo;
        _labDescriptionLabels[category] = description;
        _labRatingButtons[category] = new[] { like, neutral, dislike };

        layout.Controls.Add(combo, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(ratingPanel, 0, 2);
        group.Controls.Add(layout);
        return group;
    }

    private RadioButton RatingButton(
        string category,
        WorkspaceExperienceLabRating rating,
        string text)
    {
        var button = new RadioButton
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(4, 6, 18, 4)
        };
        button.CheckedChanged += (_, _) =>
        {
            if (_syncingLabControls || !button.Checked)
            {
                return;
            }

            var option = GetSelectedLabOption(category);
            if (option is not null)
            {
                _experienceLab.SetRating(category, option.Id, rating);
            }
        };
        return button;
    }

    private Control BuildLabDashboard()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(10)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _labSummary.Dock = DockStyle.Fill;
        _labSummary.TextAlign = ContentAlignment.TopLeft;
        _labSummary.Padding = new Padding(6);
        _labSummary.BorderStyle = BorderStyle.FixedSingle;
        _labSummary.AutoEllipsis = false;

        _labAnimationEnabled.Text = "Animation aktiv";
        _labAnimationEnabled.Dock = DockStyle.Fill;
        _labAnimationEnabled.CheckedChanged += (_, _) =>
        {
            if (!_syncingLabControls)
            {
                _experienceLab.SetAnimationEnabled(_labAnimationEnabled.Checked);
            }
        };

        _labSpeed.Minimum = 1;
        _labSpeed.Maximum = 10;
        _labSpeed.TickFrequency = 1;
        _labSpeed.Dock = DockStyle.Fill;
        _labSpeed.ValueChanged += (_, _) =>
        {
            _labSpeedLabel.Text = $"Geschwindigkeit: {_labSpeed.Value}";
            if (!_syncingLabControls)
            {
                _experienceLab.SetSpeed(_labSpeed.Value);
            }
        };
        _labSpeedLabel.Dock = DockStyle.Fill;
        _labSpeedLabel.TextAlign = ContentAlignment.MiddleLeft;

        var openButton = Button(
            "Multi-Window-Prototyp oeffnen",
            OpenMultiWindowPrototype,
            "Oeffnet zwei echte Arbeitsflaechen, die die Lab-Varianten live verwenden.");
        var hint = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = Color.FromArgb(74, 84, 96),
            Text = "Leitsatz: Nehmen. Tragen. Ablegen.\r\nVarianten duerfen nur die Darstellung veraendern, nie den Core."
        };

        panel.Controls.Add(Panel("Live-Auswahl", _labSummary), 0, 0);
        panel.Controls.Add(_labAnimationEnabled, 0, 1);
        panel.Controls.Add(Panel("Geschwindigkeit", BuildSpeedPanel()), 0, 2);
        panel.Controls.Add(openButton, 0, 3);
        panel.Controls.Add(hint, 0, 4);
        return panel;
    }

    private Control BuildSpeedPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(_labSpeedLabel, 0, 0);
        panel.Controls.Add(_labSpeed, 0, 1);
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
        _multiWindowContext = new MultiWindowWorkspaceContext(_experienceLab);
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

    private void OnExperienceLabChanged(object? sender, EventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(RefreshLabControls);
            return;
        }

        RefreshLabControls();
    }

    private void RefreshLabControls()
    {
        _syncingLabControls = true;
        try
        {
            SelectLabOption("grip", _experienceLab.GripVariantId);
            SelectLabOption("edge", _experienceLab.EdgeVariantId);
            SelectLabOption("transition", _experienceLab.TransitionVariantId);
            SelectLabOption("drop", _experienceLab.DropVariantId);
            SelectLabOption("preview", _experienceLab.PreviewVariantId);
            foreach (var category in _labVariantCombos.Keys)
            {
                RefreshLabRating(category);
            }

            _labAnimationEnabled.Checked = _experienceLab.AnimationEnabled;
            _labSpeed.Value = Math.Clamp(_experienceLab.Speed, _labSpeed.Minimum, _labSpeed.Maximum);
            _labSpeedLabel.Text = $"Geschwindigkeit: {_labSpeed.Value}";
            _labSummary.Text = _experienceLab.GetSelectedSummary();
        }
        finally
        {
            _syncingLabControls = false;
        }
    }

    private void SelectLabOption(string category, string variantId)
    {
        if (!_labVariantCombos.TryGetValue(category, out var combo))
        {
            return;
        }

        for (var index = 0; index < combo.Items.Count; index++)
        {
            if (combo.Items[index] is WorkspaceExperienceLabOption option &&
                string.Equals(option.Id, variantId, StringComparison.Ordinal))
            {
                combo.SelectedIndex = index;
                if (_labDescriptionLabels.TryGetValue(category, out var label))
                {
                    label.Text = option.Description;
                }

                return;
            }
        }
    }

    private void RefreshLabRating(string category)
    {
        var option = GetSelectedLabOption(category);
        if (option is null || !_labRatingButtons.TryGetValue(category, out var buttons))
        {
            return;
        }

        var rating = _experienceLab.GetRating(category, option.Id);
        foreach (var button in buttons)
        {
            if (button.Text.StartsWith("Gefaellt mir nicht", StringComparison.Ordinal))
            {
                button.Checked = rating == WorkspaceExperienceLabRating.Dislike;
            }
            else if (button.Text.StartsWith("Gefaellt mir", StringComparison.Ordinal))
            {
                button.Checked = rating == WorkspaceExperienceLabRating.Like;
            }
            else
            {
                button.Checked = rating == WorkspaceExperienceLabRating.Neutral;
            }
        }
    }

    private WorkspaceExperienceLabOption? GetSelectedLabOption(string category)
    {
        return _labVariantCombos.TryGetValue(category, out var combo)
            ? combo.SelectedItem as WorkspaceExperienceLabOption
            : null;
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
