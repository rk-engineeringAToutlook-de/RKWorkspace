namespace RKWorkspace.Core.Models;

public sealed record DeviceIdentity
{
    public required string DeviceId { get; init; }

    public required string DisplayName { get; init; }

    public required WorkspacePlatform Platform { get; init; }

    public required string PublicKeyFingerprint { get; init; }

    public required string PairingId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset LastSeen { get; init; }

    public static DeviceIdentity Create(
        string displayName,
        WorkspacePlatform platform,
        string publicKeyFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKeyFingerprint);

        var now = DateTimeOffset.UtcNow;

        return new DeviceIdentity
        {
            DeviceId = $"rkws-dev-{Guid.NewGuid():N}",
            DisplayName = displayName,
            Platform = platform,
            PublicKeyFingerprint = publicKeyFingerprint,
            PairingId = NewPairingId(),
            CreatedAt = now,
            LastSeen = now
        };
    }

    private static string NewPairingId()
    {
        var value = Random.Shared.Next(100000, 1000000);
        return value.ToString();
    }
}
