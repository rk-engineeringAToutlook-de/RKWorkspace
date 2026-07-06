using System.Security.Cryptography;
using System.Text;
using RKWorkspace.Protocol.Identity;
using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Protocol.Security;

public enum RkwpSecureSessionState
{
    Uninitialized,
    HandshakeStarted,
    IdentityExchanged,
    Authenticated,
    SessionKeyEstablished,
    Active,
    Rejected,
    Expired,
    Revoked,
    Failed
}

public enum RkwpHandshakeState
{
    Uninitialized,
    HandshakeStarted,
    IdentityExchanged,
    Authenticated,
    SessionKeyEstablished,
    Active,
    Rejected,
    Expired,
    Revoked,
    Failed
}

public sealed record RkwpSecurityValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public static RkwpSecurityValidationResult Success(IReadOnlyList<string>? warnings = null)
    {
        return new RkwpSecurityValidationResult(true, Array.Empty<string>(), warnings ?? Array.Empty<string>());
    }

    public static RkwpSecurityValidationResult Failure(IReadOnlyList<string> errors, IReadOnlyList<string>? warnings = null)
    {
        return new RkwpSecurityValidationResult(false, errors, warnings ?? Array.Empty<string>());
    }
}

public sealed record RkwpKeyMaterial(
    string KeyId,
    string Algorithm,
    string PublicKey,
    string PrivateKeyFingerprint,
    bool DevelopmentOnly,
    DateTimeOffset CreatedAt)
{
    public static RkwpKeyMaterial CreateDevelopment(string ablageId)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var seed = $"{ablageId}|{createdAt:O}|{Guid.NewGuid():N}";
        return new RkwpKeyMaterial(
            $"dev-key-{ablageId}-{Guid.NewGuid():N}",
            "DEV-RSA-STRUCTURAL",
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"public|{seed}"))),
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"private|{seed}"))),
            DevelopmentOnly: true,
            createdAt);
    }
}

public sealed record RkwpAblageCertificate(
    string CertificateId,
    string AblageId,
    string Subject,
    string Issuer,
    string PublicKeyId,
    string Thumbprint,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    bool DevelopmentOnly,
    bool Revoked)
{
    public bool IsValidAt(DateTimeOffset now)
    {
        return !Revoked && IssuedAt.AddSeconds(-5) <= now && now < ExpiresAt;
    }
}

public sealed record RkwpDevCertificate(
    RkwpAblageCertificate Certificate,
    RkwpKeyMaterial KeyMaterial,
    string Warning)
{
    public static RkwpDevCertificate Create(AblageIdentity identity)
    {
        var key = RkwpKeyMaterial.CreateDevelopment(identity.AblageId.Value);
        var issuedAt = DateTimeOffset.UtcNow;
        var certificateId = $"dev-cert-{identity.AblageId.Value}-{Guid.NewGuid():N}";
        var thumbprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{certificateId}|{identity.AblageId.Value}|{key.KeyId}|{issuedAt:O}")));
        var certificate = new RkwpAblageCertificate(
            certificateId,
            identity.AblageId.Value,
            identity.DisplayName,
            "RK Workspace Development Certificate Authority",
            key.KeyId,
            thumbprint,
            issuedAt,
            issuedAt.AddDays(30),
            DevelopmentOnly: true,
            Revoked: false);
        return new RkwpDevCertificate(
            certificate,
            key,
            "Development certificate only. Not trusted for production.");
    }
}

public sealed record RkwpSessionKey(
    string SessionKeyId,
    string Algorithm,
    string Fingerprint,
    DateTimeOffset EstablishedAt,
    DateTimeOffset ExpiresAt,
    bool DevelopmentOnly)
{
    public static RkwpSessionKey CreateDevelopment(string sessionId, RkwpAblageCertificate owner, RkwpAblageCertificate guest)
    {
        var establishedAt = DateTimeOffset.UtcNow;
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{sessionId}|{owner.Thumbprint}|{guest.Thumbprint}|{establishedAt:O}")));
        return new RkwpSessionKey(
            $"dev-session-key-{Guid.NewGuid():N}",
            "DEV-HKDF-STRUCTURAL",
            fingerprint,
            establishedAt,
            establishedAt.AddMinutes(15),
            DevelopmentOnly: true);
    }
}

public sealed record RkwpSignature(
    string SignatureId,
    string Algorithm,
    string CertificateId,
    string CertificateThumbprint,
    string SignedValue,
    DateTimeOffset CreatedAt,
    bool DevelopmentOnly)
{
    public static RkwpSignature SignDevelopment(string payload, RkwpAblageCertificate certificate)
    {
        var signedValue = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{certificate.Thumbprint}|{payload}")));
        return new RkwpSignature(
            $"dev-sig-{Guid.NewGuid():N}",
            "DEV-SHA256-STRUCTURAL",
            certificate.CertificateId,
            certificate.Thumbprint,
            signedValue,
            DateTimeOffset.UtcNow,
            DevelopmentOnly: true);
    }

    public bool VerifyDevelopment(string payload, RkwpAblageCertificate certificate)
    {
        var expected = SignDevelopment(payload, certificate).SignedValue;
        return DevelopmentOnly &&
               certificate.CertificateId == CertificateId &&
               certificate.Thumbprint == CertificateThumbprint &&
               string.Equals(expected, SignedValue, StringComparison.Ordinal);
    }
}

