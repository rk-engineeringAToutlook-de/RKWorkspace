using RKWorkspace.Transport;

namespace RKWorkspace.Transport.NamedPipes;

public sealed record NamedPipeTransportOptions
{
    public string ServerName { get; init; } = ".";

    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(5);

    public int MaxServerInstances { get; init; } = 1;

    public NamedPipeTransportOptions Validate()
    {
        if (string.IsNullOrWhiteSpace(ServerName))
        {
            throw new TransportException("Named pipe server name is required.");
        }

        if (DefaultTimeout <= TimeSpan.Zero)
        {
            throw new TransportException("Named pipe timeout must be positive.");
        }

        if (MaxServerInstances <= 0)
        {
            throw new TransportException("Named pipe max server instances must be positive.");
        }

        return this;
    }
}
