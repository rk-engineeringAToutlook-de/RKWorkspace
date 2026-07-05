namespace RKWorkspace.Transport.Rkwp;

public sealed record RkwpTransportDiagnostics(
    RkwpTransportMode Mode,
    TransportState State,
    int ActiveConnections,
    int MessagesSent,
    int MessagesReceived,
    int Errors,
    DateTimeOffset StartedAt,
    string LastError);
