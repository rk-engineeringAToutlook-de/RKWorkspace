namespace RKWorkspace.Transport;

public sealed record TransportEndpoint
{
    private static readonly char[] InvalidNamedPipeCharacters =
    {
        '\\',
        '/',
        ':',
        '*',
        '?',
        '"',
        '<',
        '>',
        '|'
    };

    public required string EndpointId { get; init; }

    public required string Address { get; init; }

    public required string TransportKind { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static TransportEndpoint NamedPipe(string pipeName)
    {
        return new TransportEndpoint
        {
            EndpointId = $"named-pipe:{pipeName}",
            Address = pipeName,
            TransportKind = "NamedPipe"
        }.Validate();
    }

    public TransportEndpoint Validate()
    {
        if (string.IsNullOrWhiteSpace(EndpointId))
        {
            throw new TransportException("Transport endpoint id is required.");
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            throw new TransportException("Transport endpoint address is required.");
        }

        if (string.IsNullOrWhiteSpace(TransportKind))
        {
            throw new TransportException("Transport kind is required.");
        }

        if (string.Equals(TransportKind, "NamedPipe", StringComparison.OrdinalIgnoreCase))
        {
            ValidateNamedPipeAddress(Address);
        }

        return this with
        {
            Metadata = Metadata.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        };
    }

    private static void ValidateNamedPipeAddress(string address)
    {
        if (address.IndexOfAny(InvalidNamedPipeCharacters) >= 0)
        {
            throw new TransportException($"Named pipe address '{address}' contains invalid characters.");
        }

        if (address.Length > 120)
        {
            throw new TransportException("Named pipe address must be 120 characters or less.");
        }
    }
}
