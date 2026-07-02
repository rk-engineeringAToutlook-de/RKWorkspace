namespace RKWorkspace.Core.Models;

public enum TransferStatus
{
    Created,
    ObjectSelected,
    GestureActive,
    TargetSearch,
    TargetLocked,
    TransferPending,
    TransferRunning,
    TransferVerified,
    Completed,
    Cancelled,
    Failed,
    Timeout,
    Rejected,
    Retry,
    Rollback,
    Archived
}
