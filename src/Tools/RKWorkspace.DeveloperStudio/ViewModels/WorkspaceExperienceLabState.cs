using System.Text.Json;

namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed class WorkspaceExperienceLabState
{
    private const string LabFolderName = "RKWorkspace";
    private const string LabFileName = "workspace-experience-lab.json";
    private const int MaxCombinationHistory = 24;
    private readonly Dictionary<string, WorkspaceExperienceLabRating> _ratings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _testedByVariant = new(StringComparer.Ordinal);
    private readonly List<string> _combinationHistory = new();
    private int _testedVariants;
    private int _likedVariants;
    private int _nearlyLikedVariants;
    private int _rejectedVariants;
    private int _evolutionStep;

    private WorkspaceExperienceLabState()
    {
    }

    public event EventHandler? Changed;

    public string GripVariantId { get; private set; } = "grip-generation-04";

    public string EdgeVariantId { get; private set; } = "edge-generation-02";

    public string TransitionVariantId { get; private set; } = "transition-generation-03";

    public string DropVariantId { get; private set; } = "drop-generation-02";

    public string PreviewVariantId { get; private set; } = "preview-generation-03";

    public bool AnimationEnabled { get; private set; } = true;

    public int Speed { get; private set; } = 5;

    public int TestedVariants => _testedVariants;

    public int LikedVariants => _likedVariants;

    public int NearlyLikedVariants => _nearlyLikedVariants;

    public int RejectedVariants => _rejectedVariants;

    public int EvolutionStep => _evolutionStep;

    public static IReadOnlyList<WorkspaceExperienceLabOption> GripVariants { get; } = BuildGeneratedVariants(
        "grip",
        "Greifen",
        new[]
        {
            "ruhiges Drag mit neutraler Groesse und wenig Schatten",
            "leichtes Schweben mit mehr Schatten und hellerem Objekt",
            "kleineres Objekt mit aufgenommenem, kompaktem Gefuehl",
            "angehobenes Objekt mit klarerem Rahmen und mehr Tiefe",
            "traeges Objekt mit optischem Gewicht und langsamerem Tempo",
            "sanftes Pulsieren mit wechselndem Glow",
            "eingesammeltes Objekt mit dichterem Abstand und warmer Farbe",
            "Kombination aus Lift, Pulse, Glow und leichtem Einsammeln",
            "groesseres Objekt mit ruhigem Schatten und schnellem Ansprechen",
            "transparenteres Objekt mit schwebender Cursor-Naehe",
            "minimales Schrumpfen mit magnetischem Griffpunkt",
            "mehr Tiefeneindruck mit staerkerem Rahmen",
            "spuerbare Traegheit mit kleiner Rotation",
            "heller Puls mit geringerer Deckkraft",
            "leichtes Einrasten direkt nach dem Greifen",
            "Kombination aus Rotation, Schweben und sanfter Traegheit",
            "kleines Objekt mit schneller Reaktion und kurzem Glow",
            "grosseres Objekt mit langsamem Schweben und weichem Schatten",
            "kompaktes Objekt mit transparenter Kante",
            "tiefer Schatten mit stabilem, schwerem Griffgefuehl",
            "langsamer Puls mit groesserer Distanz zum Cursor",
            "magnetisches Einsammeln mit sichtbarer Zielspannung",
            "leichte Rotation mit wechselnder Transparenz",
            "maximale Kombination aus Nehmen, Tragen und Ablegen"
        });

    public static IReadOnlyList<WorkspaceExperienceLabOption> EdgeVariants { get; } = BuildGeneratedVariants(
        "edge",
        "Rand",
        new[]
        {
            "konstantes Licht am Rand",
            "pulsierender Rand mit ruhiger Zielspannung",
            "Lauflicht als aktive Kante",
            "sich oeffnender Rand mit breiterer Zone",
            "magnetischer Rand mit optischem Zug",
            "unsichtbarer Rand mit fast nur Statusfeedback",
            "grosser Zielbereich fuer fruehes Erkennen",
            "Glow, Puls und breiter Rand kombiniert",
            "weiche Wellenbewegung im Randbereich",
            "einziehender Rand mit staerkerem Zielgefuehl",
            "Projektionsrand mit hellem Zielsignal",
            "bewegte Pfeile in Richtung Zielarbeitsflaeche",
            "Farbdynamik von neutral zu erfolgreich",
            "schmaler Rand mit sehr direktem Magnetismus",
            "breiter Rand mit langsamem Oeffnen",
            "Lichtkanal mit kurzer Einziehbewegung",
            "ruhige Projektionsflaeche ohne harte Kante",
            "schnelle Welle mit klarer Richtung",
            "grosser, fast unsichtbarer Zielraum",
            "magnetische Kante mit pulsierendem Zentrum",
            "Oeffnung plus Richtungspfeile",
            "Lauflicht plus Farbumschlag",
            "weicher Rand mit Sog nach innen",
            "voller Randmix aus Licht, Magnetismus und Bewegung"
        });

    public static IReadOnlyList<WorkspaceExperienceLabOption> TransitionVariants { get; } = BuildGeneratedVariants(
        "transition",
        "Uebergang",
        new[]
        {
            "Objekt verschwindet schnell aus der Quelle",
            "Objekt gleitet weich in Richtung Ziel",
            "Ghost erscheint im Rand",
            "Objekt bleibt halb sichtbar am Fensterrand",
            "Ziel wirkt, als wuerde es das Objekt uebernehmen",
            "Quelle und Ziel zeigen den Uebergang gleichzeitig",
            "Soft Fade mit heller werdendem Objekt",
            "Ghost, halb sichtbar und Zieluebernahme kombiniert",
            "magnetischer Zug mit leichtem Beschleunigen",
            "traeger Uebergang mit Nachlauf",
            "halb transparente Projektion im Ziel",
            "Schatten wandert vor dem Objekt",
            "Perspektivischer Eindruck mit Tiefe",
            "Objekt schrumpft in den Rand",
            "Objekt blendet aus und taucht weich auf",
            "Portalartiger Uebergang am Rand",
            "kontinuierlicher Pfad mit sichtbarer Richtung",
            "Ghost bleibt kurz im Ziel stehen",
            "Quelle gibt los, Ziel nimmt sichtbar auf",
            "kurzer Schattenimpuls vor dem Ablegen",
            "sanfter Perspektivwechsel ohne harte Bewegung",
            "schneller Magnetzug mit weichem Ende",
            "verzoegerter Uebergang mit ruhigem Fade",
            "voller Uebergangsmix aus Ghost, Magnet und Portal"
        });

    public static IReadOnlyList<WorkspaceExperienceLabOption> DropVariants { get; } = BuildGeneratedVariants(
        "drop",
        "Ablegen",
        new[]
        {
            "normales Ablegen ohne Sonderbewegung",
            "sanftes Aufsetzen mit kurzem Ausklingen",
            "kleiner Bounce nach dem Kontakt",
            "magnetisches Einrasten am Ziel",
            "kurzes Vergroessern beim Aufsetzen",
            "Glow um Objekt und Ziel",
            "Objekt richtet sich sichtbar aus",
            "Soft, Glow und Bounce kombiniert",
            "leichtes Schweben vor dem finalen Kontakt",
            "magnetisches Setzen mit schneller Stabilisierung",
            "langsames Aufsetzen mit mehr Gewicht",
            "heller Zielimpuls direkt nach dem Drop",
            "kleines Nachfedern mit geringer Rotation",
            "kompakter Snap ohne sichtbaren Bounce",
            "Objekt wird erst gross, dann ruhig",
            "weiches Ausrichten mit Zielglow",
            "schwebender Abschluss mit sehr sanftem Ende",
            "schneller Drop mit magnetischem Stop",
            "tiefer Schatten beim Aufsetzen",
            "voller Ablegemix aus Snap, Glow und Nachfedern"
        });

    public static IReadOnlyList<WorkspaceExperienceLabOption> PreviewVariants { get; } = BuildGeneratedVariants(
        "preview",
        "Vorschau",
        new[]
        {
            "keine Vorschau, nur Rand und Status",
            "Ghost im Rand ohne zusaetzliche Karte",
            "Workspace Preview mit Zielname und Objektanzahl",
            "Miniaturkarte mit Ziel und Objekt",
            "reduzierter Richtungspfeil",
            "kompakte Zielvorschau mit Status",
            "Richtungsanzeige plus Zielname",
            "minimaler Zielhinweis direkt am Rand"
        });

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
            state._testedVariants = Math.Max(0, persisted.TestedVariants);
            state._likedVariants = Math.Max(0, persisted.LikedVariants);
            state._nearlyLikedVariants = Math.Max(0, persisted.NearlyLikedVariants);
            state._rejectedVariants = Math.Max(0, persisted.RejectedVariants);
            state._evolutionStep = Math.Max(0, persisted.EvolutionStep);
            foreach (var pair in persisted.Ratings)
            {
                state._ratings[pair.Key] = pair.Value;
            }

            foreach (var pair in persisted.TestedByVariant)
            {
                state._testedByVariant[pair.Key] = Math.Max(0, pair.Value);
            }

            state._combinationHistory.AddRange(
                persisted.CombinationHistory
                    .Where(entry => !string.IsNullOrWhiteSpace(entry))
                    .TakeLast(MaxCombinationHistory));
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
            Ratings = new Dictionary<string, WorkspaceExperienceLabRating>(_ratings, StringComparer.Ordinal),
            TestedVariants = _testedVariants,
            LikedVariants = _likedVariants,
            NearlyLikedVariants = _nearlyLikedVariants,
            RejectedVariants = _rejectedVariants,
            EvolutionStep = _evolutionStep,
            CombinationHistory = _combinationHistory.ToArray(),
            TestedByVariant = new Dictionary<string, int>(_testedByVariant, StringComparer.Ordinal)
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
        if (ApplyVariant(category, variantId))
        {
            PersistAndNotify();
        }
    }

    public void SetRating(string category, string variantId, WorkspaceExperienceLabRating rating)
    {
        var key = BuildRatingKey(category, variantId);
        if (rating == WorkspaceExperienceLabRating.NotRated)
        {
            _ratings.Remove(key);
            PersistAndNotify();
            return;
        }

        _ratings[key] = rating;
        RecordEvaluation(category, variantId, rating);
        ApplyVariant(category, SelectNextGeneration(category, variantId, rating));
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

    public void Restore(WorkspaceExperienceLabSnapshot snapshot)
    {
        GripVariantId = ValidOrDefault(GripVariants, snapshot.GripVariantId, GripVariantId);
        EdgeVariantId = ValidOrDefault(EdgeVariants, snapshot.EdgeVariantId, EdgeVariantId);
        TransitionVariantId = ValidOrDefault(TransitionVariants, snapshot.TransitionVariantId, TransitionVariantId);
        DropVariantId = ValidOrDefault(DropVariants, snapshot.DropVariantId, DropVariantId);
        PreviewVariantId = ValidOrDefault(PreviewVariants, snapshot.PreviewVariantId, PreviewVariantId);
        AnimationEnabled = snapshot.AnimationEnabled;
        Speed = Math.Clamp(snapshot.Speed, 1, 10);
        _ratings.Clear();
        foreach (var pair in snapshot.Ratings)
        {
            _ratings[pair.Key] = pair.Value;
        }

        _testedVariants = Math.Max(0, snapshot.TestedVariants);
        _likedVariants = Math.Max(0, snapshot.LikedVariants);
        _nearlyLikedVariants = Math.Max(0, snapshot.NearlyLikedVariants);
        _rejectedVariants = Math.Max(0, snapshot.RejectedVariants);
        _evolutionStep = Math.Max(0, snapshot.EvolutionStep);
        _testedByVariant.Clear();
        foreach (var pair in snapshot.TestedByVariant)
        {
            _testedByVariant[pair.Key] = Math.Max(0, pair.Value);
        }

        _combinationHistory.Clear();
        _combinationHistory.AddRange(snapshot.CombinationHistory.TakeLast(MaxCombinationHistory));
        PersistAndNotify();
    }

    public bool SmokeCheck()
    {
        return GripVariants.Count >= 20 &&
            EdgeVariants.Count >= 20 &&
            TransitionVariants.Count >= 20 &&
            DropVariants.Count >= 20 &&
            PreviewVariants.Count >= 5 &&
            !string.IsNullOrWhiteSpace(GripVariantId) &&
            !string.IsNullOrWhiteSpace(EdgeVariantId) &&
            !string.IsNullOrWhiteSpace(TransitionVariantId) &&
            !string.IsNullOrWhiteSpace(DropVariantId) &&
            !string.IsNullOrWhiteSpace(PreviewVariantId);
    }

    public string GetSelectedSummary()
    {
        return $"Greifen={Find(GripVariants, GripVariantId).DisplayName}; Rand={Find(EdgeVariants, EdgeVariantId).DisplayName}; Uebergang={Find(TransitionVariants, TransitionVariantId).DisplayName}; Ablegen={Find(DropVariants, DropVariantId).DisplayName}; Vorschau={Find(PreviewVariants, PreviewVariantId).DisplayName}; Speed={Speed}; Animation={(AnimationEnabled ? "An" : "Aus")}";
    }

    public string GetEvolutionSummary()
    {
        var last = _combinationHistory.LastOrDefault() ?? "Noch keine Bewertung.";
        return $"Evolution: {_evolutionStep}\r\nGetestet: {_testedVariants}\r\nGruen: {_likedVariants}  Gelb: {_nearlyLikedVariants}  Rot: {_rejectedVariants}\r\nKombinationen: {_combinationHistory.Count}\r\nLetzte Entscheidung: {last}";
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
        string category,
        int generation,
        string title,
        string traits)
    {
        return new WorkspaceExperienceLabOption
        {
            Id = $"{category}-generation-{generation:00}",
            DisplayName = $"Generation {generation:00} - {title}",
            Description = $"Generation {generation:00}: {traits}.",
            Generation = generation,
            Traits = traits
        };
    }

    private static IReadOnlyList<WorkspaceExperienceLabOption> BuildGeneratedVariants(
        string category,
        string title,
        IReadOnlyList<string> traits)
    {
        var options = new List<WorkspaceExperienceLabOption>(traits.Count);
        for (var index = 0; index < traits.Count; index++)
        {
            options.Add(Option(category, index + 1, title, traits[index]));
        }

        return options;
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

    private bool ApplyVariant(string category, string variantId)
    {
        switch (category)
        {
            case "grip":
                GripVariantId = ValidOrDefault(GripVariants, variantId, GripVariantId);
                return true;
            case "edge":
                EdgeVariantId = ValidOrDefault(EdgeVariants, variantId, EdgeVariantId);
                return true;
            case "transition":
                TransitionVariantId = ValidOrDefault(TransitionVariants, variantId, TransitionVariantId);
                return true;
            case "drop":
                DropVariantId = ValidOrDefault(DropVariants, variantId, DropVariantId);
                return true;
            case "preview":
                PreviewVariantId = ValidOrDefault(PreviewVariants, variantId, PreviewVariantId);
                return true;
            default:
                return false;
        }
    }

    private void RecordEvaluation(string category, string variantId, WorkspaceExperienceLabRating rating)
    {
        _testedVariants++;
        _evolutionStep++;
        var key = BuildRatingKey(category, variantId);
        _testedByVariant[key] = _testedByVariant.TryGetValue(key, out var count) ? count + 1 : 1;

        switch (rating)
        {
            case WorkspaceExperienceLabRating.Like:
                _likedVariants++;
                break;
            case WorkspaceExperienceLabRating.Neutral:
                _nearlyLikedVariants++;
                break;
            case WorkspaceExperienceLabRating.Dislike:
                _rejectedVariants++;
                break;
        }

        var options = GetOptions(category);
        var option = options.Count > 0
            ? Find(options, variantId)
            : new WorkspaceExperienceLabOption
            {
                Id = variantId,
                DisplayName = variantId,
                Description = variantId,
                Generation = 0,
                Traits = variantId
            };
        _combinationHistory.Add($"{category} {option.DisplayName} => {RatingLabel(rating)}");
        while (_combinationHistory.Count > MaxCombinationHistory)
        {
            _combinationHistory.RemoveAt(0);
        }
    }

    private string SelectNextGeneration(string category, string variantId, WorkspaceExperienceLabRating rating)
    {
        var options = GetOptions(category);
        if (options.Count == 0)
        {
            return variantId;
        }

        var currentIndex = Math.Max(
            0,
            options.ToList().FindIndex(option => string.Equals(option.Id, variantId, StringComparison.Ordinal)));
        var anchorIndex = rating == WorkspaceExperienceLabRating.Dislike
            ? FindRatedIndex(category, WorkspaceExperienceLabRating.Like) ?? currentIndex
            : currentIndex;
        var step = rating switch
        {
            WorkspaceExperienceLabRating.Like => 1,
            WorkspaceExperienceLabRating.Neutral => 2,
            WorkspaceExperienceLabRating.Dislike => 3,
            _ => 1
        };
        return options[(anchorIndex + step) % options.Count].Id;
    }

    private int? FindRatedIndex(string category, WorkspaceExperienceLabRating rating)
    {
        var options = GetOptions(category);
        for (var index = options.Count - 1; index >= 0; index--)
        {
            if (GetRating(category, options[index].Id) == rating)
            {
                return index;
            }
        }

        return null;
    }

    private static IReadOnlyList<WorkspaceExperienceLabOption> GetOptions(string category)
    {
        return category switch
        {
            "grip" => GripVariants,
            "edge" => EdgeVariants,
            "transition" => TransitionVariants,
            "drop" => DropVariants,
            "preview" => PreviewVariants,
            _ => Array.Empty<WorkspaceExperienceLabOption>()
        };
    }

    private static string RatingLabel(WorkspaceExperienceLabRating rating)
    {
        return rating switch
        {
            WorkspaceExperienceLabRating.Like => "fuehlt sich richtig an",
            WorkspaceExperienceLabRating.Neutral => "fast",
            WorkspaceExperienceLabRating.Dislike => "fuehlt sich falsch an",
            _ => "nicht bewertet"
        };
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
                    Ratings = _ratings,
                    TestedByVariant = _testedByVariant,
                    TestedVariants = _testedVariants,
                    LikedVariants = _likedVariants,
                    NearlyLikedVariants = _nearlyLikedVariants,
                    RejectedVariants = _rejectedVariants,
                    EvolutionStep = _evolutionStep,
                    CombinationHistory = _combinationHistory
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

        public Dictionary<string, int> TestedByVariant { get; set; } = new(StringComparer.Ordinal);

        public int TestedVariants { get; set; }

        public int LikedVariants { get; set; }

        public int NearlyLikedVariants { get; set; }

        public int RejectedVariants { get; set; }

        public int EvolutionStep { get; set; }

        public List<string> CombinationHistory { get; set; } = new();
    }
}
