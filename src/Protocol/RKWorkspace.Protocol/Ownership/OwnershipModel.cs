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
    RequiresPolicyApproval,
    RequiresTransformation,
    RequiresAdapter,
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
    bool EncryptedRequired,
    bool RequiresUserConfirmation = false)
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
    string CurrentOwnerAblageId,
    string RequestedNewOwnerAblageId,
    OwnershipMode RequestedMode,
    DateTimeOffset RequestedAt,
    OriginalDisposition RequestedDisposition = OriginalDisposition.RetainOriginal,
    string RequestedBy = "system",
    string Reason = "not specified",
    IReadOnlySet<string>? TargetCapabilities = null,
    string PolicyId = "policy-unknown")
{
    public string OwnerAblageId => CurrentOwnerAblageId;

    public string GuestAblageId => RequestedNewOwnerAblageId;
}

public sealed record OwnershipTransferDecision(
    string RequestId,
    OwnershipTransferDecisionKind Decision,
    OriginalDisposition OriginalDisposition,
    string Reason,
    string DecisionId = "",
    bool Approved = false,
    bool Denied = false,
    bool RequiresUserConfirmation = false,
    bool RequiresPolicyApproval = false,
    bool RequiresTransformation = false,
    bool RequiresAdapter = false,
    OwnershipMode ApprovedMode = OwnershipMode.NotTransferable,
    DateTimeOffset CreatedAt = default);

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
    string MaterializationMode,
    string? MaterializedThingId = null,
    string? TargetAblageId = null,
    string? TargetLocation = null,
    OwnershipMode Mode = OwnershipMode.NotTransferable,
    string? NewOwnerAblageId = null,
    OriginalDisposition OriginalDisposition = OriginalDisposition.RetainOriginal,
    VersionReference? VersionReference = null,
    DateTimeOffset CreatedAt = default);

public static class OwnershipTransferService
{
    public static OwnershipTransferDecision Decide(ObjectKind objectKind, OwnershipTransferRequest request, OwnershipPolicy policy)
    {
        if (objectKind == ObjectKind.SettingsWindow)
        {
            return Decision(
                request,
                OwnershipTransferDecisionKind.NotSupported,
                OriginalDisposition.RetainOriginal,
                "Settings windows are not transferable.",
                requiresAdapter: true);
        }

        if (objectKind == ObjectKind.RemoteSession &&
            request.RequestedMode == OwnershipMode.SessionHandoff &&
            !HasCapability(request, "SessionHandoff"))
        {
            return Decision(
                request,
                OwnershipTransferDecisionKind.RequiresAdapter,
                OriginalDisposition.RetainOriginal,
                "Remote session handoff requires target capability SessionHandoff.",
                requiresAdapter: true);
        }

        if (!policy.OwnershipTransferAllowed && request.RequestedMode is OwnershipMode.CopyOut or OwnershipMode.ForkVersion or OwnershipMode.MoveOwnership or OwnershipMode.SnapshotExport or OwnershipMode.SessionHandoff)
        {
            return Decision(
                request,
                OwnershipTransferDecisionKind.Denied,
                OriginalDisposition.RetainOriginal,
                "Ownership transfer is denied unless explicitly allowed by policy.");
        }

        if (!policy.Allows(ToAction(request.RequestedMode)))
        {
            return Decision(
                request,
                OwnershipTransferDecisionKind.Denied,
                OriginalDisposition.RetainOriginal,
                "Policy does not allow requested ownership mode.");
        }

        if (policy.RequiresUserConfirmation || request.RequestedMode == OwnershipMode.MoveOwnership)
        {
            return Decision(
                request,
                OwnershipTransferDecisionKind.RequiresUserConfirmation,
                request.RequestedDisposition,
                "Ownership transfer requires explicit confirmation.",
                requiresUserConfirmation: true,
                approvedMode: request.RequestedMode);
        }

        return Decision(
            request,
            OwnershipTransferDecisionKind.Approved,
            request.RequestedDisposition,
            "Policy allows requested ownership mode.",
            approved: true,
            approvedMode: request.RequestedMode);
    }

