using System.Text.Json;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class WorkspaceExperienceLabState
{
    private const string LabFolderName = "RKWorkspace";
    private const string LabFileName = "workspace-experience-lab.json";
    private readonly Dictionary<string, WorkspaceExperienceLabRating> _ratings = new(StringComparer.Ordinal);

    private WorkspaceExperienceLabState()
    {
    }

    public event EventHandler? Changed;

    public string GripVariantId { get; private set; } = "grip-lift";

    public string EdgeVariantId { get; private set; } = "edge-pulse";

    public string TransitionVariantId { get; private set; } = "transition-ghost";

    public string DropVariantId { get; private set; } = "drop-soft";

    public string PreviewVariantId { get; private set; } = "preview-workspace";

    public bool AnimationEnabled { get; private set; } = true;

    public int Speed { get; private set; } = 5;

    public static IReadOnlyList<WorkspaceExperienceLabOption> GripVariants { get; } = new[]
    {
        Option("grip-normal", "A - Normales Drag", "Karte bleibt ruhig, klassisches Drag-Gefuehl."),
        Option("grip-float", "B - Objekt schwebt", "Karte wird weich heller und wirkt losgeloest."),
        Option("grip-shrink", "C - Objekt wird kleiner", "Karte zieht sich zusammen, als wuerde sie aufgenommen."),
        Option("grip-lift", "D - Objekt hebt sich", "Karte wird groesser und bekommt einen klaren Rahmen."),
        Option("grip-inertia", "E - Objekt bekommt Traegheit", "Karte folgt mit optischem Gewicht und breiterem Rand."),
        Option("grip-pulse", "F - Objekt pulsiert", "Karte pulsiert waehrend sie gehalten wird."),
        Option("grip-collected", "G - Eingesammelt", "Karte wirkt komprimiert und gefasst."),
        Option("grip-combo", "H - Kombination", "Lift, Pulse und schwebender Objektstatus kombiniert.")
    };

    public static IReadOnlyList<WorkspaceExperienceLabOption> EdgeVariants { get; } = new[]
    {
        Option("edge-glow", "A - Glow", "Rand leuchtet konstant."),
        Option("edge-pulse", "B - Pulsieren", "Rand pulsiert beim Uebergang."),
        Option("edge-runner", "C - Lauflicht", "Rand wirkt wie ein aktiver Kanal."),
        Option("edge-opening", "D - Oeffnender Rand", "Rand wird breiter und wirkt wie eine Oeffnung."),
        Option("edge-magnetic", "E - Magnetischer Rand", "Rand zieht optisch staerker an."),
        Option("edge-invisible", "F - Unsichtbarer Rand", "Nur Status und Cursor zeigen den Randkontakt."),
        Option("edge-large", "G - Grosser Zielbereich", "Randzone wird deutlich breiter."),
        Option("edge-combo", "H - Kombination", "Puls, Glow und breiter Zielbereich kombiniert.")
    };

    public static IReadOnlyList<WorkspaceExperienceLabOption> TransitionVariants { get; } = new[]
    {
        Option("transition-vanish", "A - Objekt verschwindet", "Objekt verlaesst die Quelle schnell."),
        Option("transition-slide", "B - Objekt gleitet", "Objekt bewegt sich weich in Richtung Ziel."),
        Option("transition-ghost", "C - Ghost erscheint", "Ghost-Objekt liegt im Rand."),
        Option("transition-half", "D - Halb sichtbar", "Objekt bleibt halb aus der Arbeitsflaeche herausgeschoben."),
        Option("transition-takeover", "E - Wird uebernommen", "Ziel wirkt, als wuerde es das Objekt aufnehmen."),
        Option("transition-continuous", "F - Kontinuierlich", "Quelle und Ziel zeigen gleichzeitig den Uebergang."),
        Option("transition-fade", "G - Soft Fade", "Objekt wird weich heller und taucht im Ziel auf."),
        Option("transition-combo", "H - Kombination", "Ghost, halb sichtbar und Zieluebernahme kombiniert.")
    };

    public static IReadOnlyList<WorkspaceExperienceLabOption> DropVariants { get; } = new[]
    {
        Option("drop-normal", "A - Normales Drop", "Karte erscheint ohne Sonderbewegung."),
        Option("drop-soft", "B - Sanftes Aufsetzen", "Karte setzt weich im Ziel auf."),
        Option("drop-bounce", "C - Kleiner Bounce", "Karte federt kurz nach."),
        Option("drop-snap", "D - Magnetisches Einrasten", "Karte rastet sichtbar ein."),
        Option("drop-grow", "E - Leichtes Vergroessern", "Karte wird kurz groesser."),
        Option("drop-glow", "F - Glow", "Ziel und Karte leuchten beim Ablegen."),
        Option("drop-align", "G - Objekt richtet sich aus", "Karte wirkt geordnet und ausgerichtet."),
        Option("drop-combo", "H - Kombination", "Soft, Glow und kleiner Bounce kombiniert.")
    };

    public static IReadOnlyList<WorkspaceExperienceLabOption> PreviewVariants { get; } = new[]
    {
        Option("preview-none", "A - Keine Vorschau", "Nur Rand und Status."),
        Option("preview-ghost", "B - Ghost", "Nur das Ghost-Objekt im Rand."),
        Option("preview-workspace", "C - Workspace Preview", "Zielname, Status und Objektanzahl."),
        Option("preview-miniature", "D - Miniatur", "Kompakte Vorschaukarte mit Ziel und Objekt."),
        Option("preview-arrow", "E - Nur Richtungspfeil", "Reduzierte Richtung, wenig Text.")
    };

    public static WorkspaceExperienceLabState Load()
    {
        var state = new WorkspaceExperienceLabState();
        var path = GetStoragePath();
        if (!File.Exists(path))
        {
            return state;
        }

        try
        {
            var persisted = JsonSerializer.Deserialize<PersistedLabState>(File.ReadAllText(path));
            if (persisted is null)
            {
                return state;
            }

            state.GripVariantId = ValidOrDefault(GripVariants, persisted.GripVariantId, state.GripVariantId);
            state.EdgeVariantId = ValidOrDefault(EdgeVariants, persisted.EdgeVariantId, state.EdgeVariantId);
            state.TransitionVariantId = ValidOrDefault(TransitionVariants, persisted.TransitionVariantId, state.TransitionVariantId);
            state.DropVariantId = ValidOrDefault(DropVariants, persisted.DropVariantId, state.DropVariantId);
            state.PreviewVariantId = ValidOrDefault(PreviewVariants, persisted.PreviewVariantId, state.PreviewVariantId);
            state.AnimationEnabled = persisted.AnimationEnabled;
            state.Speed = Math.Clamp(persisted.Speed, 1, 10);
            foreach (var pair in persisted.Ratings)
            {
                state._ratings[pair.Key] = pair.Value;
            }
        }
        catch
        {
            return state;
        }

        return state;
    }

    public WorkspaceExperienceLabSnapshot GetSnapshot()
    {
        return new WorkspaceExperienceLabSnapshot
        {
            GripVariantId = GripVariantId,
            EdgeVariantId = EdgeVariantId,
            TransitionVariantId = TransitionVariantId,
            DropVariantId = DropVariantId,
            PreviewVariantId = PreviewVariantId,
            AnimationEnabled = AnimationEnabled,
            Speed = Speed,
            Ratings = new Dictionary<string, WorkspaceExperienceLabRating>(_ratings, StringComparer.Ordinal)
        };
    }

    public WorkspaceExperienceLabRating GetRating(string category, string variantId)
    {
        return _ratings.TryGetValue(BuildRatingKey(category, variantId), out var rating)
            ? rating
            : WorkspaceExperienceLabRating.NotRated;
    }

    public void SetVariant(string category, string variantId)
    {
        switch (category)
        {
            case "grip":
                GripVariantId = ValidOrDefault(GripVariants, variantId, GripVariantId);
                break;
            case "edge":
                EdgeVariantId = ValidOrDefault(EdgeVariants, variantId, EdgeVariantId);
                break;
            case "transition":
                TransitionVariantId = ValidOrDefault(TransitionVariants, variantId, TransitionVariantId);
                break;
            case "drop":
                DropVariantId = ValidOrDefault(DropVariants, variantId, DropVariantId);
                break;
            case "preview":
                PreviewVariantId = ValidOrDefault(PreviewVariants, variantId, PreviewVariantId);
                break;
            default:
                return;
        }

        PersistAndNotify();
    }

    public void SetRating(string category, string variantId, WorkspaceExperienceLabRating rating)
    {
        _ratings[BuildRatingKey(category, variantId)] = rating;
        PersistAndNotify();
    }

    public void SetAnimationEnabled(bool enabled)
    {
        AnimationEnabled = enabled;
        PersistAndNotify();
    }

    public void SetSpeed(int speed)
    {
        Speed = Math.Clamp(speed, 1, 10);
        PersistAndNotify();
    }

    public bool SmokeCheck()
    {
        return GripVariants.Count >= 8 &&
            EdgeVariants.Count >= 8 &&
            TransitionVariants.Count >= 8 &&
            DropVariants.Count >= 8 &&
            PreviewVariants.Count >= 5 &&
            !string.IsNullOrWhiteSpace(GripVariantId) &&
            !string.IsNullOrWhiteSpace(EdgeVariantId) &&
            !string.IsNullOrWhiteSpace(TransitionVariantId) &&
            !string.IsNullOrWhiteSpace(DropVariantId) &&
            !string.IsNullOrWhiteSpace(PreviewVariantId);
    }

    public string GetSelectedSummary()
    {
        return $"Greifen={Find(GripVariants, GripVariantId).DisplayName}; Rand={Find(EdgeVariants, EdgeVariantId).DisplayName}; Uebergang={Find(TransitionVariants, TransitionVariantId).DisplayName}; Ablegen={Find(DropVariants, DropVariantId).DisplayName}; Preview={Find(PreviewVariants, PreviewVariantId).DisplayName}; Speed={Speed}; Animation={(AnimationEnabled ? "An" : "Aus")}";
    }

    public static WorkspaceExperienceLabOption Find(
        IReadOnlyList<WorkspaceExperienceLabOption> options,
        string id)
    {
        return options.FirstOrDefault(option => string.Equals(option.Id, id, StringComparison.Ordinal)) ??
            options[0];
    }

    public static string BuildRatingKey(string category, string variantId)
    {
        return $"{category}:{variantId}";
    }

    private static WorkspaceExperienceLabOption Option(
        string id,
        string displayName,
        string description)
    {
        return new WorkspaceExperienceLabOption
        {
            Id = id,
            DisplayName = displayName,
            Description = description
        };
    }

    private static string ValidOrDefault(
        IReadOnlyList<WorkspaceExperienceLabOption> options,
        string? id,
        string fallback)
    {
        return options.Any(option => string.Equals(option.Id, id, StringComparison.Ordinal))
            ? id ?? fallback
            : fallback;
    }

    private void PersistAndNotify()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(GetStoragePath())!);
        File.WriteAllText(
            GetStoragePath(),
            JsonSerializer.Serialize(
                new PersistedLabState
                {
                    GripVariantId = GripVariantId,
                    EdgeVariantId = EdgeVariantId,
                    TransitionVariantId = TransitionVariantId,
                    DropVariantId = DropVariantId,
                    PreviewVariantId = PreviewVariantId,
                    AnimationEnabled = AnimationEnabled,
                    Speed = Speed,
                    Ratings = _ratings
                },
                new JsonSerializerOptions { WriteIndented = true }));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static string GetStoragePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            LabFolderName,
            LabFileName);
    }

    private sealed class PersistedLabState
    {
        public string? GripVariantId { get; set; }

        public string? EdgeVariantId { get; set; }

        public string? TransitionVariantId { get; set; }

        public string? DropVariantId { get; set; }

        public string? PreviewVariantId { get; set; }

        public bool AnimationEnabled { get; set; } = true;

        public int Speed { get; set; } = 5;

        public Dictionary<string, WorkspaceExperienceLabRating> Ratings { get; set; } = new(StringComparer.Ordinal);
    }
}
