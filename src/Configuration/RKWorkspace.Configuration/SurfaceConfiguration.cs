namespace RKWorkspace.Configuration;

public sealed record SurfaceConfiguration
{
    public string Platform { get; init; } = "Windows";

    public bool GlassEdgeEnabled { get; init; } = true;

    public bool HapticsEnabled { get; init; }
}
