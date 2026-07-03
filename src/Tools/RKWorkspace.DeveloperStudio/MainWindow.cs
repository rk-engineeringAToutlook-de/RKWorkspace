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
    private readonly FirstContactSurface _firstContactSurface = new();
    private readonly InteractiveWorkspaceSurface _interactiveSurface = new();
    private readonly WorkspaceExperienceLabState _experienceLab = WorkspaceExperienceLabState.Load();
    private readonly HumanExperienceLabState _humanExperienceLab = HumanExperienceLabState.Load();
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
    private readonly Label _labEvolutionSummary = ValueLabel();
    private readonly Label _labSpeedLabel = ValueLabel();
    private readonly CheckBox _labAnimationEnabled = new();
    private readonly TrackBar _labSpeed = new();
    private readonly ComboBox _hxSelector = new();
    private readonly ComboBox _hxExperimentSelector = new();
    private readonly Label _hxActive = ValueLabel();
    private readonly Label _hxExperimentDetail = ValueLabel();
    private readonly Label _hxTimeline = ValueLabel();
    private readonly Label _hxDashboard = ValueLabel();
    private readonly TextBox _hxComment = new();
    private readonly NumericUpDown _hxDurationSeconds = new();
    private readonly NumericUpDown _hxRepetitions = new();
    private readonly RadioButton _hxRatingRight = new();
    private readonly RadioButton _hxRatingAlmost = new();
    private readonly RadioButton _hxRatingNo = new();
    private readonly DataGridView _hxExperimentGrid = CreateGrid();
    private readonly DataGridView _hxObservationGrid = CreateGrid();
    private TabControl? _tabs;
    private MultiWindowWorkspaceContext? _multiWindowContext;
    private bool _syncingLabControls;
    private bool _syncingHumanExperienceLabControls;

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
        _humanExperienceLab.Changed += OnHumanExperienceLabChanged;
        ConfigureToolTips();
        RefreshUi();
        RefreshLabControls();
        RefreshHumanExperienceLabControls();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _experienceLab.Changed -= OnExperienceLabChanged;
            _humanExperienceLab.Changed -= OnHumanExperienceLabChanged;
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
        _tabs = tabs;
        var firstContactPage = new TabPage("First Contact");
        firstContactPage.Controls.Add(BuildFirstContactLayout());
        var studioPage = new TabPage("Developer Studio");
        studioPage.Controls.Add(BuildDeveloperStudioLayout());
        var labPage = new TabPage("Human Experience Lab");
        labPage.Controls.Add(BuildHumanExperienceLab());
        tabs.TabPages.Add(firstContactPage);
        tabs.TabPages.Add(studioPage);
        tabs.TabPages.Add(labPage);
        tabs.SelectedTab = firstContactPage;
        return tabs;
    }

    private Control BuildFirstContactLayout()
    {
        var root = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        _firstContactSurface.Dock = DockStyle.Fill;
        root.Controls.Add(_firstContactSurface);
        return root;
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
            "First Contact",
            ResetFirstContactMode,
            "Oeffnet den reduzierten Erstkontakt-Test und setzt seine lokalen Messwerte zurueck."));
        panel.Controls.Add(Button(
            "Runtime starten",
            _viewModel.StartRuntime,
            "Startet die zentrale Core Runtime Engine und bereitet die Manager vor."));
        panel.Controls.Add(Button(
            "Demo-Arbeitsflaechen",
            _viewModel.AddDemoWorkspaces,
            "Legt eine linke und eine rechte Arbeitsflaeche fuer den lokalen Versuch an."));
        panel.Controls.Add(Button(
            "Textding hinlegen",
            _viewModel.CreateTextObject,
            "Legt ein Textding auf die linke Arbeitsflaeche."));
        panel.Controls.Add(Button(
            "Rechts ablegen",
            _viewModel.TransferRight,
            "Legt das Textding auf der rechten Arbeitsflaeche ab."));
        panel.Controls.Add(Button(
            "Zwei lokale Flaechen starten",
            _viewModel.StartDualAgents,
            "Startet zwei lokale Arbeitsflaechen fuer Diagnose und Vergleich."));
        panel.Controls.Add(Button(
            "Zwei lokale Flaechen stoppen",
            _viewModel.StopDualAgents,
            "Stoppt die beiden lokalen Arbeitsflaechen sauber."));
        panel.Controls.Add(Button(
            "Zuruecksetzen",
            _viewModel.Reset,
            "Setzt Runtime, Dinge, lokale Flaechen, Log und Diagnoseansicht zurueck."));
        panel.Controls.Add(Button(
            "Voll-Demo ausfuehren",
            _viewModel.RunFullDemo,
            "Fuehrt Runtime-Start, Arbeitsflaechen, Textding, Ablegen und lokale Flaechen in einem Ablauf aus."));
        panel.Controls.Add(Button(
            "Interaktive Demo zuruecksetzen",
            _viewModel.ResetInteractiveDemo,
            "Initialisiert den Pick-Carry-Place-Prototyp neu."));
        panel.Controls.Add(Button(
            "Interaktive Demo ausfuehren",
            _viewModel.RunFullInteractiveDemo,
            "Simuliert Greifen, Tragen und Ablegen ueber den Core-Pfad."));
        panel.Controls.Add(Button(
            "Multi-Window-Prototyp oeffnen",
            OpenMultiWindowPrototype,
            "Oeffnet zwei echte Arbeitsflaechen mit gemeinsamem lokalen Zustand."));

        return panel;
    }

    private bool ResetFirstContactMode()
    {
        _firstContactSurface.ResetFirstContact();
        if (_tabs is not null && _tabs.TabPages.Count > 0)
        {
            _tabs.SelectedIndex = 0;
        }

        return true;
    }

    private Control BuildHumanExperienceLab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 212));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 52));

        _hxActive.Dock = DockStyle.Fill;
        _hxActive.TextAlign = ContentAlignment.TopLeft;
        _hxActive.Padding = new Padding(8);
        _hxActive.BorderStyle = BorderStyle.FixedSingle;
        _hxActive.AutoEllipsis = false;

        _hxTimeline.Dock = DockStyle.Fill;
        _hxTimeline.TextAlign = ContentAlignment.TopLeft;
        _hxTimeline.Padding = new Padding(8);
        _hxTimeline.BorderStyle = BorderStyle.FixedSingle;
        _hxTimeline.AutoEllipsis = false;

        _hxDashboard.Dock = DockStyle.Fill;
        _hxDashboard.TextAlign = ContentAlignment.TopLeft;
        _hxDashboard.Padding = new Padding(8);
        _hxDashboard.BorderStyle = BorderStyle.FixedSingle;
        _hxDashboard.AutoEllipsis = false;

        root.Controls.Add(Panel("Aktive Human Experience", _hxActive), 0, 0);
        root.Controls.Add(Panel("Human Experience Timeline", _hxTimeline), 1, 0);
        root.Controls.Add(BuildHumanExperienceExperimentPanel(), 0, 1);
        root.Controls.Add(Panel("Human Experience Dashboard", _hxDashboard), 1, 1);
        root.Controls.Add(Panel("Experimente", _hxExperimentGrid), 0, 2);
        root.Controls.Add(Panel("Beobachtungsprotokoll", _hxObservationGrid), 1, 2);
        return root;
    }

    private Control BuildHumanExperienceExperimentPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(6)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        _hxSelector.Dock = DockStyle.Fill;
        _hxSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _hxSelector.DisplayMember = nameof(HumanExperienceDefinition.DisplayName);
        _hxSelector.SelectedIndexChanged += (_, _) =>
        {
            if (_syncingHumanExperienceLabControls ||
                _hxSelector.SelectedItem is not HumanExperienceDefinition definition)
            {
                return;
            }

            _humanExperienceLab.SetActiveHumanExperience(definition.Id);
        };

        _hxExperimentSelector.Dock = DockStyle.Fill;
        _hxExperimentSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _hxExperimentSelector.DisplayMember = nameof(HumanExperienceExperiment.DisplayName);
        _hxExperimentSelector.SelectedIndexChanged += (_, _) =>
        {
            if (_syncingHumanExperienceLabControls ||
                _hxExperimentSelector.SelectedItem is not HumanExperienceExperiment experiment)
            {
                return;
            }

            _humanExperienceLab.SetActiveExperiment(experiment.ExperimentId);
        };

        _hxExperimentDetail.Dock = DockStyle.Fill;
        _hxExperimentDetail.TextAlign = ContentAlignment.TopLeft;
        _hxExperimentDetail.Padding = new Padding(4);
        _hxExperimentDetail.BorderStyle = BorderStyle.FixedSingle;
        _hxExperimentDetail.AutoEllipsis = false;

        var ratingPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
        _hxRatingRight.Text = "Gruen - Das fuehlt sich richtig an.";
        _hxRatingAlmost.Text = "Gelb - Fast.";
        _hxRatingNo.Text = "Rot - Nein.";
        _hxRatingRight.AutoSize = true;
        _hxRatingAlmost.AutoSize = true;
        _hxRatingNo.AutoSize = true;
        _hxRatingRight.Margin = new Padding(4, 7, 12, 4);
        _hxRatingAlmost.Margin = new Padding(4, 7, 12, 4);
        _hxRatingNo.Margin = new Padding(4, 7, 12, 4);
        ratingPanel.Controls.Add(_hxRatingRight);
        ratingPanel.Controls.Add(_hxRatingAlmost);
        ratingPanel.Controls.Add(_hxRatingNo);

        _hxComment.Dock = DockStyle.Fill;
        _hxComment.Multiline = true;
        _hxComment.ScrollBars = ScrollBars.Vertical;

        _hxDurationSeconds.Minimum = 0;
        _hxDurationSeconds.Maximum = 3600;
        _hxDurationSeconds.Dock = DockStyle.Fill;
        _hxRepetitions.Minimum = 1;
        _hxRepetitions.Maximum = 999;
        _hxRepetitions.Value = 1;
        _hxRepetitions.Dock = DockStyle.Fill;

        panel.Controls.Add(new Label { Text = "Human Experience", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        panel.Controls.Add(_hxSelector, 1, 0);
        panel.Controls.Add(new Label { Text = "Experiment", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        panel.Controls.Add(_hxExperimentSelector, 1, 1);
        panel.Controls.Add(new Label { Text = "Ziel", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft }, 0, 2);
        panel.Controls.Add(_hxExperimentDetail, 1, 2);
        panel.Controls.Add(new Label { Text = "Bewertung", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        panel.Controls.Add(ratingPanel, 1, 3);
        panel.Controls.Add(new Label { Text = "Kommentar", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft }, 0, 4);
        panel.Controls.Add(_hxComment, 1, 4);
        panel.Controls.Add(new Label { Text = "Dauer Sekunden", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
        panel.Controls.Add(_hxDurationSeconds, 1, 5);
        panel.Controls.Add(new Label { Text = "Wiederholungen", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 6);
        panel.Controls.Add(_hxRepetitions, 1, 6);
        panel.Controls.Add(Button(
            "Bewertung speichern",
            RecordHumanExperienceObservation,
            "Speichert die Owner-Bewertung lokal im Human Experience Beobachtungsprotokoll."), 1, 7);
        return Panel("Experiment-Modus", panel);
    }

    private bool RecordHumanExperienceObservation()
    {
        var rating = _hxRatingRight.Checked
            ? HumanExperienceLabRating.Right
            : _hxRatingAlmost.Checked
                ? HumanExperienceLabRating.Almost
                : _hxRatingNo.Checked
                    ? HumanExperienceLabRating.No
                    : HumanExperienceLabRating.NotRated;
        if (rating == HumanExperienceLabRating.NotRated)
        {
            return false;
        }

        _humanExperienceLab.RecordObservation(
            rating,
            _hxComment.Text,
            (int)_hxDurationSeconds.Value,
            (int)_hxRepetitions.Value);
        _hxComment.Clear();
        _hxDurationSeconds.Value = 0;
        _hxRepetitions.Value = 1;
        _hxRatingRight.Checked = false;
        _hxRatingAlmost.Checked = false;
        _hxRatingNo.Checked = false;
        RefreshHumanExperienceLabControls();
        return true;
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
            RowCount = 6,
            AutoScroll = true
        };
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.7F));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.7F));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.7F));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.7F));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6F));
        variants.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6F));
        variants.Controls.Add(BuildLabVariantGroup("grip", "Greifen", WorkspaceExperienceLabState.GripVariants), 0, 0);
        variants.Controls.Add(BuildLabVariantGroup("carry", "Tragen", WorkspaceExperienceLabState.CarryVariants), 0, 1);
        variants.Controls.Add(BuildLabVariantGroup("edge", "Durchgang", WorkspaceExperienceLabState.EdgeVariants), 0, 2);
        variants.Controls.Add(BuildLabVariantGroup("transition", "Kontinuitaet", WorkspaceExperienceLabState.TransitionVariants), 0, 3);
        variants.Controls.Add(BuildLabVariantGroup("drop", "Ablegen", WorkspaceExperienceLabState.DropVariants), 0, 4);
        variants.Controls.Add(BuildLabVariantGroup("preview", "Aufmerksamkeit", WorkspaceExperienceLabState.PreviewVariants), 0, 5);

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
        var like = RatingButton(category, WorkspaceExperienceLabRating.Like, "\U0001F7E2 Das fuehlt sich richtig an.");
        var neutral = RatingButton(category, WorkspaceExperienceLabRating.Neutral, "\U0001F7E1 Fast.");
        var dislike = RatingButton(category, WorkspaceExperienceLabRating.Dislike, "\U0001F534 Fuehlt sich falsch an.");
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
            Tag = rating,
            AutoSize = true,
            Margin = new Padding(4, 6, 18, 4)
        };
        _toolTip.SetToolTip(button, "Bewertet diese Generation und waehlt automatisch eine nahe Folgegeneration aus.");
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
            RowCount = 6,
            Padding = new Padding(10)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _labSummary.Dock = DockStyle.Fill;
        _labSummary.TextAlign = ContentAlignment.TopLeft;
        _labSummary.Padding = new Padding(6);
        _labSummary.BorderStyle = BorderStyle.FixedSingle;
        _labSummary.AutoEllipsis = false;

        _labEvolutionSummary.Dock = DockStyle.Fill;
        _labEvolutionSummary.TextAlign = ContentAlignment.TopLeft;
        _labEvolutionSummary.Padding = new Padding(6);
        _labEvolutionSummary.BorderStyle = BorderStyle.FixedSingle;
        _labEvolutionSummary.AutoEllipsis = false;

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
            Text = "Leitsatz: Nehmen. Tragen. Ablegen.\r\nOwner-Test: Habe ich gegriffen? Habe ich die Test-App vergessen? Fuehlte es sich getragen an?\r\nBewertungen erzeugen automatisch eine nahe Folgegeneration. Der Core bleibt unveraendert."
        };

        panel.Controls.Add(Panel("Live-Auswahl", _labSummary), 0, 0);
        panel.Controls.Add(Panel("Evolution", _labEvolutionSummary), 0, 1);
        panel.Controls.Add(_labAnimationEnabled, 0, 2);
        panel.Controls.Add(Panel("Geschwindigkeit", BuildSpeedPanel()), 0, 3);
        panel.Controls.Add(openButton, 0, 4);
        panel.Controls.Add(hint, 0, 5);
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
        return Panel("Nehmen. Tragen. Ablegen.", _interactiveSurface);
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
        grid.Controls.Add(Panel("Dinge", _transferGrid), 1, 0);
        grid.Controls.Add(Panel("Lokale Flaechen", _agentGrid), 2, 0);
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
        AddDiagnosticRow(diagnostics, 3, "Dinge", _transferObjectCount);
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
        _firstContactSurface.SetExperienceLab(_experienceLab.GetSnapshot());
        _interactiveSurface.SetExperienceLab(_experienceLab.GetSnapshot());
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

    private void OnHumanExperienceLabChanged(object? sender, EventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(RefreshHumanExperienceLabControls);
            return;
        }

        RefreshHumanExperienceLabControls();
    }

    private void RefreshHumanExperienceLabControls()
    {
        _syncingHumanExperienceLabControls = true;
        try
        {
            var snapshot = _humanExperienceLab.GetSnapshot();
            _hxActive.Text = _humanExperienceLab.GetActiveHumanExperienceBlock();
            _hxTimeline.Text = _humanExperienceLab.GetTimelineText();
            _hxDashboard.Text = _humanExperienceLab.GetDashboardText();

            _hxSelector.DataSource = snapshot.HumanExperiences.ToArray();
            SelectHumanExperience(snapshot.ActiveHumanExperienceId);

            var experiments = _humanExperienceLab.GetExperimentsForHumanExperience(snapshot.ActiveHumanExperienceId).ToArray();
            _hxExperimentSelector.DataSource = experiments;
            SelectHumanExperienceExperiment(snapshot.ActiveExperimentId);

            var activeExperiment = experiments.FirstOrDefault(experiment =>
                    string.Equals(experiment.ExperimentId, snapshot.ActiveExperimentId, StringComparison.Ordinal)) ??
                experiments.FirstOrDefault();
            _hxExperimentDetail.Text = activeExperiment is null
                ? ""
                : $"{activeExperiment.Title}\r\n\r\n{activeExperiment.Goal}\r\n\r\n{activeExperiment.EvolutionNote}";

            _hxExperimentGrid.DataSource = snapshot.Experiments
                .Select(experiment => new
                {
                    HX = experiment.HumanExperienceId,
                    Experiment = experiment.DisplayName,
                    experiment.Title,
                    experiment.Goal,
                    Quelle = experiment.SourceExperimentId ?? "-",
                    Erstellt = experiment.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                })
                .ToArray();
            _hxObservationGrid.DataSource = snapshot.Observations
                .Select(observation => new
                {
                    HX = observation.HumanExperienceId,
                    Experiment = observation.ExperimentId,
                    Datum = observation.ObservedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    Bewertung = HumanExperienceLabState.RatingLabel(observation.Rating),
                    observation.Comment,
                    DauerSekunden = observation.DurationSeconds,
                    observation.Repetitions
                })
                .ToArray();
            ResizeColumns(_hxExperimentGrid);
            ResizeColumns(_hxObservationGrid);
            ApplyColumnHeaders(_hxExperimentGrid);
            ApplyColumnHeaders(_hxObservationGrid);
        }
        finally
        {
            _syncingHumanExperienceLabControls = false;
        }
    }

    private void SelectHumanExperience(string humanExperienceId)
    {
        for (var index = 0; index < _hxSelector.Items.Count; index++)
        {
            if (_hxSelector.Items[index] is HumanExperienceDefinition definition &&
                string.Equals(definition.Id, humanExperienceId, StringComparison.Ordinal))
            {
                _hxSelector.SelectedIndex = index;
                return;
            }
        }
    }

    private void SelectHumanExperienceExperiment(string experimentId)
    {
        for (var index = 0; index < _hxExperimentSelector.Items.Count; index++)
        {
            if (_hxExperimentSelector.Items[index] is HumanExperienceExperiment experiment &&
                string.Equals(experiment.ExperimentId, experimentId, StringComparison.Ordinal))
            {
                _hxExperimentSelector.SelectedIndex = index;
                return;
            }
        }
    }

    private void OnExperienceLabChanged(object? sender, EventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() =>
            {
                RefreshLabControls();
                RefreshUi();
            });
            return;
        }

        RefreshLabControls();
        RefreshUi();
    }

    private void RefreshLabControls()
    {
        _syncingLabControls = true;
        try
        {
            SelectLabOption("grip", _experienceLab.GripVariantId);
            SelectLabOption("carry", _experienceLab.CarryVariantId);
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
            _labEvolutionSummary.Text = _experienceLab.GetEvolutionSummary();
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
                    label.Text = $"{option.Description}\r\nMerkmale: {option.Traits}";
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
            if (button.Tag is WorkspaceExperienceLabRating buttonRating)
            {
                button.Checked = rating == buttonRating;
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
            _firstContactSurface,
            "First Contact: ein Ding nehmen, nach rechts tragen und dort ablegen. Die Messung bleibt lokal.");
        _toolTip.SetToolTip(
            _interactiveSurface,
            "Ein sichtbarer Bedienprototyp: Textding nehmen, tragen und rechts ablegen.");
        _toolTip.SetToolTip(
            _workspaceGrid,
            "Zeigt die lokal registrierten Arbeitsflaechen mit Art, Lage und Zustand.");
        _toolTip.SetToolTip(
            _transferGrid,
            "Zeigt logische Dinge im lokalen Modell, keine echten Dateien.");
        _toolTip.SetToolTip(
            _agentGrid,
            "Zeigt lokale Flaechenlaeufe, wenn die Zwei-Flaechen-Demo gestartet wurde.");
        _toolTip.SetToolTip(
            _logGrid,
            "Zeigt die zuletzt ausgefuehrten Studio-Aktionen und ihre Ergebnisse.");
        _toolTip.SetToolTip(
            _historyGrid,
            "Zeigt den lokalen Verlauf des aktuellen Dings.");
        _toolTip.SetToolTip(
            _hxSelector,
            "Waehlt die Human Experience, zu der das naechste Experiment gehoert.");
        _toolTip.SetToolTip(
            _hxExperimentSelector,
            "Waehlt ein erhaltenes Experiment. Experimente werden nicht ueberschrieben.");
        _toolTip.SetToolTip(
            _hxComment,
            "Speichert die Beobachtung des Owners lokal im Human Experience Protokoll.");
        _toolTip.SetToolTip(
            _hxExperimentGrid,
            "Zeigt alle bisherigen Human-Experience-Experimente mit Herkunft und Ziel.");
        _toolTip.SetToolTip(
            _hxObservationGrid,
            "Zeigt alle lokalen Bewertungen mit Datum, Dauer und Wiederholungen.");
        _toolTip.SetToolTip(
            _capabilities,
            "Faehigkeiten sind die vom Core erkannten Moeglichkeiten der aktuellen Arbeitsflaechen.");
    }
}
