namespace RKWorkspace.Configuration;

public sealed record ProximityConfiguration
{
    public string Source { get; init; } = "ManualMap";

    public string ManualMapPath { get; init; } = "config/manual-ablage-map.json";

    public double MinimumConfidence { get; init; } = 0.65;

    public int EdgeSwitchDelayMs { get; init; } = 450;
}
