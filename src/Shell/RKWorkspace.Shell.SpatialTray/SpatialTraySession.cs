namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTraySession
{
    private readonly object _sync = new();
    private readonly SpatialTrayConfiguration _configuration;
    private readonly IReadOnlyList<AblageDefinition> _ablages;
    private SpatialTrayState _state = SpatialTrayState.ThingOnTray;
    private SpatialTrayCarryState _carryState = SpatialTrayCarryState.OnTray;
    private string? _activeAblageId = "monitor";
    private string _activeAblage;
    private string _placementKind = "Tray";
    private double? _placedX;
    private double? _placedY;
    private DateTimeOffset? _placedAt;

    public SpatialTraySession(SpatialTrayConfiguration configuration)
    {
        _configuration = configuration.Validate();
        _activeAblage = _configuration.DesktopAblageName;
        _ablages =
        [
            new("monitor", _configuration.DesktopAblageName, "rechts", SpatialAblageDistance.VeryNear, 0.76, 0.50),
            new("schreibtisch", _configuration.DeskAblageName, "vorne", SpatialAblageDistance.Near, 0.50, 0.20),
            new("links", "Ablage links", "links", SpatialAblageDistance.Medium, 0.22, 0.54),
            new("fenster", "Ablage Fenster", "links hinten", SpatialAblageDistance.Far, 0.18, 0.24),
            new("ruhig", "Ablage Ruhe", "hinten", SpatialAblageDistance.VeryFar, 0.54, 0.10)
        ];
    }

    public SpatialTrayState CurrentState
    {
        get
        {
            lock (_sync)
            {
                return _state;
            }
        }
    }

    public bool DesktopAblageHasThing
    {
        get
        {
            lock (_sync)
            {
                return _state == SpatialTrayState.Placed &&
                    string.Equals(_placementKind, "Ablage", StringComparison.Ordinal) &&
                    string.Equals(_activeAblage, _configuration.DesktopAblageName, StringComparison.Ordinal);
            }
        }
    }

    public object Snapshot()
    {
        lock (_sync)
        {
            var bubbles = _ablages.Select(CreateBubbleSnapshot).ToArray();
            return new
            {
                state = _state.ToString(),
                carryState = _carryState.ToString(),
                thing = "Digitales Ding",
                name = _configuration.ThingName,
                activeAblage = _activeAblage,
                activeAblageId = _activeAblageId,
                compass = bubbles,
                bubbles,
                motion = new
                {
                    nameRevealThreshold = _configuration.NameRevealThreshold,
                    activationThreshold = _configuration.ActivationThreshold,
                    bubbleScaleFactor = _configuration.BubbleScaleFactor,
                    microTextOpacity = _configuration.MicroTextOpacity,
                    wobbleAmplitude = _configuration.WobbleAmplitude,
                    wobbleFrequency = _configuration.WobbleFrequency,
                    softSnapStrength = _configuration.SoftSnapStrength
                },
                haptics = new
                {
                    mobilePrepared = _configuration.MobileHapticsPrepared,
                    opticalPrepared = _configuration.OpticalHapticsPrepared,
                    digitalHand = true
                },
                placement = new
                {
                    kind = _placementKind,
                    x = _placedX,
                    y = _placedY,
                    placedAt = _placedAt
                },
                releaseMeansPlace = true,
                cancelReturnsToTray = true,
                status = StatusText(),
                placedAt = _placedAt
            };
        }
    }

    public void Pick()
    {
        lock (_sync)
        {
            _state = SpatialTrayState.ThingPicked;
            _carryState = SpatialTrayCarryState.Picked;
            _placementKind = "Hand";
            _placedAt = null;
            _placedX = null;
            _placedY = null;
        }
    }

    public void Carry()
    {
        lock (_sync)
        {
            if (_carryState == SpatialTrayCarryState.Picked || _carryState == SpatialTrayCarryState.Carried)
            {
                _state = SpatialTrayState.ThingPicked;
                _carryState = SpatialTrayCarryState.Carried;
                _placementKind = "Hand";
            }
        }
    }

    public void NearAblage(string ablage)
    {
        lock (_sync)
        {
            var target = FindAblage(ablage);
            _state = SpatialTrayState.NearAblage;
            _carryState = SpatialTrayCarryState.Carried;
            _activeAblageId = target.Id;
            _activeAblage = target.Label;
            _placementKind = "Hand";
        }
    }

    public void Release(double? x, double? y, string? ablage = null, bool placeOnAblage = false)
    {
        lock (_sync)
        {
            if (placeOnAblage && !string.IsNullOrWhiteSpace(ablage))
            {
                Place(ablage);
                return;
            }

            _state = SpatialTrayState.Placed;
            _carryState = SpatialTrayCarryState.Placed;
            _activeAblageId = null;
            _activeAblage = "Freier Raum";
            _placementKind = "Free";
            _placedX = x;
            _placedY = y;
            _placedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Place(string? ablage = null)
    {
        lock (_sync)
        {
            var target = FindAblage(ablage ?? _configuration.DesktopAblageName);
            _activeAblageId = target.Id;
            _activeAblage = target.Label;
            _state = SpatialTrayState.Placed;
            _carryState = SpatialTrayCarryState.Placed;
            _placementKind = "Ablage";
            _placedX = target.X;
            _placedY = target.Y;
            _placedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            _state = SpatialTrayState.Cancelled;
            _carryState = SpatialTrayCarryState.OnTray;
            _activeAblageId = "monitor";
            _activeAblage = _configuration.DesktopAblageName;
            _placementKind = "Tray";
            _placedAt = null;
            _placedX = null;
            _placedY = null;
        }
    }

    private object CreateBubbleSnapshot(AblageDefinition ablage)
    {
        var state = GetBubbleState(ablage);
        return new
        {
            id = ablage.Id,
            label = ablage.Label,
            direction = ablage.Direction,
            distance = ablage.Distance.ToString(),
            state = state.ToString(),
            ready = true,
            x = ablage.X,
            y = ablage.Y,
            scale = Math.Round(BaseScale(ablage.Distance) * _configuration.BubbleScaleFactor, 2),
            nameReadable = state is SpatialAblageBubbleState.Readable or SpatialAblageBubbleState.Active or SpatialAblageBubbleState.Placed,
            microTextVisible = ablage.Distance == SpatialAblageDistance.Medium,
            actionText = state == SpatialAblageBubbleState.Active ? "Hier ablegen" : string.Empty
        };
    }

    private SpatialAblageBubbleState GetBubbleState(AblageDefinition ablage)
    {
        if (_state == SpatialTrayState.Placed &&
            string.Equals(_placementKind, "Ablage", StringComparison.Ordinal) &&
            string.Equals(_activeAblageId, ablage.Id, StringComparison.Ordinal))
        {
            return SpatialAblageBubbleState.Placed;
        }

        if (_state == SpatialTrayState.NearAblage &&
            string.Equals(_activeAblageId, ablage.Id, StringComparison.Ordinal))
        {
            return SpatialAblageBubbleState.Active;
        }

        return ablage.Distance switch
        {
            SpatialAblageDistance.VeryFar => SpatialAblageBubbleState.Visible,
            SpatialAblageDistance.Far => SpatialAblageBubbleState.Visible,
            SpatialAblageDistance.Medium => SpatialAblageBubbleState.Approaching,
            _ => SpatialAblageBubbleState.Readable
        };
    }

    private AblageDefinition FindAblage(string ablage)
    {
        return _ablages.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, ablage, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(candidate.Label, ablage, StringComparison.OrdinalIgnoreCase))
            ?? _ablages.First(candidate => candidate.Id == "monitor");
    }

    private static double BaseScale(SpatialAblageDistance distance)
    {
        return distance switch
        {
            SpatialAblageDistance.VeryFar => 0.36,
            SpatialAblageDistance.Far => 0.52,
            SpatialAblageDistance.Medium => 0.72,
            SpatialAblageDistance.Near => 0.94,
            SpatialAblageDistance.VeryNear => 1.1,
            _ => 0.72
        };
    }

    private string StatusText()
    {
        return _state switch
        {
            SpatialTrayState.ThingPicked when _carryState == SpatialTrayCarryState.Carried => "Ding liegt in deiner Hand",
            SpatialTrayState.ThingPicked => "Ding genommen",
            SpatialTrayState.NearAblage => "Hier ablegen",
            SpatialTrayState.Placed when string.Equals(_placementKind, "Free", StringComparison.Ordinal) => "Ding liegt jetzt hier",
            SpatialTrayState.Placed => "Abgelegt",
            SpatialTrayState.Cancelled => "Zurueck auf dem Tablett",
            SpatialTrayState.Failed => "Nicht abgelegt",
            _ => "Ding liegt auf dem Tablett"
        };
    }

    private sealed record AblageDefinition(
        string Id,
        string Label,
        string Direction,
        SpatialAblageDistance Distance,
        double X,
        double Y);
}
