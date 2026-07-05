namespace RKWorkspace.Transport.Rkwp;

public sealed record RkwpTransportConnection(
    string ConnectionId,
    RkwpTransportMode Mode,
    string LocalAblageId,
    string RemoteAblageId,
    string SessionId,
    DateTimeOffset ConnectedAt,
    DateTimeOffset LastSeenAt,
    bool IsOpen);
