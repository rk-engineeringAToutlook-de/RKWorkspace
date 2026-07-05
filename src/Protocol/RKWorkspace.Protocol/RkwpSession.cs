namespace RKWorkspace.Protocol;

public sealed record RkwpSession(
    string SessionId,
    string OwnerAblageId,
    string GuestAblageId,
    RkwpVersion ProtocolVersion,
    bool SecureSessionRequired,
    RkwpSecurityMode SecurityMode,
    string PolicyId,
    int PolicyVersion,
    DateTimeOffset StartedAt)
{
    public static RkwpSession CreateDevelopment(string ownerAblageId, string guestAblageId)
    {
        return new RkwpSession(
            $"rkwp-session-{Guid.NewGuid():N}",
            ownerAblageId,
            guestAblageId,
            RkwpVersion.Current,
            SecureSessionRequired: false,
            RkwpSecurityMode.DevelopmentInsecure,
            "policy-development",
            1,
            DateTimeOffset.UtcNow);
    }

    public static RkwpSession CreateSecure(
        string ownerAblageId,
        string guestAblageId,
        RkwpSecurityMode securityMode = RkwpSecurityMode.EncryptedAndAuthenticated,
        string policyId = "policy-critical-frameonly",
        int policyVersion = 1)
    {
        if (securityMode is RkwpSecurityMode.DevelopmentInsecure)
        {
            throw new RkwpSecurityException("Secure sessions cannot use DevelopmentInsecure mode.");
        }

        return new RkwpSession(
            $"rkwp-session-{Guid.NewGuid():N}",
            ownerAblageId,
            guestAblageId,
            RkwpVersion.Current,
            SecureSessionRequired: true,
            securityMode,
            policyId,
            policyVersion,
            DateTimeOffset.UtcNow);
    }
}