public sealed record RkwpSecurityPolicy(
    string PolicyId,
    int PolicyVersion,
    RkwpSecurityMode RequiredSecurityMode,
    bool RequireMutualAuthentication,
    bool RequireSessionKey,
    bool RequireReplayProtection,
    bool RequirePolicyBinding,
    bool RequireAudit,
    bool AllowDevelopmentCertificates)
{
    public static RkwpSecurityPolicy DevelopmentSecure { get; } = new(
        "policy-security-dev-secure-session",
        1,
        RkwpSecurityMode.Authenticated,
        RequireMutualAuthentication: true,
        RequireSessionKey: true,
        RequireReplayProtection: true,
        RequirePolicyBinding: true,
        RequireAudit: true,
        AllowDevelopmentCertificates: true);

    public static RkwpSecurityPolicy ProductionBaseline { get; } = new(
        "policy-security-production-baseline",
        1,
        RkwpSecurityMode.EncryptedAndAuthenticated,
        RequireMutualAuthentication: true,
        RequireSessionKey: true,
        RequireReplayProtection: true,
        RequirePolicyBinding: true,
        RequireAudit: true,
        AllowDevelopmentCertificates: false);

    public RkwpSecurityValidationResult ValidateBindings(CarryLease lease, FrameSession? frame = null)
    {
        var errors = new List<string>();
        if (RequirePolicyBinding)
        {
            if (!string.Equals(lease.PolicyId, PolicyId, StringComparison.Ordinal))
            {
                errors.Add("PolicyMismatch: CarryLease policy id does not match secure session policy.");
            }

            if (lease.PolicyVersion != PolicyVersion)
            {
                errors.Add("PolicyMismatch: CarryLease policy version does not match secure session policy.");
            }

            if (frame is not null &&
                (!string.Equals(frame.PolicyId, PolicyId, StringComparison.Ordinal) || frame.PolicyVersion != PolicyVersion))
            {
                errors.Add("PolicyMismatch: FrameSession policy does not match secure session policy.");
            }
        }

        return errors.Count == 0
            ? RkwpSecurityValidationResult.Success()
            : RkwpSecurityValidationResult.Failure(errors);
    }
}

public sealed record RkwpHandshake(
    string HandshakeId,
    RkwpHandshakeState State,
    AblageIdentity OwnerIdentity,
    AblageIdentity GuestIdentity,
    RkwpAblageCertificate OwnerCertificate,
    RkwpAblageCertificate GuestCertificate,
    RkwpSignature OwnerSignature,
    RkwpSignature GuestSignature,
    RkwpSecurityPolicy Policy,
    DateTimeOffset StartedAt,
    DateTimeOffset? AuthenticatedAt,
    RkwpSessionKey? SessionKey);

