using RKWorkspace.Frame.Pdf;
using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Ownership;
using RKWorkspace.Surface.Abstractions;

var root = FindRoot();
var samplePdf = Path.Combine(root, "samples", "Objects", "Rechnung.pdf");
var checks = new List<(string Name, Func<bool> Check)>
{
    ("ProtocolVersion", () => RkwpVersion.Current.Major == 0 && RkwpVersion.Current.Minor == 1),
    ("DevelopmentProtector", DevelopmentProtector),
    ("MessageValidation", MessageValidation),
    ("RequiredFieldsValidation", RequiredFieldsValidation),
    ("EnvelopeFields", EnvelopeFields),
    ("OriginalOwnedDefault", OriginalOwnedDefault),
    ("PlaceDoesNotTransferOwnership", PlaceDoesNotTransferOwnership),
    ("FrameOnlyLease", FrameOnlyLease),
    ("FrameSessionStateMachine", FrameSessionStateMachine),
    ("FrameRevocationInvalidatesGuest", FrameRevocationInvalidatesGuest),
    ("FrameInputViewOnlyRejectsInput", FrameInputViewOnlyRejectsInput),
    ("FrameInputInteractiveAllowsScrollZoom", FrameInputInteractiveAllowsScrollZoom),
    ("FrameInputAnnotateAllowsAnnotation", FrameInputAnnotateAllowsAnnotation),
    ("FrameInputAnnotationDeniedWhenPolicyFalse", FrameInputAnnotationDeniedWhenPolicyFalse),
    ("FrameInputBindingAndSequence", FrameInputBindingAndSequence),
    ("FrameInputKeyboardDeniedWithoutPermission", FrameInputKeyboardDeniedWithoutPermission),
    ("FrameInputPointerWithoutValidSessionDenied", FrameInputPointerWithoutValidSessionDenied),
    ("FrameInputZoomDeniedWithoutPolicy", FrameInputZoomDeniedWithoutPolicy),
    ("ChangeSetCanBeCreated", ChangeSetCanBeCreated),
    ("ChangeSetWithoutLeaseInvalid", ChangeSetWithoutLeaseInvalid),
    ("ChangeSetViewOnlyRejected", ChangeSetViewOnlyRejected),
    ("ChangeSetAnnotateAllowed", ChangeSetAnnotateAllowed),
    ("ChangeSetOwnerAcceptReject", ChangeSetOwnerAcceptReject),
    ("ChangeSetForkVersionCreatesVersionReference", ChangeSetForkVersionCreatesVersionReference),
    ("ChangeSetExpiredLeaseRejected", ChangeSetExpiredLeaseRejected),
    ("ChangeSetUnsupportedOperationRejected", ChangeSetUnsupportedOperationRejected),
    ("ChangeSetPolicyChangedRequiresReview", ChangeSetPolicyChangedRequiresReview),
    ("HeartbeatMissingStartsGrace", HeartbeatMissingStartsGrace),
    ("LeaseRecovery", LeaseRecovery),
    ("OwnershipTransferGuard", OwnershipTransferGuard),
    ("OwnershipTransferDefaultDenies", OwnershipTransferDefaultDenies),
    ("OwnershipTransferPolicyAllowsCopyOut", OwnershipTransferPolicyAllowsCopyOut),
    ("OwnershipTransferPolicyRequiresConfirmation", OwnershipTransferPolicyRequiresConfirmation),
    ("OwnershipTransferSettingsWindowDenies", OwnershipTransferSettingsWindowDenies),
    ("OwnershipTransferRemoteSessionRequiresCapability", OwnershipTransferRemoteSessionRequiresCapability),
    ("OwnershipTransferMaterializationContainsDisposition", OwnershipTransferMaterializationContainsDisposition),
    ("OwnershipTransferMoveOwnershipOnlyWhenApproved", OwnershipTransferMoveOwnershipOnlyWhenApproved),
    ("OwnershipTransferRetainOriginalKeepsOwner", OwnershipTransferRetainOriginalKeepsOwner),
    ("OwnershipTransferMarkAsMoved", OwnershipTransferMarkAsMoved),
    ("OwnershipTransferCreateVersionLink", OwnershipTransferCreateVersionLink),
    ("OwnershipTransferDeniedDoesNotChangeOwnership", OwnershipTransferDeniedDoesNotChangeOwnership),
    ("ObjectKindRules", ObjectKindRuleChecks),
    ("NoFileIngress", () => new PdfFrameOwnerService().OpenFrameOnlySession(samplePdf).GuestHasNoFileIngress),
    ("PdfFrameOnly", () => new PdfFrameOwnerService().OpenFrameOnlySession(samplePdf).IsSuccessful),
    ("SurfaceContracts", SurfaceContracts),
    ("GestureTypes", GestureTypes),
    ("SurfacePlatforms", SurfacePlatforms),
    ("SurfaceDocs", SurfaceDocs),
    ("SecurityModeRequiresProductionProtector", SecurityModeRequiresProductionProtector),
    ("DevelopmentModeMarkedUnsafe", DevelopmentModeMarkedUnsafe),
    ("NonceReplay", NonceReplay),
    ("SequenceReplay", SequenceReplay),
    ("MissingNonceAndSequence", MissingNonceAndSequence),
    ("ValidSequence", ValidSequence),
    ("LeaseBinding", LeaseBinding),
    ("PolicyBinding", PolicyBinding),
    ("AuditEvents", AuditEvents),
    ("Revocation", Revocation),
    ("RecoveryHardening", RecoveryHardening)
};

