using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Protocol.Identity;

public readonly record struct AblageIdentityId(string Value)
{
    public override string ToString() => Value;
}

public sealed record AblagePublicKey(
    string KeyId,
    string Algorithm,
    string EncodedPublicKey,
    DateTimeOffset CreatedAt)
{
    public static AblagePublicKey Planned(string keyId = "planned")
    {
        return new AblagePublicKey(keyId, "planned", string.Empty, DateTimeOffset.UtcNow);
    }
}

public enum AblageTrustLevel
{
    Unknown,
    Untrusted,
    DevTrusted,
    UserTrusted,
    PolicyTrusted,
    EnterpriseTrusted,
    Revoked
}

public enum AblagePairingState
{
    Unpaired,
    PairingRequested,
    PairingPending,
    Paired,
    Denied,
    Revoked,
    Expired
}

public sealed record AblageIdentity
{
    public required AblageIdentityId AblageId { get; init; }

    public required string DisplayName { get; init; }

    public required string SurfaceType { get; init; }

    public required string Platform { get; init; }

    public AblagePublicKey? PublicKey { get; init; }

    public required AblageTrustLevel TrustLevel { get; init; }

    public required AblagePairingState PairingState { get; init; }

    public IReadOnlyList<string> Capabilities { get; init; } = [];

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset LastSeen { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static AblageIdentity CreateDev(
        string ablageId,
        string displayName,
        string platform,
        IReadOnlyList<string>? capabilities = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new AblageIdentity
        {
            AblageId = new AblageIdentityId(ablageId),
            DisplayName = displayName,
            SurfaceType = "DevelopmentSurface",
            Platform = platform,
            PublicKey = AblagePublicKey.Planned($"dev-{ablageId}"),
            TrustLevel = AblageTrustLevel.DevTrusted,
            PairingState = AblagePairingState.Paired,
            Capabilities = capabilities ?? ["FrameOnly", "Heartbeat", "NoFileIngress"],
            CreatedAt = now,
            LastSeen = now
        };
    }

    public IReadOnlyDictionary<string, string> ToHelloPayload()
    {
        return IdentityPayload("hello");
    }

    public IReadOnlyDictionary<string, string> ToCapabilitiesPayload()
    {
        var payload = IdentityPayload("capabilities");
        payload["capabilities"] = string.Join(",", Capabilities);
        return payload;
    }

    private Dictionary<string, string> IdentityPayload(string purpose)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["purpose"] = purpose,
            ["ablageId"] = AblageId.Value,
            ["displayName"] = DisplayName,
            ["surfaceType"] = SurfaceType,
            ["platform"] = Platform,
            ["trustLevel"] = TrustLevel.ToString(),
            ["pairingState"] = PairingState.ToString(),
            ["publicKeyId"] = PublicKey?.KeyId ?? string.Empty
        };
    }
}

public sealed record AblagePairingRequest(
    string PairingRequestId,
    AblageIdentityId RequestingAblageId,
    AblageIdentityId TargetAblageId,
    DateTimeOffset RequestedAt,
    AblagePairingState State,
    string Purpose);

public sealed record AblagePairingDecision(
    string PairingRequestId,
    AblageIdentityId RequestingAblageId,
    AblageIdentityId TargetAblageId,
    bool Approved,
    AblageTrustLevel GrantedTrustLevel,
    DateTimeOffset DecidedAt,
    string Reason);

public sealed record AblageTrustPolicy(
    string PolicyId,
    int PolicyVersion,
    bool AllowFrameOnly,
    bool AllowInteractiveFrame,
    bool AllowInput,
    bool AllowExtract,
    bool AllowOwnershipTransfer,
    bool RequireSecureSession,
    bool RequireUserConfirmation,
    bool RequireAudit)
{
    public static AblageTrustPolicy DenyUnknown { get; } = new(
        "policy-trust-deny-unknown",
        1,
        AllowFrameOnly: false,
        AllowInteractiveFrame: false,
        AllowInput: false,
        AllowExtract: false,
        AllowOwnershipTransfer: false,
        RequireSecureSession: true,
        RequireUserConfirmation: true,
        RequireAudit: true);

    public static AblageTrustPolicy DevelopmentFrameOnly { get; } = new(
        "policy-trust-dev-frameonly",
        1,
        AllowFrameOnly: true,
        AllowInteractiveFrame: false,
        AllowInput: false,
        AllowExtract: false,
        AllowOwnershipTransfer: false,
        RequireSecureSession: false,
        RequireUserConfirmation: false,
        RequireAudit: true);

    public static AblageTrustPolicy TrustedInteractiveFrame { get; } = new(
        "policy-trust-interactive-frame",
        1,
        AllowFrameOnly: true,
        AllowInteractiveFrame: true,
        AllowInput: true,
        AllowExtract: false,
        AllowOwnershipTransfer: false,
        RequireSecureSession: true,
        RequireUserConfirmation: true,
        RequireAudit: true);
}

public sealed class AblageTrustException : Exception
{
    public AblageTrustException(string message)
        : base(message)
    {
    }
}

