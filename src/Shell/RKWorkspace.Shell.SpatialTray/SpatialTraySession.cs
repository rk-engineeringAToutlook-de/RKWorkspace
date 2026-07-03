namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTraySession
{
    private readonly object _sync = new();
    private readonly SpatialTrayConfiguration _configuration;
    private SpatialTrayState _state = SpatialTrayState.ThingOnTray;
    private string _activeAblage = "Ablage Monitor";
    private DateTimeOffset? _placedAt;

    public SpatialTraySession(SpatialTrayConfiguration configuration)
    {
        _configuration = configuration.Validate();
    }

    public object Snapshot()
    {
        lock (_sync)
        {
            return new
            {
                state = _state.ToString(),
                thing = "Digitales Ding",
                name = _configuration.ThingName,
                activeAblage = _activeAblage,
                compass = new[]
                {
                    new { id = "monitor", label = _configuration.DesktopAblageName, direction = "rechts", ready = true },
                    new { id = "schreibtisch", label = _configuration.DeskAblageName, direction = "vorne", ready = true },
                    new { id = "links", label = "Ablage links", direction = "links", ready = true }
                },
                status = StatusText(),
                placedAt = _placedAt
            };
        }
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

    public void Pick()
    {
        lock (_sync)
        {
            _state = SpatialTrayState.ThingPicked;
        }
    }

    public void NearAblage(string ablage)
    {
        lock (_sync)
        {
            _state = SpatialTrayState.NearAblage;
            _activeAblage = string.IsNullOrWhiteSpace(ablage)
                ? _configuration.DesktopAblageName
                : ablage;
        }
    }

    public void Place(string? ablage = null)
    {
        lock (_sync)
        {
            _activeAblage = string.IsNullOrWhiteSpace(ablage)
                ? _configuration.DesktopAblageName
                : ablage;
            _state = SpatialTrayState.Placed;
            _placedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            _state = SpatialTrayState.Cancelled;
            _placedAt = null;
        }
    }

    private string StatusText()
    {
        return _state switch
        {
            SpatialTrayState.ThingPicked => "Ding genommen",
            SpatialTrayState.NearAblage => "Hier ablegen",
            SpatialTrayState.Placed => "Abgelegt",
            SpatialTrayState.Cancelled => "Zurueck auf dem Tablett",
            SpatialTrayState.Failed => "Nicht abgelegt",
            _ => "Ding liegt auf dem Tablett"
        };
    }
}
