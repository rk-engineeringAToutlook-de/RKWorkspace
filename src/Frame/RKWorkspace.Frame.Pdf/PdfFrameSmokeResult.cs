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

    public bool GuestShowsFrameRepresentation =>
        !string.IsNullOrWhiteSpace(GuestFrame.DisplayText) &&
        GuestFrame.PageCount > 0 &&
        GuestFrame.RepresentationKind is PdfFrameRepresentationKind.MetadataPreview or
            PdfFrameRepresentationKind.RenderedFirstPage or
            PdfFrameRepresentationKind.RenderedPageImage;

    public IReadOnlyList<VisibleFrameState> VisibleStates =>
        OwnerGuestFrameStateUx.CreateTimeline(Ownership, Lease, FrameSession, ReturnedLease, Recovery);

    public string OwnerVisibleStatus =>
        OwnerGuestFrameStateUx.GetOwnerText(OwnerGuestFrameStateUx.GetOwnerState(Ownership, Lease));

    public string GuestVisibleStatus =>
        OwnerGuestFrameStateUx.GetGuestText(OwnerGuestFrameStateUx.GetGuestState(FrameSession));

    public bool VisibleStateLanguageIsValid =>
        OwnerGuestFrameStateUx.ValidateVisibleText(VisibleStates.Select(state => state.Text)).IsValid;

    public bool IsSuccessful =>
        OwnerStillOwnsOriginal &&
        GuestHasNoFileIngress &&
        GuestShowsFrameRepresentation &&
        Lease.State == CarryLeaseState.Active &&
        FrameSession.State == FrameSessionState.Active &&
        ReturnedLease.State == CarryLeaseState.Returned &&
        Recovery.OwnerRecoveredThing &&
        Recovery.GuestFrameInvalidated &&
        VisibleStateLanguageIsValid;
}
