namespace RKWorkspace.Protocol.Ownership;

public enum ChangeSetOperationKind
{
    AnnotationAdded,
    AnnotationRemoved,
    TextExtracted,
    TextInserted,
    FieldChanged,
    SnapshotTaken,
    MetadataChanged,
    Unknown
}

public enum ChangeSetState
{
    Draft,
    Submitted,
    Accepted,
    Rejected,
    Conflict,
    Applied,
    Discarded
}

public enum ChangeSetDecision
{
    Accept,
    Reject,
    RequireReview,
    CreateNewVersion,
    ApplyToOriginal,
    ForkVersion
}

public enum ChangeSetConflictKind
{
    BaseVersionMismatch,
    OwnerChangedDuringLease,
    GuestSubmittedExpiredLease,
    PolicyChanged,
    UnsupportedOperation,
    Unknown
}

public sealed class ChangeSetException : Exception
{
    public ChangeSetException(string message)
        : base(message)
    {
    }
}

public sealed record OriginReference(
    string ThingId,
    string OwnerAblageId,
    string SourceKind,
    string? SourceLocation,
    string? VersionId);

public sealed record VersionReference(
    string VersionId,
    string ThingId,
    string OwnerAblageId,
    string BaseVersionId,
    DateTimeOffset CreatedAt,
    string Reason);

public sealed record ChangeSetOperation(
    string OperationId,
    ChangeSetOperationKind OperationKind,
    string Description,
    int? PageNumber,
    string? Payload,
    DateTimeOffset CreatedAt);

public sealed record ChangeSetConflict(
    ChangeSetConflictKind ConflictKind,
    string Message,
    string? Expected,
    string? Actual);

public sealed record ChangeSetPolicy(
    string PolicyId,
    int PolicyVersion,
    bool AllowChangeSets,
    bool AllowAnnotation,
    bool AllowTextInsertion,
    bool AllowSnapshot,
    bool RequireOwnerDecision,
    string? PolicyHash = null)
{
    public static ChangeSetPolicy ViewOnly { get; } = new(
        "policy-changeset-viewonly",
        1,
        AllowChangeSets: false,
        AllowAnnotation: false,
        AllowTextInsertion: false,
        AllowSnapshot: false,
        RequireOwnerDecision: true);

    public static ChangeSetPolicy Annotate { get; } = new(
        "policy-changeset-annotate",
        1,
        AllowChangeSets: true,
        AllowAnnotation: true,
        AllowTextInsertion: false,
        AllowSnapshot: false,
        RequireOwnerDecision: true);
}

public sealed record ChangeSet
{
    public required string ChangeSetId { get; init; }

    public required string ThingId { get; init; }

    public required string LeaseId { get; init; }

    public required string FrameSessionId { get; init; }

    public required string OwnerAblageId { get; init; }

    public required string GuestAblageId { get; init; }

    public required string BaseVersionId { get; init; }

    public required IReadOnlyList<ChangeSetOperation> Operations { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? SubmittedAt { get; init; }

    public required ChangeSetState State { get; init; }

    public required string PolicyId { get; init; }

    public required int PolicyVersion { get; init; }

    public string? PolicyHash { get; init; }

    public OriginReference Origin { get; init; } = new(string.Empty, string.Empty, string.Empty, null, null);

    public IReadOnlyList<ChangeSetConflict> Conflicts { get; init; } = Array.Empty<ChangeSetConflict>();

    public ChangeSet Submit(DateTimeOffset now)
    {
        if (State != ChangeSetState.Draft)
        {
            throw new ChangeSetException("Only a draft changeset can be submitted.");
        }

        return this with { State = ChangeSetState.Submitted, SubmittedAt = now };
    }
}

public sealed record ChangeSetDecisionResult(
    string DecisionId,
    string ChangeSetId,
    ChangeSetDecision Decision,
    ChangeSetState ResultingState,
    bool OriginalChanged,
    VersionReference? VersionReference,
    ChangeSetConflict? Conflict,
    DateTimeOffset DecidedAt,
    string Message);

public static class ChangeSetService
{
    public static ChangeSet Create(
        CarryLease? lease,
        FrameSession? frameSession,
        ChangeSetPolicy policy,
        string baseVersionId,
        IReadOnlyList<ChangeSetOperation> operations,
        DateTimeOffset now)
    {
        if (lease is null)
        {
            throw new ChangeSetException("A ChangeSet requires an active CarryLease.");
        }

        if (frameSession is null)
        {
            throw new ChangeSetException("A ChangeSet requires an active FrameSession.");
        }

        if (lease.State != CarryLeaseState.Active)
        {
            throw new ChangeSetException("A ChangeSet requires an active CarryLease.");
        }

        if (frameSession.FrameMode == FrameMode.ViewOnly || !policy.AllowChangeSets)
        {
            throw new ChangeSetException("The active frame policy does not allow ChangeSets.");
        }

        if (!string.Equals(lease.LeaseId, frameSession.LeaseId, StringComparison.Ordinal))
        {
            throw new ChangeSetException("ChangeSet lease and frame session are not bound.");
        }

        if (operations.Count == 0)
        {
            throw new ChangeSetException("A ChangeSet requires at least one operation.");
        }

        foreach (var operation in operations)
        {
            ValidateOperation(operation, policy);
        }

        return new ChangeSet
        {
            ChangeSetId = $"changeset-{Guid.NewGuid():N}",
            ThingId = lease.ThingId,
            LeaseId = lease.LeaseId,
            FrameSessionId = frameSession.FrameSessionId,
            OwnerAblageId = lease.OwnerAblageId,
            GuestAblageId = lease.GuestAblageId,
            BaseVersionId = baseVersionId,
            Operations = operations,
            CreatedAt = now,
            State = ChangeSetState.Draft,
            PolicyId = policy.PolicyId,
            PolicyVersion = policy.PolicyVersion,
            PolicyHash = policy.PolicyHash,
            Origin = new OriginReference(lease.ThingId, lease.OwnerAblageId, "FrameSession", frameSession.FrameSessionId, baseVersionId)
        };
    }

