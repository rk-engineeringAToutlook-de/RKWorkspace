namespace RKWorkspace.Protocol;

public sealed record RkwpSession(
    string SessionId,
    string OwnerAblageId,
    string GuestAblageId,
    RkwpVersion ProtocolVersion,
    bool SecureSessionRequired,
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
            DateTimeOffset.UtcNow);
    }
}
