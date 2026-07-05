namespace RKWorkspace.Protocol.Ownership;

public static class RkwpLeaseBindingValidator
{
    public static void Validate(RkwpMessage message, CarryLease lease, FrameSession? frameSession = null)
    {
        if (!string.Equals(message.SessionId, lease.SessionId, StringComparison.Ordinal))
        {
            throw new RkwpSecurityException("Lease binding violation: message SessionId does not match lease SessionId.");
        }

        if (!string.Equals(message.LeaseId, lease.LeaseId, StringComparison.Ordinal))
        {
            throw new RkwpSecurityException("Lease binding violation: message LeaseId does not match lease LeaseId.");
        }

        if (frameSession is not null &&
            (!string.Equals(frameSession.SessionId, lease.SessionId, StringComparison.Ordinal) ||
             !string.Equals(frameSession.LeaseId, lease.LeaseId, StringComparison.Ordinal)))
        {
            throw new RkwpSecurityException("Lease binding violation: frame session is not bound to the lease.");
        }
    }
}
