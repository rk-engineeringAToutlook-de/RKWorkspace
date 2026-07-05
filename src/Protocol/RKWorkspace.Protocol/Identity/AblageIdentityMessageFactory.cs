namespace RKWorkspace.Protocol.Identity;

public static class AblageIdentityMessageFactory
{
    public static RkwpMessage CreateHello(
        RkwpSession session,
        AblageIdentity sourceIdentity,
        long sequenceNumber)
    {
        EnsureSourceMatches(session, sourceIdentity);
        return RkwpMessage.Create(
            RkwpMessageType.AblageHello,
            session,
            sequenceNumber,
            sourceIdentity.ToHelloPayload());
    }

    public static RkwpMessage CreateCapabilities(
        RkwpSession session,
        AblageIdentity sourceIdentity,
        long sequenceNumber)
    {
        EnsureSourceMatches(session, sourceIdentity);
        return RkwpMessage.Create(
            RkwpMessageType.AblageCapabilities,
            session,
            sequenceNumber,
            sourceIdentity.ToCapabilitiesPayload());
    }

    private static void EnsureSourceMatches(RkwpSession session, AblageIdentity sourceIdentity)
    {
        if (!string.Equals(session.OwnerAblageId, sourceIdentity.AblageId.Value, StringComparison.Ordinal) &&
            !string.Equals(session.GuestAblageId, sourceIdentity.AblageId.Value, StringComparison.Ordinal))
        {
            throw new AblageTrustException("Ablage identity does not belong to this RKWP session.");
        }
    }
}
