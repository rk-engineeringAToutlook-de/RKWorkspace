namespace RKWorkspace.Frame.Pdf;

public sealed record FrameCacheEntry(
    string CacheId,
    string FrameSessionId,
    string ThingId,
    int PageNumber,
    FrameFormat FrameFormat,
    FrameCacheScope Scope,
    DateTimeOffset CreatedAt,
    long FrameBytes,
    bool ContainsOriginalFileBytes,
    bool MaterializesOriginalFile,
    string BackingFilePath);
