namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialTrayConfiguration
{
    public const int DefaultPort = 5099;

    public int Port { get; init; } = DefaultPort;

    public string ThingName { get; init; } = "Rechnung.pdf";

    public string DesktopAblageName { get; init; } = "Ablage Monitor";

    public string DeskAblageName { get; init; } = "Ablage Schreibtisch";

    public string WebRootPath { get; init; } =
        Path.Combine(AppContext.BaseDirectory, "Web");

    public SpatialTrayConfiguration Validate()
    {
        if (Port is <= 0 or > 65535)
        {
            throw new SpatialTrayException($"Unsupported port '{Port}'.");
        }

        if (string.IsNullOrWhiteSpace(ThingName))
        {
            throw new SpatialTrayException("Thing name is required.");
        }

        if (string.IsNullOrWhiteSpace(DesktopAblageName))
        {
            throw new SpatialTrayException("Desktop ablage name is required.");
        }

        return this;
    }
}