Console.WriteLine("RK Workspace RKWP Protocol Tests");
foreach (var (name, check) in checks)
{
    if (!check())
    {
        Console.WriteLine($"{name}: FAILED");
        return 1;
    }

    Console.WriteLine($"{name}: PASS");
}

Console.WriteLine("RkwpTests: SUCCESS");
Console.WriteLine("RESULT: SUCCESS");
return 0;

static bool DevelopmentProtector()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1);
    var protector = new DevelopmentRkwpSessionProtector();
    var protectedMessage = protector.Protect(message, session);
    return protectedMessage.AuthTag is not null && protector.Verify(protectedMessage, session);
}

static bool MessageValidation()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.CarryLeaseHeartbeat, session, 2, leaseId: "lease-1");
    return RkwpMessageValidator.Validate(message).Count == 0;
}

static bool RequiredFieldsValidation()
{
    var invalid = new RkwpMessage
    {
        MessageId = string.Empty,
        MessageType = RkwpMessageType.AblageHello,
        ProtocolVersion = RkwpVersion.Current,
        SessionId = string.Empty,
        SourceAblageId = string.Empty,
        TargetAblageId = string.Empty,
        Timestamp = default,
        SequenceNumber = 0,
        Nonce = string.Empty
    };

    var errors = RkwpMessageValidator.Validate(invalid);
    return errors.Any(error => error.Code == "MessageIdMissing") &&
           errors.Any(error => error.Code == "SessionIdMissing") &&
           errors.Any(error => error.Code == "SourceAblageIdMissing") &&
           errors.Any(error => error.Code == "TargetAblageIdMissing") &&
           errors.Any(error => error.Code == "NonceMissing") &&
           errors.Any(error => error.Code == "TimestampMissing") &&
           errors.Any(error => error.Code == "SequenceNumberInvalid");
}

static bool EnvelopeFields()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.FrameSessionOpen, session, 3, leaseId: "lease-2");
    var envelope = new RkwpEnvelope(message);
    return envelope.MessageId == message.MessageId &&
           envelope.MessageType == message.MessageType &&
           envelope.ProtocolVersion == RkwpVersion.Current &&
           envelope.LeaseId == "lease-2" &&
           envelope.Nonce == message.Nonce;
}

static bool OriginalOwnedDefault()
{
    var ownership = ThingOwnership.Original("thing-1", "ablage-owner");
    return ownership.State == OwnershipState.OriginalOwned &&
           ownership.OwnerAblageId == "ablage-owner" &&
           ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool PlaceDoesNotTransferOwnership()
{
    var ownership = ThingOwnership.Original("thing-1", "ablage-owner").LeaseToGuest("ablage-guest", OwnershipMode.FrameOnly);
    return ownership.OwnerAblageId == "ablage-owner" &&
           ownership.GuestAblageId == "ablage-guest" &&
           ownership.State == OwnershipState.LockedOnOwner &&
           ownership.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool FrameOnlyLease()
{
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, DateTimeOffset.UtcNow);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, DateTimeOffset.UtcNow).Ready(DateTimeOffset.UtcNow).Activate(DateTimeOffset.UtcNow);
    return lease.Mode == OwnershipMode.FrameOnly &&
           lease.AllowedActions.HasFlag(RkwpAllowedAction.View) &&
           !frame.EditAllowed &&
           frame.State == FrameSessionState.Active;
}

static bool FrameSessionStateMachine()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var opening = FrameSession.Open(lease, FrameMode.ViewOnly, now);
    var active = opening.Ready(now.AddMilliseconds(1)).Activate(now.AddMilliseconds(2));
    var closed = active.Close(now.AddMilliseconds(3));
    return opening.State == FrameSessionState.Opening &&
           active.State == FrameSessionState.Active &&
           closed.State == FrameSessionState.Closed;
}