    public static OwnershipTransferResult Materialize(OwnershipTransferRequest request, OwnershipTransferDecision decision)
    {
        if (decision.Decision != OwnershipTransferDecisionKind.Approved)
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

    public static MaterializationResult MaterializeDetailed(OwnershipTransferRequest request, OwnershipTransferDecision decision, DateTimeOffset now)
    {
        if (decision.Decision != OwnershipTransferDecisionKind.Approved)
        {
            return new MaterializationResult(
                CreatedGuestThing: false,
                GuestThingId: null,
                OriginalFileIngress: false,
                MaterializationMode: "NotMaterialized",
                MaterializedThingId: null,
                TargetAblageId: request.RequestedNewOwnerAblageId,
                TargetLocation: null,
                Mode: request.RequestedMode,
                NewOwnerAblageId: null,
                decision.OriginalDisposition,
                VersionReference: null,
                CreatedAt: now);
        }

        var materializedThingId = request.RequestedMode switch
        {
            OwnershipMode.CopyOut => $"copy-{request.ThingId}-{Guid.NewGuid():N}",
            OwnershipMode.ForkVersion => $"fork-{request.ThingId}-{Guid.NewGuid():N}",
            OwnershipMode.SnapshotExport => $"snapshot-{request.ThingId}-{Guid.NewGuid():N}",
            _ => request.ThingId
        };

        var versionReference = decision.OriginalDisposition == OriginalDisposition.CreateVersionLink ||
                               request.RequestedMode == OwnershipMode.ForkVersion
            ? new VersionReference($"version-{Guid.NewGuid():N}", request.ThingId, request.CurrentOwnerAblageId, "current", now, request.RequestedMode.ToString())
            : null;

        return new MaterializationResult(
            CreatedGuestThing: request.RequestedMode != OwnershipMode.SessionHandoff,
            GuestThingId: materializedThingId,
            OriginalFileIngress: request.RequestedMode is OwnershipMode.CopyOut or OwnershipMode.ForkVersion or OwnershipMode.SnapshotExport,
            MaterializationMode: request.RequestedMode.ToString(),
            MaterializedThingId: materializedThingId,
            TargetAblageId: request.RequestedNewOwnerAblageId,
            TargetLocation: "policy-controlled",
            Mode: request.RequestedMode,
            NewOwnerAblageId: request.RequestedMode == OwnershipMode.MoveOwnership ? request.RequestedNewOwnerAblageId : request.CurrentOwnerAblageId,
            decision.OriginalDisposition,
            versionReference,
            now);
    }

    public static ThingOwnership ApplyApprovedTransfer(ThingOwnership ownership, OwnershipTransferRequest request, OwnershipTransferDecision decision)
    {
        if (decision.Decision != OwnershipTransferDecisionKind.Approved)
        {
            return ownership;
        }

        if (request.RequestedMode != OwnershipMode.MoveOwnership)
        {
            return ownership with { OriginalDisposition = decision.OriginalDisposition };
        }

        return ownership with
        {
            OwnerAblageId = request.RequestedNewOwnerAblageId,
            GuestAblageId = null,
            State = OwnershipState.OwnershipTransferred,
            Mode = OwnershipMode.MoveOwnership,
            OriginalDisposition = decision.OriginalDisposition
        };
    }

    private static OwnershipTransferDecision Decision(
        OwnershipTransferRequest request,
        OwnershipTransferDecisionKind decisionKind,
        OriginalDisposition originalDisposition,
        string reason,
        bool approved = false,
        bool requiresUserConfirmation = false,
        bool requiresPolicyApproval = false,
        bool requiresTransformation = false,
        bool requiresAdapter = false,
        OwnershipMode approvedMode = OwnershipMode.NotTransferable)
    {
        return new OwnershipTransferDecision(
            request.RequestId,
            decisionKind,
            originalDisposition,
            reason,
            $"ownership-decision-{Guid.NewGuid():N}",
            approved,
            decisionKind is OwnershipTransferDecisionKind.Denied or OwnershipTransferDecisionKind.NotSupported,
            requiresUserConfirmation,
            requiresPolicyApproval,
            requiresTransformation,
            requiresAdapter,
            approvedMode,
            request.RequestedAt);
    }

    private static bool HasCapability(OwnershipTransferRequest request, string capability)
    {
        return request.TargetCapabilities?.Contains(capability) == true;
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
