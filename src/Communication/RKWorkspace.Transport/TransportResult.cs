namespace RKWorkspace.Transport;

public sealed record TransportResult
{
    public required bool Success { get; init; }

    public TransportMessage? Message { get; init; }

    public string Error { get; init; } = string.Empty;

    public bool TimedOut { get; init; }

    public static TransportResult FromMessage(TransportMessage message)
    {
        return new TransportResult
        {
            Success = true,
            Message = message
        };
    }

    public static TransportResult FromResponse(TransportMessage response)
    {
        return new TransportResult
        {
            Success = response.MessageType != TransportMessageType.ErrorResponse,
            Message = response,
            Error = response.MessageType == TransportMessageType.ErrorResponse &&
                response.Payload.TryGetValue("error", out var error)
                    ? error
                    : string.Empty
        };
    }

    public static TransportResult Failed(string error, bool timedOut = false)
    {
        return new TransportResult
        {
            Success = false,
            Error = string.IsNullOrWhiteSpace(error) ? "Transport operation failed." : error,
            TimedOut = timedOut
        };
    }
}