static bool FrameRevocationInvalidatesGuest()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now).Ready(now).Activate(now);
    var revokedLease = lease.Revoke(now.AddSeconds(1));
    var revokedFrame = frame.Revoke(now.AddSeconds(1));
    return revokedLease.State == CarryLeaseState.Revoked &&
           revokedFrame.State == FrameSessionState.Revoked;
}

static bool FrameInputViewOnlyRejectsInput()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now).Ready(now).Activate(now);
    var result = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.PointerMove, now), lease, frame, FramePolicy.CriticalViewOnly, audit);
    return !result.Accepted && audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool FrameInputInteractiveAllowsScrollZoom()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Interactive, now).Ready(now).Activate(now);
    var scroll = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Scroll, now), lease, frame, FramePolicy.InteractiveView);
    var zoom = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Zoom, now, deltaScale: 1.2), lease, frame, FramePolicy.InteractiveView);
    return scroll.Accepted && zoom.Accepted;
}

static bool FrameInputAnnotateAllowsAnnotation()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Annotate, now).Ready(now).Activate(now);
    var annotation = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.AnnotationStart, now), lease, frame, FramePolicy.Annotate);
    return annotation.Accepted;
}

static bool FrameInputAnnotationDeniedWhenPolicyFalse()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Annotate, now).Ready(now).Activate(now);
    var annotation = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.AnnotationStart, now), lease, frame, FramePolicy.InteractiveView, audit);
    return !annotation.Accepted && audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool FrameInputBindingAndSequence()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Interactive, now).Ready(now).Activate(now);
    var badSequence = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Scroll, now, sequenceNumber: 0), lease, frame, FramePolicy.InteractiveView);
    var wrongLease = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Scroll, now) with { LeaseId = "lease-wrong" }, lease, frame, FramePolicy.InteractiveView);
    var wrongFrame = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Scroll, now) with { FrameSessionId = "frame-wrong" }, lease, frame, FramePolicy.InteractiveView);
    var valid = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Scroll, now, sequenceNumber: 2), lease, frame, FramePolicy.InteractiveView);
    return !badSequence.Accepted && !wrongLease.Accepted && !wrongFrame.Accepted && valid.Accepted;
}

static bool FrameInputKeyboardDeniedWithoutPermission()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Interactive, now).Ready(now).Activate(now);
    var keyboard = FrameInputValidator.Validate(
        Input(frame, lease, FrameInputType.KeyboardText, now) with { Text = "nicht direkt schreiben" },
        lease,
        frame,
        FramePolicy.InteractiveView,
        audit);
    return !keyboard.Accepted && audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool FrameInputPointerWithoutValidSessionDenied()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Interactive, now).Ready(now).Activate(now);
    var pointer = Input(frame, lease, FrameInputType.PointerMove, now) with { FrameSessionId = "frame-not-active" };
    var result = FrameInputValidator.Validate(pointer, lease, frame, FramePolicy.InteractiveView, audit);
    return !result.Accepted && audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool FrameInputZoomDeniedWithoutPolicy()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, FrameMode.Interactive, now).Ready(now).Activate(now);
    var noZoomPolicy = FramePolicy.InteractiveView with { AllowZoom = false };
    var zoom = FrameInputValidator.Validate(Input(frame, lease, FrameInputType.Zoom, now, deltaScale: 1.1), lease, frame, noZoomPolicy, audit);
    return !zoom.Accepted && audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool ChangeSetCanBeCreated()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var changeSet = ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now);
    return changeSet.State == ChangeSetState.Draft &&
           changeSet.ThingId == lease.ThingId &&
           changeSet.Operations.Count == 1 &&
           changeSet.Origin.OwnerAblageId == lease.OwnerAblageId;
}

static bool ChangeSetWithoutLeaseInvalid()
{
    var now = DateTimeOffset.UtcNow;
    var (_, frame) = ActiveFrame(FrameMode.Annotate, now);
    return Throws<ChangeSetException>(() => ChangeSetService.Create(null, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now));
}

static bool ChangeSetViewOnlyRejected()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.ViewOnly, now);
    return Throws<ChangeSetException>(() => ChangeSetService.Create(lease, frame, ChangeSetPolicy.ViewOnly, "version-1", new[] { AnnotationOperation(now) }, now));
}

static bool ChangeSetAnnotateAllowed()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var changeSet = ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now);
    var submitted = changeSet.Submit(now.AddSeconds(1));
    return submitted.State == ChangeSetState.Submitted &&
           submitted.SubmittedAt == now.AddSeconds(1);
}