public sealed record AblageTrustDecision(
    bool Allowed,
    string Reason,
    AblageIdentity Identity,
    AblageTrustPolicy Policy)
{
    public static AblageTrustDecision Allow(AblageIdentity identity, AblageTrustPolicy policy, string reason)
    {
        return new AblageTrustDecision(true, reason, identity, policy);
    }

    public static AblageTrustDecision Deny(AblageIdentity identity, AblageTrustPolicy policy, string reason)
    {
        return new AblageTrustDecision(false, reason, identity, policy);
    }
}

public static class AblageTrustGate
{
    public static AblageTrustDecision CanGrantLease(
        AblageIdentity guest,
        AblageTrustPolicy policy,
        OwnershipMode requestedMode,
        RkwpSecurityMode securityMode,
        bool secureSessionRequired)
    {
        if (guest.TrustLevel is AblageTrustLevel.Unknown)
        {
            return AblageTrustDecision.Deny(guest, policy, "Unknown ablage is not trusted.");
        }

        if (guest.TrustLevel is AblageTrustLevel.Untrusted)
        {
            return AblageTrustDecision.Deny(guest, policy, "Untrusted ablage is denied.");
        }

        if (guest.TrustLevel is AblageTrustLevel.Revoked ||
            guest.PairingState is AblagePairingState.Revoked)
        {
            return AblageTrustDecision.Deny(guest, policy, "Revoked ablage is denied.");
        }

        if (guest.PairingState is AblagePairingState.Denied)
        {
            return AblageTrustDecision.Deny(guest, policy, "Pairing was denied.");
        }

        if (guest.PairingState is AblagePairingState.PairingRequested or AblagePairingState.PairingPending)
        {
            return AblageTrustDecision.Deny(guest, policy, "Pairing is pending.");
        }

        if ((policy.RequireSecureSession || secureSessionRequired) &&
            securityMode == RkwpSecurityMode.DevelopmentInsecure)
        {
            return AblageTrustDecision.Deny(guest, policy, "Secure session is required.");
        }

        if (guest.TrustLevel == AblageTrustLevel.DevTrusted &&
            securityMode != RkwpSecurityMode.DevelopmentInsecure)
        {
            return AblageTrustDecision.Deny(guest, policy, "DevTrusted ablage may only be used for development sessions.");
        }

        if (requestedMode == OwnershipMode.FrameOnly && policy.AllowFrameOnly)
        {
            return AblageTrustDecision.Allow(guest, policy, "FrameOnly allowed by trust policy.");
        }

        if (requestedMode == OwnershipMode.InteractiveFrame && policy.AllowInteractiveFrame)
        {
            return AblageTrustDecision.Allow(guest, policy, "Interactive frame allowed by trust policy.");
        }

        if (requestedMode == OwnershipMode.ExtractOnly && policy.AllowExtract)
        {
            return AblageTrustDecision.Allow(guest, policy, "Extract allowed by trust policy.");
        }

        if (requestedMode is OwnershipMode.CopyOut or OwnershipMode.ForkVersion or OwnershipMode.MoveOwnership &&
            !policy.AllowOwnershipTransfer)
        {
            return AblageTrustDecision.Deny(guest, policy, "Ownership transfer is not allowed by trust policy.");
        }

        return AblageTrustDecision.Deny(guest, policy, "Requested mode is not allowed by trust policy.");
    }

    public static CarryLease GrantLease(
        string thingId,
        string ownerAblageId,
        AblageIdentity guest,
        CarryLeasePolicy leasePolicy,
        AblageTrustPolicy trustPolicy,
        RkwpSecurityMode securityMode,
        bool secureSessionRequired,
        DateTimeOffset now)
    {
        var decision = CanGrantLease(
            guest,
            trustPolicy,
            leasePolicy.Mode,
            securityMode,
            secureSessionRequired);
        if (!decision.Allowed)
        {
            throw new AblageTrustException(decision.Reason);
        }

        return CarryLease.Grant(thingId, ownerAblageId, guest.AblageId.Value, leasePolicy, now);
    }

    public static AblageTrustDecision CanOpenFrame(
        AblageIdentity guest,
        AblageTrustPolicy policy,
        FrameMode frameMode,
        RkwpSecurityMode securityMode,
        bool secureSessionRequired)
    {
        var leaseMode = frameMode switch
        {
            FrameMode.ViewOnly or FrameMode.ReadOnlyPresentation => OwnershipMode.FrameOnly,
            FrameMode.ExtractAllowed => OwnershipMode.ExtractOnly,
            _ => OwnershipMode.InteractiveFrame
        };
        var leaseDecision = CanGrantLease(guest, policy, leaseMode, securityMode, secureSessionRequired);
        if (!leaseDecision.Allowed)
        {
            return leaseDecision;
        }

        if (frameMode is FrameMode.Interactive or FrameMode.Annotate && !policy.AllowInteractiveFrame)
        {
            return AblageTrustDecision.Deny(guest, policy, "Interactive frame is not allowed by trust policy.");
        }

        if (frameMode == FrameMode.ExtractAllowed && !policy.AllowExtract)
        {
            return AblageTrustDecision.Deny(guest, policy, "Extract frame is not allowed by trust policy.");
        }

        return AblageTrustDecision.Allow(guest, policy, "Frame mode allowed by trust policy.");
    }
}
