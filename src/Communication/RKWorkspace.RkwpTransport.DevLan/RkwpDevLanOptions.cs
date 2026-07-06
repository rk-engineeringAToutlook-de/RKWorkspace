namespace RKWorkspace.RkwpTransport.DevLan;

public sealed record RkwpDevLanOptions
{
    public string BindAddress { get; init; } = "127.0.0.1";

    public string Host { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 57100;

    public string AblageId { get; init; } = "ablage-windows-owner";

    public string DisplayName { get; init; } = "Ablage Windows Owner";

    public string DevIdentity { get; init; } = "devlan-local";

    public string SessionId { get; init; } = $"rkwp-devlan-session-{Guid.NewGuid():N}";

    public bool DevPairingAllowed { get; init; } = true;

    public bool SecureSessionRequired { get; init; }

    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(2);

    public TimeSpan GracePeriod { get; init; } = TimeSpan.FromSeconds(10);

    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(3);

    public IReadOnlyList<string> AllowedPeerIds { get; init; } = [];
}