static bool ChangeSetOwnerAcceptReject()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var submitted = ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now).Submit(now);
    var accept = ChangeSetService.Decide(submitted, ChangeSetDecision.Accept, ChangeSetPolicy.Annotate, now);
    var reject = ChangeSetService.Decide(submitted, ChangeSetDecision.Reject, ChangeSetPolicy.Annotate, now);
    return accept.ResultingState == ChangeSetState.Accepted &&
           !accept.OriginalChanged &&
           reject.ResultingState == ChangeSetState.Rejected &&
           !reject.OriginalChanged;
}

static bool ChangeSetForkVersionCreatesVersionReference()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var submitted = ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-base", new[] { AnnotationOperation(now) }, now).Submit(now);
    var fork = ChangeSetService.Decide(submitted, ChangeSetDecision.ForkVersion, ChangeSetPolicy.Annotate, now);
    return fork.ResultingState == ChangeSetState.Applied &&
           !fork.OriginalChanged &&
           fork.VersionReference is not null &&
           fork.VersionReference.BaseVersionId == "version-base";
}

static bool ChangeSetExpiredLeaseRejected()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var expired = lease.Advance(now.AddMinutes(16));
    return Throws<ChangeSetException>(() => ChangeSetService.Create(expired, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now));
}

static bool ChangeSetUnsupportedOperationRejected()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var unsupported = new ChangeSetOperation("op-unknown", ChangeSetOperationKind.Unknown, "Unsupported.", 1, null, now);
    return Throws<ChangeSetException>(() => ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-1", new[] { unsupported }, now));
}

static bool ChangeSetPolicyChangedRequiresReview()
{
    var now = DateTimeOffset.UtcNow;
    var (lease, frame) = ActiveFrame(FrameMode.Annotate, now);
    var submitted = ChangeSetService.Create(lease, frame, ChangeSetPolicy.Annotate, "version-1", new[] { AnnotationOperation(now) }, now).Submit(now);
    var changedPolicy = ChangeSetPolicy.Annotate with { PolicyVersion = 2 };
    var decision = ChangeSetService.Decide(submitted, ChangeSetDecision.ApplyToOriginal, changedPolicy, now);
    return decision.Decision == ChangeSetDecision.RequireReview &&
           decision.ResultingState == ChangeSetState.Conflict &&
           decision.Conflict?.ConflictKind == ChangeSetConflictKind.PolicyChanged &&
           !decision.OriginalChanged;
}

static bool HeartbeatMissingStartsGrace()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var missing = lease.Advance(now.AddSeconds(5));
    var grace = missing.Advance(now.AddSeconds(6));
    var heartbeat = grace.Heartbeat(now.AddSeconds(7));
    return missing.State == CarryLeaseState.HeartbeatMissing &&
           grace.State == CarryLeaseState.GracePeriod &&
           heartbeat.State == CarryLeaseState.Active;
}

static bool LeaseRecovery()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var recovered = (lease with { LastHeartbeat = now.AddSeconds(-30) }).Advance(now).Recover();
    return recovered.OwnerRecoveredThing && recovered.GuestFrameInvalidated;
}

static bool OwnershipTransferGuard()
{
    var request = new OwnershipTransferRequest("request-1", "thing-1", "owner", "guest", OwnershipMode.MoveOwnership, DateTimeOffset.UtcNow);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipPolicy.CriticalDefault);
    return decision.Decision == OwnershipTransferDecisionKind.Denied &&
           decision.Denied &&
           !decision.Approved;
}

static bool OwnershipTransferDefaultDenies()
{
    var request = new OwnershipTransferRequest("request-default-deny", "thing-1", "owner", "guest", OwnershipMode.CopyOut, DateTimeOffset.UtcNow);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipPolicy.CriticalDefault);
    return decision.Decision == OwnershipTransferDecisionKind.Denied &&
           decision.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool OwnershipTransferPolicyAllowsCopyOut()
{
    var now = DateTimeOffset.UtcNow;
    var request = new OwnershipTransferRequest("request-copyout", "thing-1", "owner", "guest", OwnershipMode.CopyOut, now);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.CopyOut));
    var materialized = OwnershipTransferService.MaterializeDetailed(request, decision, now);
    return decision.Decision == OwnershipTransferDecisionKind.Approved &&
           decision.Approved &&
           materialized.CreatedGuestThing &&
           materialized.Mode == OwnershipMode.CopyOut &&
           materialized.TargetAblageId == "guest";
}

static bool OwnershipTransferPolicyRequiresConfirmation()
{
    var request = new OwnershipTransferRequest(
        "request-confirm",
        "thing-1",
        "owner",
        "guest",
        OwnershipMode.MoveOwnership,
        DateTimeOffset.UtcNow,
        RequestedDisposition: OriginalDisposition.MarkAsMoved);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.MoveOwnership));
    return decision.Decision == OwnershipTransferDecisionKind.RequiresUserConfirmation &&
           decision.RequiresUserConfirmation &&
           decision.ApprovedMode == OwnershipMode.MoveOwnership;
}

