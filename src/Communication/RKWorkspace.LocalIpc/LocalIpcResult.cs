using RKWorkspace.Transport;

namespace RKWorkspace.LocalIpc;

public sealed record LocalIpcResult
{
    public required bool Success { get; init; }

    public LocalIpcMessage? Response { get; init; }

    public string Error { get; init; } = string.Empty;

    public bool TimedOut { get; init; }

    public static LocalIpcResult FromResponse(LocalIpcMessage response)
    {
        return new LocalIpcResult
        {
            Success = response.MessageType != LocalIpcMessageType.ErrorResponse,
            Response = response,
            Error = response.MessageType == LocalIpcMessageType.ErrorResponse &&
                response.Payload.TryGetValue("error", out var error)
                    ? error
                    : string.Empty
        };
    }

    public static LocalIpcResult Failed(string error, bool timedOut = false)
    {
        return new LocalIpcResult
        {
            Success = false,
            Error = error,
            TimedOut = timedOut
        };
    }

    public static LocalIpcResult FromTransport(TransportResult result)
    {
        return new LocalIpcResult
        {
            Success = result.Success,
            Response = result.Message is null
                ? null
                : LocalIpcTransportMapper.ToLocal(result.Message),
            Error = result.Error,
            TimedOut = result.TimedOut
        };
    }
}
