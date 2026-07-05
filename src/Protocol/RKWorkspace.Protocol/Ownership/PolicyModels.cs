namespace RKWorkspace.Protocol.Ownership;

public sealed record AblagePolicy(
    string PolicyId,
    int PolicyVersion,
    bool AllowsFrameGuest,
    bool RequiresEncryption,
    bool AuditRequired,
    string? PolicyHash = null)
{
    public bool CanViewFrame() => AllowsFrameGuest;
}

public sealed record ThingPolicy(
    string PolicyId,
    int PolicyVersion,
    ObjectKind ObjectKind,
    OwnershipMode DefaultMode,
    RkwpAllowedAction AllowedActions,
    string? PolicyHash = null)
{
    public bool Allows(RkwpAllowedAction action) => (AllowedActions & action) == action;

    public bool CanGuestView() => Allows(RkwpAllowedAction.View);

    public bool CanGuestInteract() => Allows(RkwpAllowedAction.Input) || Allows(RkwpAllowedAction.Annotate);

    public bool CanGuestExtract() => Allows(RkwpAllowedAction.ExtractText) || Allows(RkwpAllowedAction.ExtractImage);

    public bool CanGuestRequestCopyOut() => Allows(RkwpAllowedAction.CopyOut);

    public bool CanGuestRequestMoveOwnership() => Allows(RkwpAllowedAction.MoveOwnership);

    public bool CanGuestRequestSnapshotExport() => Allows(RkwpAllowedAction.SnapshotExport);

    public bool CanGuestRequestSessionHandoff() => Allows(RkwpAllowedAction.SessionHandoff);
}

public sealed record FramePolicy(
    string PolicyId,
    int PolicyVersion,
    FrameMode DefaultFrameMode,
    bool InputAllowed,
    bool EditAllowed,
    bool ExtractAllowed,
    string? PolicyHash = null);

public sealed record ExtractionPolicy(
    string PolicyId,
    int PolicyVersion,
    bool TextAllowed,
    bool ImageAllowed,
    bool FileIngressAllowed,
    string? PolicyHash = null);

public sealed record OwnershipTransferPolicy(
    string PolicyId,
    int PolicyVersion,
    bool CopyOutAllowed,
    bool ForkVersionAllowed,
    bool MoveOwnershipAllowed,
    bool RequiresUserConfirmation,
    string? PolicyHash = null)
{
    public bool Allows(OwnershipMode mode)
    {
        return mode switch
        {
            OwnershipMode.CopyOut => CopyOutAllowed,
            OwnershipMode.ForkVersion => ForkVersionAllowed,
            OwnershipMode.MoveOwnership => MoveOwnershipAllowed,
            _ => false
        };
    }
}