static bool OwnershipTransferSettingsWindowDenies()
{
    var request = new OwnershipTransferRequest("request-settings", "settings-1", "owner", "guest", OwnershipMode.MoveOwnership, DateTimeOffset.UtcNow);
    var decision = OwnershipTransferService.Decide(ObjectKind.SettingsWindow, request, OwnershipTransferPolicy(RkwpAllowedAction.MoveOwnership));
    return decision.Decision == OwnershipTransferDecisionKind.NotSupported &&
           decision.RequiresAdapter &&
           decision.Denied;
}

static bool OwnershipTransferRemoteSessionRequiresCapability()
{
    var now = DateTimeOffset.UtcNow;
    var missing = new OwnershipTransferRequest("request-remote-1", "remote-1", "owner", "guest", OwnershipMode.SessionHandoff, now);
    var denied = OwnershipTransferService.Decide(ObjectKind.RemoteSession, missing, OwnershipTransferPolicy(RkwpAllowedAction.SessionHandoff));
    var supported = missing with { RequestId = "request-remote-2", TargetCapabilities = new HashSet<string> { "SessionHandoff" } };
    var approved = OwnershipTransferService.Decide(ObjectKind.RemoteSession, supported, OwnershipTransferPolicy(RkwpAllowedAction.SessionHandoff));
    return denied.Decision == OwnershipTransferDecisionKind.RequiresAdapter &&
           approved.Decision == OwnershipTransferDecisionKind.Approved;
}

static bool OwnershipTransferMaterializationContainsDisposition()
{
    var now = DateTimeOffset.UtcNow;
    var request = new OwnershipTransferRequest(
        "request-disposition",
        "thing-1",
        "owner",
        "guest",
        OwnershipMode.CopyOut,
        now,
        RequestedDisposition: OriginalDisposition.KeepReadOnlyArchive);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.CopyOut));
    var materialized = OwnershipTransferService.MaterializeDetailed(request, decision, now);
    return materialized.OriginalDisposition == OriginalDisposition.KeepReadOnlyArchive &&
           materialized.MaterializedThingId is not null &&
           materialized.NewOwnerAblageId == "owner";
}

static bool OwnershipTransferMoveOwnershipOnlyWhenApproved()
{
    var now = DateTimeOffset.UtcNow;
    var ownership = ThingOwnership.Original("thing-1", "owner");
    var request = new OwnershipTransferRequest(
        "request-move-approved",
        "thing-1",
        "owner",
        "guest",
        OwnershipMode.MoveOwnership,
        now,
        RequestedDisposition: OriginalDisposition.MarkAsMoved);
    var confirmation = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.MoveOwnership));
    var unchanged = OwnershipTransferService.ApplyApprovedTransfer(ownership, request, confirmation);
    var approved = confirmation with { Decision = OwnershipTransferDecisionKind.Approved, Approved = true, RequiresUserConfirmation = false };
    var moved = OwnershipTransferService.ApplyApprovedTransfer(ownership, request, approved);
    return unchanged.OwnerAblageId == "owner" &&
           moved.OwnerAblageId == "guest" &&
           moved.State == OwnershipState.OwnershipTransferred;
}

static bool OwnershipTransferRetainOriginalKeepsOwner()
{
    var now = DateTimeOffset.UtcNow;
    var ownership = ThingOwnership.Original("thing-1", "owner");
    var request = new OwnershipTransferRequest("request-retain", "thing-1", "owner", "guest", OwnershipMode.CopyOut, now);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.CopyOut));
    var result = OwnershipTransferService.ApplyApprovedTransfer(ownership, request, decision);
    return result.OwnerAblageId == "owner" &&
           result.OriginalDisposition == OriginalDisposition.RetainOriginal;
}

static bool OwnershipTransferMarkAsMoved()
{
    var now = DateTimeOffset.UtcNow;
    var request = new OwnershipTransferRequest(
        "request-mark",
        "thing-1",
        "owner",
        "guest",
        OwnershipMode.CopyOut,
        now,
        RequestedDisposition: OriginalDisposition.MarkAsMoved);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.CopyOut));
    var materialized = OwnershipTransferService.MaterializeDetailed(request, decision, now);
    return decision.OriginalDisposition == OriginalDisposition.MarkAsMoved &&
           materialized.OriginalDisposition == OriginalDisposition.MarkAsMoved;
}

static bool OwnershipTransferCreateVersionLink()
{
    var now = DateTimeOffset.UtcNow;
    var request = new OwnershipTransferRequest(
        "request-version-link",
        "thing-1",
        "owner",
        "guest",
        OwnershipMode.ForkVersion,
        now,
        RequestedDisposition: OriginalDisposition.CreateVersionLink);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipTransferPolicy(RkwpAllowedAction.ForkVersion));
    var materialized = OwnershipTransferService.MaterializeDetailed(request, decision, now);
    return materialized.VersionReference is not null &&
           materialized.OriginalDisposition == OriginalDisposition.CreateVersionLink;
}

