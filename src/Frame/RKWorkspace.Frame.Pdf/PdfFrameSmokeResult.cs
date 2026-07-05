using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameSmokeResult(
    PdfFrameDocument Document,
    ThingOwnership Ownership,
    CarryLease Lease,
    FrameSession FrameSession,
    FrameUpdate FirstFrameUpdate,
    PdfGuestFrame GuestFrame,
    CarryLease ReturnedLease,
    CarryLeaseRecovery Recovery)
{
    public bool OwnerStillOwnsOriginal =>
        Ownership.OwnerAblageId == Lease.OwnerAblageId &&
        Ownership.State == OwnershipState.LockedOnOwner &&
        Ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;

    public bool GuestHasNoFileIngress =>
        !FirstFrameUpdate.ContainsOriginalFileBytes &&
        !GuestFrame.ContainsOriginalFileBytes &&
        !GuestFrame.HasOriginalFilePath;

    public bool IsSuccessful =>
        OwnerStillOwnsOriginal &&
        GuestHasNoFileIngress &&
        Lease.State == CarryLeaseState.Active &&
        FrameSession.State == FrameSessionState.Active &&
        ReturnedLease.State == CarryLeaseState.Returned &&
        Recovery.OwnerRecoveredThing &&
        Recovery.GuestFrameInvalidated;
}
