using System.Text.Json;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class HumanExperienceLabState
{
    private const string LabFolderName = "RKWorkspace";
    private const string LabFileName = "human-experience-lab.json";
    private readonly bool _persist;
    private readonly List<HumanExperienceExperiment> _experiments = new();
    private readonly List<HumanExperienceObservation> _observations = new();
    private string _activeHumanExperienceId = "HX-000";
    private string _activeExperimentId = BuildExperimentId("HX-000", 1);
    private int _evolutionStep;

    private HumanExperienceLabState(bool persist)
    {
        _persist = persist;
        EnsureSeedExperiments();
    }

    public event EventHandler? Changed;

    public string ActiveHumanExperienceId => _activeHumanExperienceId;

    public string ActiveExperimentId => _activeExperimentId;

    public int EvolutionStep => _evolutionStep;

    public static IReadOnlyList<HumanExperienceDefinition> HumanExperiences { get; } = new[]
    {
        new HumanExperienceDefinition
        {
            Id = "HX-000",
            Title = "Ich bin in meinem Arbeitsraum.",
            Description = "Der erste Blick erzeugt einen einzigen Arbeitsraum statt mehrere Geraete."
        },
        new HumanExperienceDefinition
        {
            Id = "HX-001",
            Title = "Das gehoert zu meiner Arbeit.",
            Description = "Ein digitales Ding wirkt relevant fuer die aktuelle Aufgabe."
        },
        new HumanExperienceDefinition
        {
            Id = "HX-002",
            Title = "Ich habe etwas in meiner Hand.",
            Description = "Nach dem Greifen fuehlt sich das Ding in der digitalen Hand an."
        },
        new HumanExperienceDefinition
        {
            Id = "HX-003",
            Title = "Ich trage etwas.",
            Description = "Das Ding bewegt sich mit dem Benutzer durch den Arbeitsraum."
        }
    };

    public static HumanExperienceLabState Load()
    {
        var state = new HumanExperienceLabState(persist: true);
        var path = GetStoragePath();
        if (!File.Exists(path))
        {
            return state;
        }

        try
        {
            var persisted = JsonSerializer.Deserialize<PersistedHumanExperienceLab>(File.ReadAllText(path));
            if (persisted is null)
            {
                return state;
            }

            state._experiments.Clear();
            state._experiments.AddRange(persisted.Experiments.Where(IsKnownHumanExperience));
            state._observations.Clear();
            state._observations.AddRange(persisted.Observations.Where(IsKnownHumanExperience));
            state._evolutionStep = Math.Max(0, persisted.EvolutionStep);
            state.EnsureSeedExperiments();
            var activeHumanExperienceId = IsKnownHumanExperience(persisted.ActiveHumanExperienceId)
                ? persisted.ActiveHumanExperienceId!
                : "HX-000";
            state._activeHumanExperienceId = activeHumanExperienceId;
            state._activeExperimentId = state.IsKnownExperiment(persisted.ActiveExperimentId, activeHumanExperienceId)
                ? persisted.ActiveExperimentId!
                : state.GetExperimentsForHumanExperience(activeHumanExperienceId).First().ExperimentId;
        }
        catch
        {
            return state;
        }

        return state;
    }

    public static HumanExperienceLabState CreateTransient()
    {
        return new HumanExperienceLabState(persist: false);
    }

    public HumanExperienceLabSnapshot GetSnapshot()
    {
        return new HumanExperienceLabSnapshot
        {
            ActiveHumanExperienceId = _activeHumanExperienceId,
            ActiveExperimentId = _activeExperimentId,
            EvolutionStep = _evolutionStep,
            HumanExperiences = HumanExperiences.ToArray(),
            Experiments = _experiments.OrderBy(experiment => experiment.HumanExperienceId).ThenBy(experiment => experiment.Number).ToArray(),
            Observations = _observations.OrderByDescending(observation => observation.ObservedAtUtc).ToArray()
        };
    }

    public HumanExperienceDefinition GetActiveHumanExperience()
    {
        return FindHumanExperience(_activeHumanExperienceId);
    }

    public HumanExperienceExperiment GetActiveExperiment()
    {
        return _experiments.First(experiment => string.Equals(experiment.ExperimentId, _activeExperimentId, StringComparison.Ordinal));
    }

    public IReadOnlyList<HumanExperienceExperiment> GetExperimentsForHumanExperience(string humanExperienceId)
    {
        return _experiments
            .Where(experiment => string.Equals(experiment.HumanExperienceId, humanExperienceId, StringComparison.Ordinal))
            .OrderBy(experiment => experiment.Number)
            .ToArray();
    }

    public void SetActiveHumanExperience(string humanExperienceId)
    {
        if (!IsKnownHumanExperience(humanExperienceId) ||
            string.Equals(_activeHumanExperienceId, humanExperienceId, StringComparison.Ordinal))
        {
            return;
        }

        _activeHumanExperienceId = humanExperienceId;
        _activeExperimentId = GetExperimentsForHumanExperience(humanExperienceId).First().ExperimentId;
        PersistAndNotify();
    }

    public void SetActiveExperiment(string experimentId)
    {
        if (!IsKnownExperiment(experimentId, _activeHumanExperienceId) ||
            string.Equals(_activeExperimentId, experimentId, StringComparison.Ordinal))
        {
            return;
        }

        _activeExperimentId = experimentId;
        PersistAndNotify();
    }

    public HumanExperienceObservation RecordObservation(
        HumanExperienceLabRating rating,
        string? comment,
        int durationSeconds,
        int repetitions)
    {
        if (rating == HumanExperienceLabRating.NotRated)
        {
            throw new InvalidOperationException("Human Experience rating is required.");
        }

        var active = GetActiveExperiment();
        var observation = new HumanExperienceObservation
        {
            HumanExperienceId = active.HumanExperienceId,
            ExperimentId = active.ExperimentId,
            ObservedAtUtc = DateTime.UtcNow,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? "" : comment.Trim(),
            DurationSeconds = Math.Max(0, durationSeconds),
            Repetitions = Math.Max(1, repetitions)
        };
        _observations.Add(observation);
        _evolutionStep++;
        _activeExperimentId = EnsureNextExperiment(active, rating).ExperimentId;
        PersistAndNotify();
        return observation;
    }

    public string GetActiveHumanExperienceBlock()
    {
        return "Aktive Human Experience\r\n\r\n" +
            string.Join(
                "\r\n\r\n",
                HumanExperiences.Select(hx =>
                    string.Equals(hx.Id, _activeHumanExperienceId, StringComparison.Ordinal)
                        ? $"> {hx.Id}\r\n{hx.Title}"
                        : $"{hx.Id}\r\n{hx.Title}"));
    }

    public string GetTimelineText()
    {
        return string.Join(
            "\r\n\r\nv\r\n\r\n",
            HumanExperiences.Select(hx => $"{hx.Id}\r\n{hx.Title}\r\nStatus: {GetHumanExperienceStatus(hx.Id)}"));
    }

    public string GetDashboardText()
    {
        var testedHx = HumanExperiences.Count(hx => _observations.Any(observation => observation.HumanExperienceId == hx.Id));
        var confirmedHx = HumanExperiences.Count(hx => _observations.Any(observation =>
            observation.HumanExperienceId == hx.Id &&
            observation.Rating == HumanExperienceLabRating.Right));
        var openHx = HumanExperiences.Count - testedHx;
        var rejectedExperiments = _experiments.Count(experiment =>
            _observations.Any(observation => observation.ExperimentId == experiment.ExperimentId) &&
            _observations
                .Where(observation => observation.ExperimentId == experiment.ExperimentId)
                .All(observation => observation.Rating == HumanExperienceLabRating.No));
        return $"Getestete HX: {testedHx}\r\nOffene HX: {openHx}\r\nBestaetigte HX: {confirmedHx}\r\nVerworfene Experimente: {rejectedExperiments}\r\nAktuelle Evolution: {_evolutionStep}\r\nProtokolle: {_observations.Count}";
    }

    public bool SmokeCheck()
    {
        return HumanExperiences.Count == 4 &&
            _experiments.Count >= 4 &&
            HumanExperiences.All(hx => _experiments.Any(experiment => experiment.HumanExperienceId == hx.Id)) &&
            GetActiveHumanExperienceBlock().Contains("HX-000", StringComparison.Ordinal) &&
            GetTimelineText().Contains("HX-003", StringComparison.Ordinal);
    }

    public static string RatingLabel(HumanExperienceLabRating rating)
    {
        return rating switch
        {
            HumanExperienceLabRating.Right => "Das fuehlt sich richtig an.",
            HumanExperienceLabRating.Almost => "Fast.",
            HumanExperienceLabRating.No => "Nein.",
            _ => "Nicht bewertet."
        };
    }

    private static HumanExperienceDefinition FindHumanExperience(string humanExperienceId)
    {
        return HumanExperiences.First(hx => string.Equals(hx.Id, humanExperienceId, StringComparison.Ordinal));
    }

    private static bool IsKnownHumanExperience(string? humanExperienceId)
    {
        return !string.IsNullOrWhiteSpace(humanExperienceId) &&
            HumanExperiences.Any(hx => string.Equals(hx.Id, humanExperienceId, StringComparison.Ordinal));
    }

    private static bool IsKnownHumanExperience(HumanExperienceExperiment experiment)
    {
        return IsKnownHumanExperience(experiment.HumanExperienceId);
    }

    private static bool IsKnownHumanExperience(HumanExperienceObservation observation)
    {
        return IsKnownHumanExperience(observation.HumanExperienceId);
    }

    private bool IsKnownExperiment(string? experimentId, string humanExperienceId)
    {
        return !string.IsNullOrWhiteSpace(experimentId) &&
            _experiments.Any(experiment =>
                string.Equals(experiment.ExperimentId, experimentId, StringComparison.Ordinal) &&
                string.Equals(experiment.HumanExperienceId, humanExperienceId, StringComparison.Ordinal));
    }

    private string GetHumanExperienceStatus(string humanExperienceId)
    {
        if (_observations.Any(observation =>
            observation.HumanExperienceId == humanExperienceId &&
            observation.Rating == HumanExperienceLabRating.Right))
        {
            return "bestaetigt";
        }

        if (_observations.Any(observation => observation.HumanExperienceId == humanExperienceId))
        {
            return "in Pruefung";
        }

        return "offen";
    }

    private HumanExperienceExperiment EnsureNextExperiment(
        HumanExperienceExperiment current,
        HumanExperienceLabRating rating)
    {
        var anchor = SelectEvolutionAnchor(current, rating);
        var nextNumber = _experiments
            .Where(experiment => experiment.HumanExperienceId == current.HumanExperienceId)
            .Select(experiment => experiment.Number)
            .DefaultIfEmpty(0)
            .Max() + 1;
        var experiment = new HumanExperienceExperiment
        {
            HumanExperienceId = current.HumanExperienceId,
            ExperimentId = BuildExperimentId(current.HumanExperienceId, nextNumber),
            Number = nextNumber,
            Title = $"{FindHumanExperience(current.HumanExperienceId).Title} - Weiterentwicklung",
            Goal = BuildGoalFor(current.HumanExperienceId),
            EvolutionNote = $"Gezielte Weiterentwicklung aus {anchor.DisplayName}: {RatingLabel(rating)}",
            SourceExperimentId = anchor.ExperimentId,
            CreatedAtUtc = DateTime.UtcNow
        };
        _experiments.Add(experiment);
        return experiment;
    }

    private HumanExperienceExperiment SelectEvolutionAnchor(
        HumanExperienceExperiment current,
        HumanExperienceLabRating rating)
    {
        if (rating is HumanExperienceLabRating.Right or HumanExperienceLabRating.Almost)
        {
            return current;
        }

        var preferredObservation = _observations
            .Where(observation =>
                observation.HumanExperienceId == current.HumanExperienceId &&
                observation.Rating is HumanExperienceLabRating.Right or HumanExperienceLabRating.Almost)
            .OrderByDescending(observation => observation.Rating == HumanExperienceLabRating.Right)
            .ThenByDescending(observation => observation.ObservedAtUtc)
            .FirstOrDefault();
        return preferredObservation is null
            ? current
            : _experiments.First(experiment => experiment.ExperimentId == preferredObservation.ExperimentId);
    }

    private void EnsureSeedExperiments()
    {
        foreach (var hx in HumanExperiences)
        {
            if (_experiments.Any(experiment => experiment.HumanExperienceId == hx.Id))
            {
                continue;
            }

            _experiments.Add(new HumanExperienceExperiment
            {
                HumanExperienceId = hx.Id,
                ExperimentId = BuildExperimentId(hx.Id, 1),
                Number = 1,
                Title = $"{hx.Title} - Grundexperiment",
                Goal = BuildGoalFor(hx.Id),
                EvolutionNote = "Erstes Grundexperiment dieser Human Experience.",
                SourceExperimentId = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    private static string BuildGoalFor(string humanExperienceId)
    {
        return humanExperienceId switch
        {
            "HX-000" => "Pruefen, ob der Owner einen einzigen Arbeitsraum wahrnimmt.",
            "HX-001" => "Pruefen, ob ein Ding als Teil der aktuellen Arbeit wirkt.",
            "HX-002" => "Pruefen, ob sich ein gegriffenes Ding in der digitalen Hand anfuehlt.",
            "HX-003" => "Pruefen, ob sich ein Ding getragen statt gezogen anfuehlt.",
            _ => "Pruefen, ob diese Human Experience wahrnehmbar wird."
        };
    }

    private static string BuildExperimentId(string humanExperienceId, int number)
    {
        return $"{humanExperienceId.ToLowerInvariant()}-experiment-{number:000}";
    }

    private void PersistAndNotify()
    {
        if (_persist)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetStoragePath())!);
            File.WriteAllText(
                GetStoragePath(),
                JsonSerializer.Serialize(
                    new PersistedHumanExperienceLab
                    {
                        ActiveHumanExperienceId = _activeHumanExperienceId,
                        ActiveExperimentId = _activeExperimentId,
                        EvolutionStep = _evolutionStep,
                        Experiments = _experiments,
                        Observations = _observations
                    },
                    new JsonSerializerOptions { WriteIndented = true }));
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static string GetStoragePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            LabFolderName,
            LabFileName);
    }

    private sealed class PersistedHumanExperienceLab
    {
        public string? ActiveHumanExperienceId { get; set; }

        public string? ActiveExperimentId { get; set; }

        public int EvolutionStep { get; set; }

        public List<HumanExperienceExperiment> Experiments { get; set; } = new();

        public List<HumanExperienceObservation> Observations { get; set; } = new();
    }
}
