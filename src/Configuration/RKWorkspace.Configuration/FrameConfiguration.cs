namespace RKWorkspace.Configuration;

public sealed record FrameConfiguration
{
    public string FrameCachePolicy { get; init; } = "MemoryOnly";

    public string RendererName { get; init; } = "MetadataPreviewDevRenderer";

    public bool AllowGuestFileIngress { get; init; }
}
