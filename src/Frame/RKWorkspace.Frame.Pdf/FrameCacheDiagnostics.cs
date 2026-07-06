namespace RKWorkspace.Frame.Pdf;

public sealed record FrameCacheDiagnostics(
    FrameCacheScope Scope,
    string PolicyProfile,
    int Entries,
    long CachedFrameBytes,
    int FileWrites,
    bool ContainsOriginalFileBytes,
    bool MaterializesOriginalFile,
    string LastEvictionReason,
    bool IsCleared);
