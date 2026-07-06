using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Identity;
using RKWorkspace.Protocol.Security;
using RKWorkspace.Transport;
using RKWorkspace.Transport.Dev;
using RKWorkspace.Transport.NamedPipes;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.SecureDev;

public sealed class RkwpSecureDevTransport : IRkwpTransport
{
    private readonly RkwpDevTransport inner;

    public RkwpSecureDevTransport(RkwpSecureDevTransportOptions options)
    {
        Options = options;
        ValidateOptions(options);
        inner = new RkwpDevTransport(new NamedPipeTransportOptions
        {
            DefaultTimeout = options.DefaultTimeout
        });
    }

    public RkwpSecureDevTransportOptions Options { get; }

    public RkwpTransportMode Mode => RkwpTransportMode.SecureDev;

    public TransportState State => inner.State;

    public bool TlsEnabled => Options.PreferTls && Options.TlsAvailable;

    public RkwpSecurityMode EffectiveSecurityMode =>
        TlsEnabled ? Options.RequestedSecurityMode : RkwpSecurityMode.DevelopmentAuthenticated;

    public string TransportProfile =>
        TlsEnabled ? "SecureDev/TLS" : "SecureDev/NamedPipeDevFallback";

    public string TlsStatus =>
        TlsEnabled ? "TLS enabled for SecureDev spike." : $"TLS unavailable: {Options.TlsBlocker}";

    public bool FallbackClearlyMarked => !TlsEnabled && TlsStatus.Contains("TLS unavailable:", StringComparison.Ordinal);

    public IRkwpTransportServer CreateServer(TransportEndpoint endpoint)
    {
        return inner.CreateServer(endpoint);
    }

    public IRkwpTransportClient CreateClient(TransportEndpoint endpoint)
    {
        return inner.CreateClient(endpoint);
    }

    public RkwpTransportDiagnostics GetDiagnostics()
    {
        var diagnostics = inner.GetDiagnostics();
        return diagnostics with
        {
            Mode = Mode
        };
    }

    public RkwpSecureSession EstablishSession(IRkwpAuditSink? auditSink = null)
    {
        ValidatePeer(Options.OwnerIdentity, "owner");
        ValidatePeer(Options.GuestIdentity, "guest");
        if (EffectiveSecurityMode == RkwpSecurityMode.DevelopmentInsecure)
        {
            throw new RkwpSecureDevTransportException("SecureDevTransport rejects DevelopmentInsecure sessions.");
        }

        return RkwpSecureSession.EstablishDevelopment(
            Options.OwnerIdentity,
            Options.GuestIdentity,
            RkwpSecurityPolicy.DevelopmentSecure,
            auditSink);
    }

    public static void ValidatePeer(AblageIdentity identity, string role)
    {
        if (identity.TrustLevel is AblageTrustLevel.Unknown or AblageTrustLevel.Untrusted or AblageTrustLevel.Revoked)
        {
            throw new RkwpSecureDevTransportException($"{role} ablage is not trusted for SecureDevTransport.");
        }

        if (identity.PairingState is AblagePairingState.Denied or AblagePairingState.Revoked or AblagePairingState.PairingPending or AblagePairingState.PairingRequested)
        {
            throw new RkwpSecureDevTransportException($"{role} ablage pairing state is not active for SecureDevTransport.");
        }
    }

    private static void ValidateOptions(RkwpSecureDevTransportOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EndpointName))
        {
            throw new RkwpSecureDevTransportException("SecureDev endpoint name is required.");
        }

        if (!options.SecureSessionRequired)
        {
            throw new RkwpSecureDevTransportException("SecureDevTransport requires SecureSessionRequired=true.");
        }

        ValidatePeer(options.OwnerIdentity, "owner");
        ValidatePeer(options.GuestIdentity, "guest");
    }
}