static bool OwnershipTransferDeniedDoesNotChangeOwnership()
{
    var now = DateTimeOffset.UtcNow;
    var ownership = ThingOwnership.Original("thing-1", "owner");
    var request = new OwnershipTransferRequest("request-denied", "thing-1", "owner", "guest", OwnershipMode.MoveOwnership, now);
    var decision = OwnershipTransferService.Decide(ObjectKind.PdfDocument, request, OwnershipPolicy.CriticalDefault);
    var result = OwnershipTransferService.ApplyApprovedTransfer(ownership, request, decision);
    var materialized = OwnershipTransferService.MaterializeDetailed(request, decision, now);
    return result.OwnerAblageId == "owner" &&
           result.State == OwnershipState.OriginalOwned &&
           !materialized.CreatedGuestThing;
}

static bool ObjectKindRuleChecks()
{
    var pdf = ObjectKindRules.For(ObjectKind.PdfDocument);
    var settings = ObjectKindRules.For(ObjectKind.SettingsWindow);
    var remote = ObjectKindRules.For(ObjectKind.RemoteSession);
    return pdf.DefaultMode == OwnershipMode.FrameOnly &&
           pdf.AllowsOptionalMode(OwnershipMode.CopyOut) &&
           settings.DefaultMode == OwnershipMode.InteractiveFrame &&
           !settings.OwnershipTransferSupported &&
           settings.AllowsOptionalMode(OwnershipMode.SnapshotExport) &&
           remote.AllowsOptionalMode(OwnershipMode.SessionHandoff);
}

static bool SurfaceContracts()
{
    var placement = new SurfaceFramePlacement("tablet", "right", 0.4, true);
    var identity = new SurfaceIdentity("tablet", SurfacePlatform.IPadOS, "Tablet", "desk");
    var capabilities = SurfaceCapabilities.Overlay | SurfaceCapabilities.GlassEdge | SurfaceCapabilities.FramePresentation;
    return placement.IsNearest &&
           identity.Platform == SurfacePlatform.IPadOS &&
           capabilities.HasFlag(SurfaceCapabilities.FramePresentation);
}

static bool GestureTypes()
{
    var values = Enum.GetValues<GestureType>();
    return values.Contains(GestureType.ThreeFingerHold) &&
           values.Contains(GestureType.LongPress) &&
           values.Contains(GestureType.MouseLongPress) &&
           values.Contains(GestureType.KeyboardActivation) &&
           values.Contains(GestureType.TouchHold) &&
           values.Contains(GestureType.PenHold);
}

static bool SurfacePlatforms()
{
    var values = Enum.GetValues<SurfacePlatform>();
    return values.Contains(SurfacePlatform.Windows) &&
           values.Contains(SurfacePlatform.MacOS) &&
           values.Contains(SurfacePlatform.IOS) &&
           values.Contains(SurfacePlatform.IPadOS) &&
           values.Contains(SurfacePlatform.Android) &&
           values.Contains(SurfacePlatform.Linux);
}

static bool SurfaceDocs()
{
    var root = FindRoot();
    var mac = File.ReadAllText(Path.Combine(root, "Docs", "Codex", "PlatformTasks", "macOS.md"));
    var ios = File.ReadAllText(Path.Combine(root, "Docs", "Codex", "PlatformTasks", "iOS_iPadOS.md"));
    return mac.Contains("Accessibility", StringComparison.OrdinalIgnoreCase) &&
           mac.Contains("Screen Recording", StringComparison.OrdinalIgnoreCase) &&
           mac.Contains("Sandbox", StringComparison.OrdinalIgnoreCase) &&
           ios.Contains("Xcode", StringComparison.OrdinalIgnoreCase) &&
           ios.Contains("USB", StringComparison.OrdinalIgnoreCase);
}

static bool SecurityModeRequiresProductionProtector()
{
    var secureSession = RkwpSession.CreateSecure("owner", "guest");
    var message = RkwpMessage.Create(RkwpMessageType.AblageHello, secureSession, 1);
    var protector = new DevelopmentRkwpSessionProtector();
    var devRejected = Throws<RkwpProtocolException>(() => protector.Protect(message, secureSession));
    var insecureRejected = Throws<RkwpSecurityException>(() => RkwpSession.CreateSecure("owner", "guest", RkwpSecurityMode.DevelopmentInsecure));
    return secureSession.SecurityMode == RkwpSecurityMode.EncryptedAndAuthenticated &&
           secureSession.SecureSessionRequired &&
           devRejected &&
           insecureRejected;
}

