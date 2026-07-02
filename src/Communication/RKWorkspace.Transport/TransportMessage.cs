using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.Transport;

public sealed record TransportMessage : ITransportMessage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(allowIntegerValues: false)
        }
    };

    public required string MessageId { get; init; }

    public required TransportMessageType MessageType { get; init; }

    public required string SourceId { get; init; }

    public required string TargetId { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string> Payload { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string CorrelationId { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static TransportMessage Create(
        TransportMessageType messageType,
        string sourceId,
        string targetId,
        IReadOnlyDictionary<string, string>? payload = null,
        string correlationId = "",
        IReadOnlyDictionary<string, string>? headers = null)
    {
        return new TransportMessage
        {
            MessageId = $"rkws-transport-{Guid.NewGuid():N}",
            MessageType = messageType,
            SourceId = sourceId,
            TargetId = targetId,
            Timestamp = DateTimeOffset.UtcNow,
            Payload = Copy(payload),
            CorrelationId = correlationId,
            Headers = Copy(headers)
        }.Validate();
    }

    public string ToJson()
    {
        Validate();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public TransportMessage Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
        {
            throw new TransportException("Transport message id is required.");
        }

        if (MessageType == TransportMessageType.Unknown)
        {
            throw new TransportException("Transport message type must not be Unknown.");
        }

        if (string.IsNullOrWhiteSpace(SourceId))
        {
            throw new TransportException("Transport source id is required.");
        }

        if (string.IsNullOrWhiteSpace(TargetId))
        {
            throw new TransportException("Transport target id is required.");
        }

        if (Timestamp == default)
        {
            throw new TransportException("Transport timestamp is required.");
        }

        if (Payload is null)
        {
            throw new TransportException("Transport payload is required.");
        }

        if (Headers is null)
        {
            throw new TransportException("Transport headers are required.");
        }

        return this with
        {
            Payload = Copy(Payload),
            Headers = Copy(Headers)
        };
    }

    public static TransportMessage FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new TransportException("Transport message JSON is required.");
        }

        try
        {
            return (JsonSerializer.Deserialize<TransportMessage>(json, JsonOptions)
                ?? throw new TransportException("Transport message JSON did not contain a message."))
                .Validate();
        }
        catch (JsonException ex)
        {
            throw new TransportException("Transport message JSON is invalid or contains an unknown MessageType.", ex);
        }
    }

    private static IReadOnlyDictionary<string, string> Copy(
        IReadOnlyDictionary<string, string>? values)
    {
        return values?.ToDictionary(
            item => item.Key,
            item => item.Value,
            StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
