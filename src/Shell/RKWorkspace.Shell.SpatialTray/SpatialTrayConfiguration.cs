namespace RKWorkspace.Shell.SpatialTray;

public sealed record SpatialTrayConfiguration
{
    public const int DefaultPort = 5099;

    public int Port { get; init; } = DefaultPort;

    public string ThingName { get; init; } = "Rechnung.pdf";

    public string DesktopAblageName { get; init; } = "Ablage Monitor";

    public string DeskAblageName { get; init; } = "Ablage Schreibtisch";

    public double NameRevealThreshold { get; init; } = 0.62;

    public double ActivationThreshold { get; init; } = 0.84;

    public double BubbleScaleFactor { get; init; } = 1.18;

    public double MicroTextOpacity { get; init; } = 0.34;

    public double WobbleAmplitude { get; init; } = 0.005;

    public double WobbleFrequency { get; init; } = 0.02;

    public double SoftSnapStrength { get; init; } = 0.12;

    public double InitialResistanceDistancePx { get; init; } = 10;

    public double HeldCompactScale { get; init; } = 0.94;

    public double HandOcclusionRatio { get; init; } = 0.34;

    public double LiftDepthPx { get; init; } = 34;

    public double VectorTiltMaxDegrees { get; init; } = 4.2;

    public int GlideIntoBubbleMs { get; init; } = 180;

    public bool MobileHapticsPrepared { get; init; } = true;

    public bool OpticalHapticsPrepared { get; init; } = true;

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

        if (NameRevealThreshold is < 0 or > 1)
        {
            throw new SpatialTrayException("Name reveal threshold must be between 0 and 1.");
        }

        if (ActivationThreshold is < 0 or > 1)
        {
            throw new SpatialTrayException("Activation threshold must be between 0 and 1.");
        }

        if (BubbleScaleFactor <= 0)
        {
            throw new SpatialTrayException("Bubble scale factor must be positive.");
        }

        if (WobbleAmplitude < 0)
        {
            throw new SpatialTrayException("Wobble amplitude must not be negative.");
        }

        if (WobbleFrequency < 0)
        {
            throw new SpatialTrayException("Wobble frequency must not be negative.");
        }

        if (SoftSnapStrength is < 0 or > 1)
        {
            throw new SpatialTrayException("Soft snap strength must be between 0 and 1.");
        }

        if (InitialResistanceDistancePx < 0)
        {
            throw new SpatialTrayException("Initial resistance distance must not be negative.");
        }

        if (HeldCompactScale is <= 0 or > 1)
        {
            throw new SpatialTrayException("Held compact scale must be greater than 0 and no more than 1.");
        }

        if (HandOcclusionRatio is < 0 or > 1)
        {
            throw new SpatialTrayException("Hand occlusion ratio must be between 0 and 1.");
        }

        if (LiftDepthPx < 0)
        {
            throw new SpatialTrayException("Lift depth must not be negative.");
        }

        if (VectorTiltMaxDegrees is < 0 or > 12)
        {
            throw new SpatialTrayException("Vector tilt must be subtle.");
        }

        if (GlideIntoBubbleMs < 0)
        {
            throw new SpatialTrayException("Glide duration must not be negative.");
        }

        return this;
    }
}