static bool DevelopmentModeMarkedUnsafe()
{
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var protector = new DevelopmentRkwpSessionProtector();
    return session.SecurityMode == RkwpSecurityMode.DevelopmentInsecure &&
           protector.IsDevelopmentOnly &&
           protector.SecurityNotice.Contains("no real encryption", StringComparison.OrdinalIgnoreCase);
}

static bool NonceReplay()
{
    var audit = new InMemoryRkwpAuditSink();
    var validator = new RkwpSequenceValidator(audit);
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var first = RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1);
    var replay = RkwpMessage.Create(RkwpMessageType.AblageCapabilities, session, 2) with { Nonce = first.Nonce };
    validator.ValidateAndRecord(first);
    return Throws<RkwpSecurityException>(() => validator.ValidateAndRecord(replay)) &&
           audit.Contains(RkwpAuditEventType.ReplayDetected);
}

static bool SequenceReplay()
{
    var audit = new InMemoryRkwpAuditSink();
    var validator = new RkwpSequenceValidator(audit);
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    validator.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageHello, session, 5));
    return Throws<RkwpSecurityException>(() => validator.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageCapabilities, session, 4))) &&
           audit.Contains(RkwpAuditEventType.ReplayDetected);
}

static bool MissingNonceAndSequence()
{
    var audit = new InMemoryRkwpAuditSink();
    var validator = new RkwpSequenceValidator(audit);
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var missingNonce = RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1) with { Nonce = string.Empty };
    var missingSequence = RkwpMessage.Create(RkwpMessageType.AblageCapabilities, session, 0);
    return Throws<RkwpSecurityException>(() => validator.ValidateAndRecord(missingNonce)) &&
           Throws<RkwpSecurityException>(() => validator.ValidateAndRecord(missingSequence)) &&
           audit.Contains(RkwpAuditEventType.SecurityViolation);
}

static bool ValidSequence()
{
    var validator = new RkwpSequenceValidator();
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    validator.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1));
    validator.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageCapabilities, session, 2));
    validator.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.NearestAblageSelected, session, 3));
    return true;
}

static bool LeaseBinding()
{
    var now = DateTimeOffset.UtcNow;
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    var otherSession = RkwpSession.CreateDevelopment("owner", "guest");
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now, session.SessionId);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now);
    var heartbeat = RkwpMessage.Create(RkwpMessageType.CarryLeaseHeartbeat, session, 1, leaseId: lease.LeaseId);
    var wrongLease = heartbeat with { LeaseId = "lease-wrong" };
    var wrongSession = RkwpMessage.Create(RkwpMessageType.CarryLeaseReturn, otherSession, 2, leaseId: lease.LeaseId);
    var correct = RkwpMessage.Create(RkwpMessageType.CarryLeaseReturn, session, 3, leaseId: lease.LeaseId);

    RkwpLeaseBindingValidator.Validate(heartbeat, lease, frame);
    return Throws<RkwpSecurityException>(() => RkwpLeaseBindingValidator.Validate(wrongLease, lease, frame)) &&
           Throws<RkwpSecurityException>(() => RkwpLeaseBindingValidator.Validate(wrongSession, lease, frame)) &&
           !Throws<RkwpSecurityException>(() => RkwpLeaseBindingValidator.Validate(correct, lease, frame));
}

static bool PolicyBinding()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var binder = new RkwpPolicyBinder(audit);
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now, "session-1");
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now);
    var valid = binder.Validate(lease, frame);
    var changedPolicy = frame with { PolicyVersion = frame.PolicyVersion + 1 };
    var denied = binder.Validate(lease, changedPolicy);
    return valid.IsValid &&
           !denied.IsValid &&
           audit.Contains(RkwpAuditEventType.PolicyDenied);
}

static bool AuditEvents()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.LeaseGranted, "session-1", "lease-1", "thing-1", "Lease granted.", now));
    audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.FrameReturned, "session-1", "lease-1", "thing-1", "Frame returned.", now));

    var sequence = new RkwpSequenceValidator(audit);
    var session = RkwpSession.CreateDevelopment("owner", "guest");
    sequence.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageHello, session, 1));
    _ = Throws<RkwpSecurityException>(() => sequence.ValidateAndRecord(RkwpMessage.Create(RkwpMessageType.AblageCapabilities, session, 1)));

    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now, session.SessionId);
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now) with { PolicyId = "changed" };
    _ = new RkwpPolicyBinder(audit).Validate(lease, frame);
    audit.Write(RkwpAuditEvent.Create(RkwpAuditEventType.RecoveredByOwner, session.SessionId, lease.LeaseId, lease.ThingId, "Owner recovered thing.", now));

    return audit.Contains(RkwpAuditEventType.LeaseGranted) &&
           audit.Contains(RkwpAuditEventType.FrameReturned) &&
           audit.Contains(RkwpAuditEventType.ReplayDetected) &&
           audit.Contains(RkwpAuditEventType.PolicyDenied) &&
           audit.Contains(RkwpAuditEventType.RecoveredByOwner);
}

