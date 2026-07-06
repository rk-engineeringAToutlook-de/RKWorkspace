using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Security;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.SecureDev;

public sealed record RkwpSecureDevTransportResult(
    bool Success,
    string AblageId,
    string PeerAblageId,
    RkwpSecurityMode SecurityMode,
    string TransportProfile,
    bool SecureSessionRequired,
    bool TlsEnabled,
    string TlsStatus,
    RkwpSecureSessionState HandshakeState,
    string SessionId,
    IReadOnlyList<string> Events,
    RkwpTransportDiagnostics Diagnostics);
