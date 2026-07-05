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
    bool AllowPointer,
    bool AllowScroll,
    bool AllowZoom,
    bool AllowKeyboard,
    bool AllowTextInput,
    bool AllowAnnotation,
    bool AllowClipboard,
    bool AllowExtract,
    bool AllowSystemShortcuts,
    string? PolicyHash = null)
{
    public static FramePolicy CriticalViewOnly { get; } = new(
        "policy-frame-critical-viewonly",
        1,
        FrameMode.ViewOnly,
        InputAllowed: false,
        EditAllowed: false,
        ExtractAllowed: false,
        AllowPointer: false,
        AllowScroll: false,
        AllowZoom: false,
        AllowKeyboard: false,
        AllowTextInput: false,
        AllowAnnotation: false,
        AllowClipboard: false,
        AllowExtract: false,
        AllowSystemShortcuts: false);

    public static FramePolicy InteractiveView { get; } = new(
        "policy-frame-interactive",
        1,
        FrameMode.Interactive,
        InputAllowed: true,
        EditAllowed: false,
        ExtractAllowed: false,
        AllowPointer: true,
        AllowScroll: true,
        AllowZoom: true,
        AllowKeyboard: false,
        AllowTextInput: false,
        AllowAnnotation: false,
        AllowClipboard: false,
        AllowExtract: false,
        AllowSystemShortcuts: false);

    public static FramePolicy Annotate { get; } = new(
        "policy-frame-annotate",
        1,
        FrameMode.Annotate,
        InputAllowed: true,
        EditAllowed: false,
        ExtractAllowed: false,
        AllowPointer: true,
        AllowScroll: true,
        AllowZoom: true,
        AllowKeyboard: false,
        AllowTextInput: false,
        AllowAnnotation: true,
        AllowClipboard: false,
        AllowExtract: false,
        AllowSystemShortcuts: false);
}

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