static bool Revocation()
{
    var now = DateTimeOffset.UtcNow;
    var audit = new InMemoryRkwpAuditSink();
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now, "session-1");
    var frame = FrameSession.Open(lease, FrameMode.ViewOnly, now).Ready(now).Activate(now);
    var request = new RkwpRevocationRequest("revoke-1", lease.SessionId, lease.LeaseId, RkwpRevocationReason.OwnerRequested, now.AddSeconds(1));
    var result = RkwpRevocationService.Revoke(request, lease, frame, audit);
    var invalid = new RkwpRevocationRequest("revoke-2", "wrong-session", lease.LeaseId, RkwpRevocationReason.SecurityViolation, now.AddSeconds(2));
    return result.Lease.State == CarryLeaseState.Revoked &&
           result.FrameSession.State == FrameSessionState.Revoked &&
           result.GuestFrameInvalid &&
           result.OwnerUnlockedThing &&
           audit.Contains(RkwpAuditEventType.LeaseRevoked) &&
           Throws<RkwpSecurityException>(() => RkwpRevocationService.Revoke(invalid, lease, frame, audit));
}

static bool RecoveryHardening()
{
    var now = DateTimeOffset.UtcNow;
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now, "session-1");
    var heartbeatLost = (lease with { LastHeartbeat = now.AddSeconds(-30) }).Advance(now).Recover();
    var expired = lease.Advance(now.AddMinutes(16)).Recover();
    var connectionLost = lease.MarkConnectionLost(now.AddSeconds(1)).Recover();
    var returned = lease.Return(now.AddSeconds(2)).Recover();

    return heartbeatLost.Reason == CarryLeaseRecoveryReason.RecoveredByOwner &&
           heartbeatLost.FinalState == CarryLeaseState.RecoveredByOwner &&
           expired.Reason == CarryLeaseRecoveryReason.LeaseExpired &&
           expired.FinalState == CarryLeaseState.Expired &&
           connectionLost.Reason == CarryLeaseRecoveryReason.ConnectionLost &&
           connectionLost.FinalState == CarryLeaseState.RecoveredByOwner &&
           returned.Reason == CarryLeaseRecoveryReason.Returned &&
           returned.FinalState == CarryLeaseState.Returned;
}

static (CarryLease Lease, FrameSession Frame) ActiveFrame(FrameMode mode, DateTimeOffset now)
{
    var lease = CarryLease.Grant("thing-1", "owner", "guest", CarryLeasePolicy.FrameOnlyDefault, now);
    var frame = FrameSession.Open(lease, mode, now).Ready(now).Activate(now);
    return (lease, frame);
}

static ChangeSetOperation AnnotationOperation(DateTimeOffset now)
{
    return new ChangeSetOperation(
        "operation-annotation-1",
        ChangeSetOperationKind.AnnotationAdded,
        "Annotation added in guest frame.",
        1,
        "{ \"x\": 0.5, \"y\": 0.5, \"text\": \"review\" }",
        now);
}

static OwnershipPolicy OwnershipTransferPolicy(RkwpAllowedAction actions, bool requiresUserConfirmation = false)
{
    return OwnershipPolicy.CriticalDefault with
    {
        PolicyId = "policy-transfer-test",
        AllowedActions = actions,
        OwnershipTransferAllowed = true,
        RequiresUserConfirmation = requiresUserConfirmation
    };
}

static FrameInputEvent Input(
    FrameSession frame,
    CarryLease lease,
    FrameInputType inputType,
    DateTimeOffset timestamp,
    long sequenceNumber = 1,
    double deltaScale = 1.0)
{
    return new FrameInputEvent(
        $"input-{Guid.NewGuid():N}",
        frame.FrameSessionId,
        lease.LeaseId,
        lease.GuestAblageId,
        lease.OwnerAblageId,
        sequenceNumber,
        inputType,
        new FrameCoordinate(0.5, 0.5),
        new FrameDelta(0.0, 120.0, deltaScale),
        null,
        Array.Empty<string>(),
        null,
        FramePointerKind.Mouse,
        timestamp);
}

static bool Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
        return false;
    }
    catch (TException)
    {
        return true;
    }
}

static string FindRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
    {
        directory = directory.Parent;
    }

    if (directory is null)
    {
        throw new InvalidOperationException("Repository root could not be located.");
    }

    return directory.FullName;
}
