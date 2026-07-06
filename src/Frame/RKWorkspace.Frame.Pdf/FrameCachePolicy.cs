namespace RKWorkspace.Frame.Pdf;

public sealed record FrameCachePolicy(
    FrameCacheScope Scope,
    string PolicyProfile,
    bool AllowDevInspectable,
    bool AllowTemporaryEncrypted)
{
    public static FrameCachePolicy DevelopmentMemoryOnly { get; } =
        new(FrameCacheScope.MemoryOnly, "DevelopmentLab", AllowDevInspectable: true, AllowTemporaryEncrypted: false);

    public static FrameCachePolicy CriticalInfrastructure { get; } =
        new(FrameCacheScope.MemoryOnly, "CriticalInfrastructure", AllowDevInspectable: false, AllowTemporaryEncrypted: false);

    public static FrameCachePolicy Disabled { get; } =
        new(FrameCacheScope.Disabled, "NoCache", AllowDevInspectable: false, AllowTemporaryEncrypted: false);

    public FrameCachePolicy Validate()
    {
        if (Scope == FrameCacheScope.DevInspectable && !AllowDevInspectable)
        {
            throw new PdfFrameRendererException("DevInspectable frame cache is not allowed by this policy.");
        }

        if (Scope == FrameCacheScope.TemporaryEncrypted && !AllowTemporaryEncrypted)
        {
            throw new PdfFrameRendererException("TemporaryEncrypted frame cache is only prepared and not enabled by this policy.");
        }

        if (string.Equals(PolicyProfile, "CriticalInfrastructure", StringComparison.OrdinalIgnoreCase) &&
            Scope == FrameCacheScope.DevInspectable)
        {
            throw new PdfFrameRendererException("CriticalInfrastructure blocks DevInspectable frame cache.");
        }

        return this;
    }
}