public sealed record RkwpSecureSession(
    RkwpSession Session,
    RkwpSecureSessionState State,
    RkwpHandshake Handshake,
    RkwpSessionKey SessionKey,
    RkwpSecurityPolicy Policy,
    DateTimeOffset StartedAt,
    DateTimeOffset ExpiresAt)
{
    public static RkwpSecureSession EstablishDevelopment(
        AblageIdentity owner,
        AblageIdentity guest,
        RkwpSecurityPolicy? policy = null,
        IRkwpAuditSink? auditSink = null,
        DateTimeOffset? now = null)
    {
        var effectivePolicy = policy ?? RkwpSecurityPolicy.DevelopmentSecure;
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var ownerDev = RkwpDevCertificate.Create(owner);
        var guestDev = RkwpDevCertificate.Create(guest);
        var ownerSignature = RkwpSignature.SignDevelopment($"{owner.AblageId.Value}|{guest.AblageId.Value}", ownerDev.Certificate);
        var guestSignature = RkwpSignature.SignDevelopment($"{guest.AblageId.Value}|{owner.AblageId.Value}", guestDev.Certificate);
        var handshake = new RkwpHandshake(
            $"handshake-{Guid.NewGuid():N}",
            RkwpHandshakeState.HandshakeStarted,
            owner,
            guest,
            ownerDev.Certificate,
            guestDev.Certificate,
            ownerSignature,
            guestSignature,
            effectivePolicy,
            timestamp,
            null,
            null);

        auditSink?.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.HandshakeStarted,
            handshake.HandshakeId,
            null,
            null,
            "RKWP secure session handshake started.",
            timestamp));

        var validation = ValidateMutualDevelopmentHandshake(handshake, timestamp);
        if (!validation.IsValid)
        {
            auditSink?.Write(RkwpAuditEvent.SecurityViolation(
                handshake.HandshakeId,
                null,
                "HandshakeRejected",
                string.Join(" ", validation.Errors),
                timestamp));
            throw new RkwpSecurityException(string.Join(" ", validation.Errors));
        }

        auditSink?.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.IdentityExchanged,
            handshake.HandshakeId,
            null,
            null,
            "RKWP secure session identities exchanged.",
            timestamp));

        var session = RkwpSession.CreateSecure(
            owner.AblageId.Value,
            guest.AblageId.Value,
            effectivePolicy.RequiredSecurityMode,
            effectivePolicy.PolicyId,
            effectivePolicy.PolicyVersion);
        var sessionKey = RkwpSessionKey.CreateDevelopment(session.SessionId, ownerDev.Certificate, guestDev.Certificate);
        var authenticatedHandshake = handshake with
        {
            State = RkwpHandshakeState.SessionKeyEstablished,
            AuthenticatedAt = timestamp,
            SessionKey = sessionKey
        };

        auditSink?.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.SecureSessionAuthenticated,
            session.SessionId,
            null,
            null,
            "RKWP development secure session authenticated structurally.",
            timestamp,
            new Dictionary<string, string>
            {
                ["handshakeId"] = handshake.HandshakeId,
                ["developmentOnly"] = "true"
            }));

        return new RkwpSecureSession(
            session,
            RkwpSecureSessionState.Active,
            authenticatedHandshake,
            sessionKey,
            effectivePolicy,
            timestamp,
            sessionKey.ExpiresAt);
    }

    public RkwpSecurityValidationResult ValidateMessage(RkwpMessage message, CarryLease? lease = null, FrameSession? frame = null)
    {
        var errors = new List<string>();
        if (State != RkwpSecureSessionState.Active)
        {
            errors.Add("SecureSessionInactive: message cannot be accepted.");
        }

        if (!string.Equals(message.SessionId, Session.SessionId, StringComparison.Ordinal))
        {
            errors.Add("SessionMismatch: message session id does not match secure session.");
        }

        if (message.Headers.TryGetValue("policy-id", out var policyId) &&
            !string.Equals(policyId, Policy.PolicyId, StringComparison.Ordinal))
        {
            errors.Add("PolicyMismatch: message policy id does not match secure session policy.");
        }

        if (message.Headers.TryGetValue("policy-version", out var policyVersionRaw) &&
            int.TryParse(policyVersionRaw, out var policyVersion) &&
            policyVersion != Policy.PolicyVersion)
        {
            errors.Add("PolicyMismatch: message policy version does not match secure session policy.");
        }

        if (lease is not null)
        {
            if (!string.Equals(lease.SessionId, Session.SessionId, StringComparison.Ordinal))
            {
                errors.Add("LeaseSessionMismatch: lease is bound to a different session.");
            }

            errors.AddRange(Policy.ValidateBindings(lease, frame).Errors);
        }

        return errors.Count == 0
            ? RkwpSecurityValidationResult.Success()
            : RkwpSecurityValidationResult.Failure(errors);
    }

    private static RkwpSecurityValidationResult ValidateMutualDevelopmentHandshake(RkwpHandshake handshake, DateTimeOffset now)
    {
        var errors = new List<string>();
        if (!handshake.Policy.AllowDevelopmentCertificates)
        {
            errors.Add("Development certificates are not allowed by this security policy.");
        }

        ValidateIdentity("Owner", handshake.OwnerIdentity, handshake.OwnerCertificate, errors, now);
        ValidateIdentity("Guest", handshake.GuestIdentity, handshake.GuestCertificate, errors, now);

        if (!handshake.OwnerSignature.VerifyDevelopment(
                $"{handshake.OwnerIdentity.AblageId.Value}|{handshake.GuestIdentity.AblageId.Value}",
                handshake.OwnerCertificate))
        {
            errors.Add("Owner development signature is invalid.");
        }

        if (!handshake.GuestSignature.VerifyDevelopment(
                $"{handshake.GuestIdentity.AblageId.Value}|{handshake.OwnerIdentity.AblageId.Value}",
                handshake.GuestCertificate))
        {
            errors.Add("Guest development signature is invalid.");
        }

        return errors.Count == 0
            ? RkwpSecurityValidationResult.Success(["Development secure session is structurally authenticated only."])
            : RkwpSecurityValidationResult.Failure(errors);
    }

    private static void ValidateIdentity(
        string role,
        AblageIdentity identity,
        RkwpAblageCertificate certificate,
        List<string> errors,
        DateTimeOffset now)
    {
        if (!string.Equals(identity.AblageId.Value, certificate.AblageId, StringComparison.Ordinal))
        {
            errors.Add($"{role} certificate does not belong to identity.");
        }

        if (!certificate.IsValidAt(now))
        {
            errors.Add($"{role} certificate is expired or revoked.");
        }

        if (identity.TrustLevel is AblageTrustLevel.Unknown or AblageTrustLevel.Untrusted or AblageTrustLevel.Revoked)
        {
            errors.Add($"{role} ablage trust level is not allowed for secure session.");
        }

        if (identity.PairingState is AblagePairingState.Denied or AblagePairingState.Revoked or AblagePairingState.PairingPending or AblagePairingState.PairingRequested)
        {
            errors.Add($"{role} ablage pairing state is not allowed for secure session.");
        }
    }
}
