namespace RKWorkspace.Protocol;

public enum RkwpAuditEventType
{
    SessionStarted,
    LeaseRequested,
    LeaseGranted,
    LeaseDenied,
    FrameOpened,
    FrameInput,
    FrameReturned,
    LeaseRevoked,
    LeaseExpired,
    RecoveredByOwner,
    OwnershipTransferRequested,
    OwnershipTransferDenied,
    OwnershipTransferApproved,
    SecurityViolation,
    ReplayDetected,
    PolicyDenied
}