    public static ChangeSetDecisionResult Decide(
        ChangeSet changeSet,
        ChangeSetDecision decision,
        ChangeSetPolicy currentPolicy,
        DateTimeOffset now)
    {
        if (!PolicyMatches(changeSet, currentPolicy))
        {
            var conflict = new ChangeSetConflict(
                ChangeSetConflictKind.PolicyChanged,
                "ChangeSet policy changed after creation.",
                $"{changeSet.PolicyId}:{changeSet.PolicyVersion}",
                $"{currentPolicy.PolicyId}:{currentPolicy.PolicyVersion}");

            return new ChangeSetDecisionResult(
                $"changeset-decision-{Guid.NewGuid():N}",
                changeSet.ChangeSetId,
                ChangeSetDecision.RequireReview,
                ChangeSetState.Conflict,
                OriginalChanged: false,
                VersionReference: null,
                conflict,
                now,
                "Owner review required because policy changed.");
        }

        return decision switch
        {
            ChangeSetDecision.Accept => Result(changeSet, decision, ChangeSetState.Accepted, false, null, null, now, "Owner accepted ChangeSet for later application."),
            ChangeSetDecision.Reject => Result(changeSet, decision, ChangeSetState.Rejected, false, null, null, now, "Owner rejected ChangeSet. Original remains unchanged."),
            ChangeSetDecision.RequireReview => Result(changeSet, decision, ChangeSetState.Conflict, false, new ChangeSetConflict(ChangeSetConflictKind.Unknown, "Owner requires manual review.", null, null), null, now, "Manual owner review required."),
            ChangeSetDecision.ApplyToOriginal => Result(changeSet, decision, ChangeSetState.Applied, true, null, null, now, "Owner applied ChangeSet to original."),
            ChangeSetDecision.CreateNewVersion => Result(changeSet, decision, ChangeSetState.Applied, false, null, CreateVersion(changeSet, now, "new-version"), now, "Owner created a new version."),
            ChangeSetDecision.ForkVersion => Result(changeSet, decision, ChangeSetState.Applied, false, null, CreateVersion(changeSet, now, "fork-version"), now, "Owner forked a new version."),
            _ => Result(changeSet, ChangeSetDecision.RequireReview, ChangeSetState.Conflict, false, new ChangeSetConflict(ChangeSetConflictKind.Unknown, "Unknown ChangeSet decision.", null, null), null, now, "Unknown decision.")
        };
    }

    public static ChangeSetDecisionResult RejectConflict(ChangeSet changeSet, ChangeSetConflictKind conflictKind, DateTimeOffset now)
    {
        var conflict = new ChangeSetConflict(conflictKind, "ChangeSet rejected by conflict rule.", null, null);
        return Result(changeSet, ChangeSetDecision.Reject, ChangeSetState.Rejected, false, conflict, null, now, "ChangeSet rejected by conflict rule.");
    }

    private static void ValidateOperation(ChangeSetOperation operation, ChangeSetPolicy policy)
    {
        switch (operation.OperationKind)
        {
            case ChangeSetOperationKind.AnnotationAdded:
            case ChangeSetOperationKind.AnnotationRemoved:
                if (!policy.AllowAnnotation)
                {
                    throw new ChangeSetException("Annotation operations are not allowed by policy.");
                }

                break;
            case ChangeSetOperationKind.TextInserted:
            case ChangeSetOperationKind.FieldChanged:
                if (!policy.AllowTextInsertion)
                {
                    throw new ChangeSetException("Text insertion or field changes are not allowed by policy.");
                }

                break;
            case ChangeSetOperationKind.SnapshotTaken:
                if (!policy.AllowSnapshot)
                {
                    throw new ChangeSetException("Snapshot operations are not allowed by policy.");
                }

                break;
            case ChangeSetOperationKind.TextExtracted:
            case ChangeSetOperationKind.MetadataChanged:
                break;
            case ChangeSetOperationKind.Unknown:
            default:
                throw new ChangeSetException("Unsupported ChangeSet operation.");
        }
    }

    private static bool PolicyMatches(ChangeSet changeSet, ChangeSetPolicy currentPolicy)
    {
        return string.Equals(changeSet.PolicyId, currentPolicy.PolicyId, StringComparison.Ordinal) &&
               changeSet.PolicyVersion == currentPolicy.PolicyVersion &&
               string.Equals(changeSet.PolicyHash, currentPolicy.PolicyHash, StringComparison.Ordinal);
    }

    private static VersionReference CreateVersion(ChangeSet changeSet, DateTimeOffset now, string reason)
    {
        return new VersionReference(
            $"version-{Guid.NewGuid():N}",
            changeSet.ThingId,
            changeSet.OwnerAblageId,
            changeSet.BaseVersionId,
            now,
            reason);
    }

    private static ChangeSetDecisionResult Result(
        ChangeSet changeSet,
        ChangeSetDecision decision,
        ChangeSetState resultingState,
        bool originalChanged,
        ChangeSetConflict? conflict,
        VersionReference? versionReference,
        DateTimeOffset now,
        string message)
    {
        return new ChangeSetDecisionResult(
            $"changeset-decision-{Guid.NewGuid():N}",
            changeSet.ChangeSetId,
            decision,
            resultingState,
            originalChanged,
            versionReference,
            conflict,
            now,
            message);
    }
}
