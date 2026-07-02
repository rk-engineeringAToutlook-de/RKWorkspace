using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.LocalIpc;

public sealed record LocalIpcMessage
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

    public required LocalIpcMessageType MessageType { get; init; }

    public required string SourceAgentId { get; init; }

    public required string TargetAgentId { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string> Payload { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static LocalIpcMessage Create(
        LocalIpcMessageType messageType,
        string sourceAgentId,
        string targetAgentId,
        IReadOnlyDictionary<string, string>? payload = null)
    {
        return new LocalIpcMessage
        {
            MessageId = $"rkws-ipc-{Guid.NewGuid():N}",
            MessageType = messageType,
            SourceAgentId = sourceAgentId,
            TargetAgentId = targetAgentId,
            Timestamp = DateTimeOffset.UtcNow,
            Payload = payload?.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        }.Validate();
    }

    public string ToJson()
    {
        Validate();
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public LocalIpcMessage Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
        {
            throw new LocalIpcException("IPC message id is required.");
        }

        if (string.IsNullOrWhiteSpace(SourceAgentId))
        {
            throw new LocalIpcException("IPC source agent id is required.");
        }

        if (string.IsNullOrWhiteSpace(TargetAgentId))
        {
            throw new LocalIpcException("IPC target agent id is required.");
        }

        if (Timestamp == default)
        {
            throw new LocalIpcException("IPC timestamp is required.");
        }

        if (Payload is null)
        {
            throw new LocalIpcException("IPC payload is required.");
        }

        return this with
        {
            Payload = Payload.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        };
    }

    public static LocalIpcMessage FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new LocalIpcException("IPC message JSON is required.");
        }

        try
        {
            return (JsonSerializer.Deserialize<LocalIpcMessage>(json, JsonOptions)
                ?? throw new LocalIpcException("IPC message JSON did not contain a message."))
                .Validate();
        }
        catch (JsonException ex)
        {
            throw new LocalIpcException("IPC message JSON is invalid or contains an unknown MessageType.", ex);
        }
    }
}
