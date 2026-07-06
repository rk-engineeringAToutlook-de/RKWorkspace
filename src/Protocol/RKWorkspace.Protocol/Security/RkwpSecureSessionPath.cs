using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Protocol.Security;

public sealed record RkwpSessionKeyMaterial(
    string KeyId,
    string Algorithm,
    string Fingerprint,
    DateTimeOffset PreparedAt,
    bool DevelopmentOnly)
{
    public static RkwpSessionKeyMaterial FromSessionKey(RkwpSessionKey sessionKey)
    {
        return new RkwpSessionKeyMaterial(
            sessionKey.SessionKeyId,
            sessionKey.Algorithm,
            sessionKey.Fingerprint,
            sessionKey.EstablishedAt,
            sessionKey.DevelopmentOnly);
    }
}

public sealed record RkwpIdentityProof(
    string AblageId,
    string CertificateId,
    string CertificateThumbprint,
    string SignatureId,
    bool DevelopmentOnly);

public sealed record RkwpAuthenticationResult(
    bool Authenticated,
    RkwpSecureSessionState State,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public static RkwpAuthenticationResult Success(RkwpSecureSessionState state, IReadOnlyList<string>? warnings = null)
    {
        return new RkwpAuthenticationResult(true, state, Array.Empty<string>(), warnings ?? Array.Empty<string>());
    }

    public static RkwpAuthenticationResult Failure(RkwpSecureSessionState state, IReadOnlyList<string> errors)
    {
        return new RkwpAuthenticationResult(false, state, errors, Array.Empty<string>());
    }
}

public sealed record RkwpEncryptionProfile(
    string ProfileId,
    RkwpSecurityMode SecurityMode,
    bool EncryptionRequired,
    bool MutualAuthenticationRequired,
    bool ProductionReady,
    string Notes)
{
    public static RkwpEncryptionProfile DevelopmentAuthenticated { get; } = new(
        "rkwp-dev-authenticated",
        RkwpSecurityMode.DevelopmentAuthenticated,
        EncryptionRequired: false,
        MutualAuthenticationRequired: true,
        ProductionReady: false,
        "Development-authenticated structural session. Not production encryption.");

    public static RkwpEncryptionProfile TestSecure { get; } = new(
        "rkwp-test-secure",
        RkwpSecurityMode.TestSecure,
        EncryptionRequired: true,
        MutualAuthenticationRequired: true,
        ProductionReady: false,
        "Test secure profile for SecureDevTransport spikes.");

    public static RkwpEncryptionProfile ProductionSecure { get; } = new(
        "rkwp-production-secure",
        RkwpSecurityMode.ProductionSecure,
        EncryptionRequired: true,
        MutualAuthenticationRequired: true,
        ProductionReady: true,
        "Production profile placeholder. Final cipher suite is selected later.");
}

public sealed class RkwpReplayProtectionState
{
    private readonly RkwpSequenceValidator validator;

    public RkwpReplayProtectionState(IRkwpAuditSink? auditSink = null)
    {
        validator = new RkwpSequenceValidator(auditSink);
    }

    public void ValidateAndRecord(RkwpMessage message)
    {
        validator.ValidateAndRecord(message);
    }
}

public static class RkwpSecureSessionPath
{
    public static RkwpSecurityGateDecision Evaluate(
        RkwpSecurityConfiguration configuration,
        RkwpSecurityMode requestedSecurityMode,
        bool secureSessionRequiredByPolicy)
    {
        var decision = RkwpSecurityGate.Evaluate(configuration, requestedSecurityMode);
        if (!decision.Allowed || !secureSessionRequiredByPolicy || requestedSecurityMode != RkwpSecurityMode.DevelopmentInsecure)
        {
            return decision;
        }

        var errors = decision.Errors
            .Concat(new[] { "SecureSessionRequired rejects DevelopmentInsecure sessions." })
            .ToArray();
        return RkwpSecurityGateDecision.Deny(
            secureSessionRequired: true,
            decision.AuditRequired,
            decision.ReplayProtectionRequired,
            decision.PolicyBindingRequired,
            errors,
            decision.Warnings);
    }

    public static RkwpAuthenticationResult Authenticate(RkwpSecureSession? secureSession)
    {
        if (secureSession is null)
        {
            return RkwpAuthenticationResult.Failure(
                RkwpSecureSessionState.Rejected,
                ["SecureSessionMissing: authenticated control message requires an active secure session."]);
        }

        if (secureSession.State != RkwpSecureSessionState.Active)
        {
            return RkwpAuthenticationResult.Failure(
                secureSession.State,
                ["SecureSessionInactive: authenticated control message requires an active secure session."]);
        }

        return RkwpAuthenticationResult.Success(secureSession.State);
    }

    public static void RequireAuthenticatedControlMessage(
        RkwpSecureSession? secureSession,
        RkwpMessage message,
        RkwpMessageType expectedMessageType,
        CarryLease? lease = null,
        FrameSession? frame = null,
        RkwpReplayProtectionState? replayProtection = null)
    {
        if (message.MessageType != expectedMessageType)
        {
            throw new RkwpSecurityException($"Unexpected control message type: {message.MessageType}.");
        }

        var authentication = Authenticate(secureSession);
        if (!authentication.Authenticated)
        {
            throw new RkwpSecurityException(string.Join(" ", authentication.Errors));
        }

        var validation = secureSession!.ValidateMessage(message, lease, frame);
        if (!validation.IsValid)
        {
            throw new RkwpSecurityException(string.Join(" ", validation.Errors));
        }

        if (lease is not null)
        {
            RkwpLeaseBindingValidator.Validate(message, lease, frame);
        }

        replayProtection?.ValidateAndRecord(message);
    }
}
