using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Identity;

namespace RKWorkspace.RkwpTransport.SecureDev;

public sealed record RkwpSecureDevTransportOptions
{
    public required AblageIdentity OwnerIdentity { get; init; }

    public required AblageIdentity GuestIdentity { get; init; }

    public string EndpointName { get; init; } = $"rkws-rkwp-securedev-{Guid.NewGuid():N}";

    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(3);

    public bool PreferTls { get; init; } = true;

    public bool TlsAvailable { get; init; }

    public string TlsBlocker { get; init; } =
        "TLS transport is not implemented in MA011.03; SecureDev uses DevelopmentAuthenticated over NamedPipeDev fallback.";

    public bool SecureSessionRequired { get; init; } = true;

    public RkwpSecurityMode RequestedSecurityMode { get; init; } = RkwpSecurityMode.TestSecure;
}
