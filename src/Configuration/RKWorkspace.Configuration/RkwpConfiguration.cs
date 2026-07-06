namespace RKWorkspace.Configuration;

public sealed record RkwpConfiguration
{
    public string TransportProfile { get; init; } = "LocalNetworkDev";

    public string Host { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 57100;

    public bool DevPairingAllowed { get; init; } = true;

    public int HeartbeatIntervalSeconds { get; init; } = 2;
}
