using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class HumanExperiencePlaygroundState
{
    private const string LabFolderName = "RKWorkspace";
    private const string LabFileName = "human-experience-playground.json";
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private readonly bool _persist;
    private readonly List<HumanExperiencePlaygroundLogEntry> _log = new();
    private string _activeHypothesisId = "A";

    private HumanExperiencePlaygroundState(bool persist)
    {
        _persist = persist;
    }

    public event EventHandler? Changed;

    public string ActiveHypothesisId => _activeHypothesisId;

    public static IReadOnlyList<HumanExperiencePlaygroundHypothesis> Hypotheses { get; } = new[]
    {
        new HumanExperiencePlaygroundHypothesis
        {
            Id = "A",
            Title = "Das Objekt loest sich",
            Perception = "Das Ding loest sich wie ein Blatt Papier vom Untergrund.",
            Intent = "Eine Ecke hebt sich leicht. Der Moment soll nicht wie Schweben wirken, sondern wie physischer Kontakt."
        },
        new HumanExperiencePlaygroundHypothesis
        {
            Id = "B",
            Title = "Die digitale Hand",
            Perception = "Eine fast unsichtbare Greifbewegung laesst das Gehirn die Hand ergaenzen.",
            Intent = "Keine Comic-Hand und kein Mauszeiger. Nur eine minimale Andeutung von Umfassen."
        },
        new HumanExperiencePlaygroundHypothesis
        {
            Id = "C",
            Title = "Das Objekt verschwindet teilweise",
            Perception = "Ein Teil des Dings wird verdeckt, als wuerde es von einer Hand umfasst.",
            Intent = "Das Objekt bleibt sichtbar. Der verdeckte Teil soll Besitz andeuten, nicht verstecken."
        },
        new HumanExperiencePlaygroundHypothesis
        {
            Id = "D",
            Title = "Das Objekt antwortet",
            Perception = "Das Ding bestaetigt Kontrolle durch Ausrichtung, Stabilisierung, Nachgeben und Traegheit.",
            Intent = "Keine Lichteffekte. Nur Verhalten, das sagt: Du kontrollierst mich."
        },
        new HumanExperiencePlaygroundHypothesis
        {
            Id = "E",
            Title = "Die Welt reagiert",
            Perception = "Die Umgebung tritt zurueck und macht Platz fuer den Moment des Haltens.",
            Intent = "Nicht das Ding wird lauter. Die Arbeitsflaeche wird ruhiger und empfangsbereit."
        }
    };

    public static HumanExperiencePlaygroundState Load()
    {
        var state = new HumanExperiencePlaygroundState(persist: true);
        var path = GetStoragePath();
        if (!File.Exists(path))
        {
            return state;
        }

        try
        {
            var persisted = JsonSerializer.Deserialize<PersistedHumanExperiencePlayground>(File.ReadAllText(path), SerializerOptions);
            if (persisted is null)
            {
                return state;
            }

            state._activeHypothesisId = IsKnownHypothesis(persisted.ActiveHypothesisId)
                ? persisted.ActiveHypothesisId!
                : "A";
            state._log.AddRange(persisted.Log.Where(entry => !string.IsNullOrWhiteSpace(entry.Variant)));
        }
        catch
        {
            return state;
        }

        return state;
    }

    public static HumanExperiencePlaygroundState CreateTransient()
    {
        return new HumanExperiencePlaygroundState(persist: false);
    }

    public HumanExperiencePlaygroundSnapshot GetSnapshot()
    {
        return new HumanExperiencePlaygroundSnapshot
        {
            ActiveHypothesisId = _activeHypothesisId,
            Hypotheses = Hypotheses.ToArray(),
            Log = _log.ToArray()
        };
    }

    public HumanExperiencePlaygroundHypothesis GetActiveHypothesis()
    {
        return Hypotheses.First(hypothesis => string.Equals(hypothesis.Id, _activeHypothesisId, StringComparison.Ordinal));
    }

    public void SetActiveHypothesis(string hypothesisId)
    {
        if (!IsKnownHypothesis(hypothesisId) ||
            string.Equals(_activeHypothesisId, hypothesisId, StringComparison.Ordinal))
        {
            return;
        }

        _activeHypothesisId = hypothesisId;
        PersistAndNotify();
    }

    public HumanExperiencePlaygroundLogEntry RecordObservation(
        HumanExperiencePlaygroundRating rating,
        string? comment)
    {
        if (rating == HumanExperiencePlaygroundRating.NotRated)
        {
            throw new InvalidOperationException("A playground rating is required.");
        }

        var hypothesis = GetActiveHypothesis();
        var entry = new HumanExperiencePlaygroundLogEntry
        {
            Variant = hypothesis.DisplayName,
            Perception = hypothesis.Perception,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? "" : comment.Trim()
        };
        _log.Add(entry);
        PersistAndNotify();
        return entry;
    }

    public string GetMissionText()
    {
        return "Der erste Magic Moment\r\n\r\n" +
            "Der Benutzer soll fuer einen kurzen Moment glauben, dass sich ein digitales Objekt wirklich in seiner Hand befindet.";
    }

    public string GetActiveHypothesisText()
    {
        var hypothesis = GetActiveHypothesis();
        return $"{hypothesis.DisplayName}\r\n\r\nWahrnehmung:\r\n{hypothesis.Perception}\r\n\r\nAbsicht:\r\n{hypothesis.Intent}";
    }

    public string GetLogText()
    {
        if (_log.Count == 0)
        {
            return "Noch keine Wahrnehmung gespeichert.";
        }

        var last = _log[^1];
        return $"Letzte Wahrnehmung:\r\n{last.Variant}\r\n{RatingLabel(last.Rating)}";
    }

    public bool SmokeCheck()
    {
        return Hypotheses.Count == 5 &&
            Hypotheses.Select(hypothesis => hypothesis.Id).SequenceEqual(new[] { "A", "B", "C", "D", "E" }) &&
            Hypotheses.All(hypothesis =>
                !hypothesis.DisplayName.Contains("Generation", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(hypothesis.Perception) &&
                !string.IsNullOrWhiteSpace(hypothesis.Intent));
    }

    public static string RatingLabel(HumanExperiencePlaygroundRating rating)
    {
        return rating switch
        {
            HumanExperiencePlaygroundRating.Believe => "Ich glaube fuer einen Moment, dass ich es halte.",
            HumanExperiencePlaygroundRating.Almost => "Fast.",
            HumanExperiencePlaygroundRating.Software => "Es bleibt nur Software.",
            _ => "Nicht bewertet."
        };
    }

    private static bool IsKnownHypothesis(string? hypothesisId)
    {
        return !string.IsNullOrWhiteSpace(hypothesisId) &&
            Hypotheses.Any(hypothesis => string.Equals(hypothesis.Id, hypothesisId, StringComparison.Ordinal));
    }

    private void PersistAndNotify()
    {
        if (_persist)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetStoragePath())!);
            File.WriteAllText(
                GetStoragePath(),
                JsonSerializer.Serialize(
                    new PersistedHumanExperiencePlayground
                    {
                        ActiveHypothesisId = _activeHypothesisId,
                        Log = _log
                    },
                    SerializerOptions));
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string GetStoragePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            LabFolderName,
            LabFileName);
    }

    private sealed class PersistedHumanExperiencePlayground
    {
        public string? ActiveHypothesisId { get; set; }

        public List<HumanExperiencePlaygroundLogEntry> Log { get; set; } = new();
    }
}
