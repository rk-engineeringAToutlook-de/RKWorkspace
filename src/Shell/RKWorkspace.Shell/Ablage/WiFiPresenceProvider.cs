namespace RKWorkspace.Shell;

public sealed record NetworkPeerPresence(
    AblageIdentity AblageId,
    string DisplayName,
    AblageSurfacePlatform Platform,
    AblageDirection Direction,
    string NetworkProfile,
    TimeSpan LastSeenAge,
    double? LatencyMilliseconds,
    double Confidence,
    bool IsAvailable = true)
{
    public AblageSurface ToSurface(DateTimeOffset capturedAt)
    {
        var lastActiveAt = capturedAt - (LastSeenAge < TimeSpan.Zero ? TimeSpan.Zero : LastSeenAge);
        return new AblageSurface(
            AblageId,
            DisplayName,
            Platform,
            IsAvailable,
            AblagePose.FromDirection(Direction),
            AblageDistance.FromSource(EstimateDistance(), null, Confidence, AblageProximitySource.WiFi),
            lastActiveAt);
    }

    private AblageDistanceKind EstimateDistance()
    {
        if (!IsAvailable || Confidence < 0.30)
        {
            return AblageDistanceKind.Unknown;
        }

        if (LatencyMilliseconds is null)
        {
            return Confidence >= 0.86 ? AblageDistanceKind.Near : AblageDistanceKind.Medium;
        }

        return LatencyMilliseconds.Value switch
        {
            <= 8 => AblageDistanceKind.VeryNear,
            <= 18 => AblageDistanceKind.Near,
            <= 45 => AblageDistanceKind.Medium,
            <= 90 => AblageDistanceKind.Far,
            _ => AblageDistanceKind.VeryFar
        };
    }
}

public sealed class WiFiPresenceProvider : IAblageProximityProvider
{
    private readonly IReadOnlyList<NetworkPeerPresence> _peers;
    private readonly DateTimeOffset? _capturedAt;

    public WiFiPresenceProvider(IReadOnlyList<NetworkPeerPresence> peers, DateTimeOffset? capturedAt = null)
    {
        _peers = peers;
        _capturedAt = capturedAt;
    }

    public AblageProximitySnapshot GetSnapshot(AblageIdentity currentAblageId)
    {
        var capturedAt = _capturedAt ?? DateTimeOffset.UtcNow;
        var surfaces = _peers
            .Where(peer => peer.AblageId != currentAblageId)
            .Select(peer => peer.ToSurface(capturedAt))
            .ToArray();

        return new AblageProximitySnapshot(currentAblageId, surfaces, capturedAt);
    }

    public static IReadOnlyList<NetworkPeerPresence> CreateLabPeers(DateTimeOffset? capturedAt = null)
    {
        return
        [
            new NetworkPeerPresence(
                new AblageIdentity("ablage-macos"),
                "Ablage macOS",
                AblageSurfacePlatform.MacOS,
                AblageDirection.Right,
                "rkws-lab",
                TimeSpan.FromSeconds(2),
                6,
                0.88),
            new NetworkPeerPresence(
                new AblageIdentity("ablage-ipad"),
                "Ablage iPad",
                AblageSurfacePlatform.IOS,
                AblageDirection.Up,
                "rkws-lab",
                TimeSpan.FromSeconds(5),
                18,
                0.74),
            new NetworkPeerPresence(
                new AblageIdentity("ablage-android"),
                "Ablage Android",
                AblageSurfacePlatform.Android,
                AblageDirection.Left,
                "rkws-lab",
                TimeSpan.FromSeconds(12),
                54,
                0.58)
        ];
    }
}
