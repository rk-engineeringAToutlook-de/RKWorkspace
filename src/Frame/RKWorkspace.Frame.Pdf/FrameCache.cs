namespace RKWorkspace.Frame.Pdf;

public sealed class FrameCache
{
    private readonly Dictionary<string, FrameCacheEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private string _lastEvictionReason = string.Empty;

    public FrameCache(FrameCachePolicy policy)
    {
        Policy = policy.Validate();
    }

    public FrameCachePolicy Policy { get; }

    public FrameCacheEntry? Store(string frameSessionId, string thingId, PdfFrameRenderResult renderResult, DateTimeOffset now)
    {
        if (Policy.Scope == FrameCacheScope.Disabled)
        {
            return null;
        }

        var cacheId = $"frame-cache-{Guid.NewGuid():N}";
        var entry = new FrameCacheEntry(
            cacheId,
            frameSessionId,
            thingId,
            renderResult.PageNumber,
            renderResult.FrameFormat,
            Policy.Scope,
            now,
            renderResult.PixelData?.Length ?? 0,
            ContainsOriginalFileBytes: false,
            MaterializesOriginalFile: false,
            BackingFilePath: string.Empty);
        _entries[cacheId] = entry;
        return entry;
    }

    public void EvictFrameSession(string frameSessionId, FrameCacheEvictionReason reason)
    {
        var removeIds = _entries.Values
            .Where(entry => string.Equals(entry.FrameSessionId, frameSessionId, StringComparison.OrdinalIgnoreCase))
            .Select(entry => entry.CacheId)
            .ToList();

        foreach (var cacheId in removeIds)
        {
            _entries.Remove(cacheId);
        }

        _lastEvictionReason = reason.ToString();
    }

    public FrameCacheDiagnostics GetDiagnostics()
    {
        return new FrameCacheDiagnostics(
            Policy.Scope,
            Policy.PolicyProfile,
            _entries.Count,
            _entries.Values.Sum(entry => entry.FrameBytes),
            FileWrites: 0,
            ContainsOriginalFileBytes: _entries.Values.Any(entry => entry.ContainsOriginalFileBytes),
            MaterializesOriginalFile: _entries.Values.Any(entry => entry.MaterializesOriginalFile),
            _lastEvictionReason,
            IsCleared: _entries.Count == 0);
    }
}
