namespace RKWorkspace.Protocol.Ownership;

public enum OwnershipState
{
    OriginalOwned,
    LeasedToGuest,
    LockedOnOwner,
    PresentedOnGuest,
    InteractiveOnGuest,
    ReturningToOwner,
    ReturnedToOwner,
    OwnershipTransferRequested,
    OwnershipTransferred,
    LeaseExpired,
    LeaseRevoked,
    ConnectionLost,
    RecoveredByOwner,
    Lost
}

public enum OwnershipMode
{
    FrameOnly,
    InteractiveFrame,
    ExtractOnly,
    CopyOut,
    ForkVersion,
    MoveOwnership,
    SnapshotExport,
    SessionHandoff,
    NotTransferable
}

public enum OriginalDisposition
{
    RetainOriginal,
    DeleteOriginal,
    MarkAsMoved,
    CreateTombstone,
    CreateVersionLink,
    KeepReadOnlyArchive,
    RequireManualCleanup
}

public enum OwnershipTransferDecisionKind
{
    Approved,
    Denied,
    RequiresUserConfirmation,
    NotSupported
}

public sealed class OwnershipException : Exception
{
    public OwnershipException(string message)
        : base(message)
    {
    }
}

[Flags]
public enum RkwpAllowedAction
{
    None = 0,
    View = 1 << 0,
    Scroll = 1 << 1,
    Zoom = 1 << 2,
    Annotate = 1 << 3,
    Input = 1 << 4,
    ExtractText = 1 << 5,
    ExtractImage = 1 << 6,
    CopyOut = 1 << 7,
    ForkVersion = 1 << 8,
    MoveOwnership = 1 << 9,
    SnapshotExport = 1 << 10,
    SessionHandoff = 1 << 11,
    Return = 1 << 12,
    Revoke = 1 << 13
}

public sealed record ThingOwnership(
    string ThingId,
    string OwnerAblageId,
    string? GuestAblageId,
    OwnershipState State,
    OwnershipMode Mode,
    OriginalDisposition OriginalDisposition)
{
    public static ThingOwnership Original(string thingId, string ownerAblageId)
    {
        return new ThingOwnership(
            thingId,
            ownerAblageId,
            null,
            OwnershipState.OriginalOwned,
            OwnershipMode.FrameOnly,
            OriginalDisposition.RetainOriginal);
    }

    public ThingOwnership LeaseToGuest(string guestAblageId, OwnershipMode mode)
    {
        if (State != OwnershipState.OriginalOwned)
        {
            throw new OwnershipException("Only an original-owned thing can be leased.");
        }

        return this with
        {
            GuestAblageId = guestAblageId,
            State = OwnershipState.LockedOnOwner,
            Mode = mode
        };
    }

    public ThingOwnership ReturnToOwner()
    {
        return this with
        {
            GuestAblageId = null,
            State = OwnershipState.ReturnedToOwner,
            Mode = OwnershipMode.FrameOnly
        };
    }
}

public sealed record OwnershipPolicy(
    string PolicyId,
    OwnershipMode DefaultMode,
    RkwpAllowedAction AllowedActions,
    bool NoFileIngress,
    bool OwnershipTransferAllowed,
    bool Revocable,
    bool AuditRequired,
    bool EncryptedRequired)
{
    public static OwnershipPolicy CriticalDefault { get; } = new(
        "policy-critical-frameonly",
        OwnershipMode.FrameOnly,
        RkwpAllowedAction.View | RkwpAllowedAction.Scroll | RkwpAllowedAction.Zoom | RkwpAllowedAction.Return | RkwpAllowedAction.Revoke,
        NoFileIngress: true,
        OwnershipTransferAllowed: false,
        Revocable: true,
        AuditRequired: true,
        EncryptedRequired: true);

    public bool Allows(RkwpAllowedAction action) => (AllowedActions & action) == action;
}

public sealed record OwnershipTransferRequest(
    string RequestId,
    string ThingId,
    string OwnerAblageId,
    string GuestAblageId,
    OwnershipMode RequestedMode,
    DateTimeOffset RequestedAt);

public sealed record OwnershipTransferDecision(
    string RequestId,
    OwnershipTransferDecisionKind Decision,
    OriginalDisposition OriginalDisposition,
    string Reason);

public sealed record OwnershipTransferResult(
    string RequestId,
    bool OwnershipChanged,
    string? NewThingId,
    OriginalDisposition OriginalDisposition,
    string Message);

public sealed record MaterializationResult(
    bool CreatedGuestThing,
    string? GuestThingId,
    bool OriginalFileIngress,
    string MaterializationMode);

public static class OwnershipTransferService
{
    public static OwnershipTransferDecision Decide(ObjectKind objectKind, OwnershipTransferRequest request, OwnershipPolicy policy)
    {
        if (objectKind == ObjectKind.SettingsWindow)
        {
            return new OwnershipTransferDecision(request.RequestId, OwnershipTransferDecisionKind.NotSupported, OriginalDisposition.RetainOriginal, "Settings windows are not transferable.");
        }

        if (!policy.OwnershipTransferAllowed && request.RequestedMode is OwnershipMode.CopyOut or OwnershipMode.ForkVersion or OwnershipMode.MoveOwnership)
        {
            return new OwnershipTransferDecision(request.RequestId, OwnershipTransferDecisionKind.RequiresUserConfirmation, OriginalDisposition.RetainOriginal, "Ownership transfer requires explicit confirmation.");
        }

        if (!policy.Allows(ToAction(request.RequestedMode)))
        {
            return new OwnershipTransferDecision(request.RequestId, OwnershipTransferDecisionKind.Denied, OriginalDisposition.RetainOriginal, "Policy does not allow requested ownership mode.");
        }

        return new OwnershipTransferDecision(request.RequestId, OwnershipTransferDecisionKind.Approved, OriginalDisposition.RetainOriginal, "Policy allows requested ownership mode.");
    }

    public static OwnershipTransferResult Materialize(OwnershipTransferRequest request, OwnershipTransferDecision decision)
    {
        if (decision.Decision is OwnershipTransferDecisionKind.Denied or OwnershipTransferDecisionKind.NotSupported)
        {
            return new OwnershipTransferResult(request.RequestId, false, null, decision.OriginalDisposition, decision.Reason);
        }

        return new OwnershipTransferResult(
            request.RequestId,
            request.RequestedMode == OwnershipMode.MoveOwnership,
            $"thing-guest-{Guid.NewGuid():N}",
            decision.OriginalDisposition,
            "Guest thing materialized by policy decision.");
    }

    private static RkwpAllowedAction ToAction(OwnershipMode mode)
    {
        return mode switch
        {
            OwnershipMode.CopyOut => RkwpAllowedAction.CopyOut,
            OwnershipMode.ForkVersion => RkwpAllowedAction.ForkVersion,
            OwnershipMode.MoveOwnership => RkwpAllowedAction.MoveOwnership,
            OwnershipMode.SnapshotExport => RkwpAllowedAction.SnapshotExport,
            OwnershipMode.SessionHandoff => RkwpAllowedAction.SessionHandoff,
            _ => RkwpAllowedAction.View
        };
    }
}
