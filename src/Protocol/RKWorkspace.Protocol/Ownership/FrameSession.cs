namespace RKWorkspace.Protocol.Ownership;

public enum FrameSessionState
{
    Opening,
    Ready,
    Active,
    Paused,
    Returning,
    Closed,
    Revoked,
    Expired,
    Failed
}

public enum FrameMode
{
    ViewOnly,
    Interactive,
    Annotate,
    ExtractAllowed,
    ReadOnlyPresentation
}

public enum FramePermission
{
    View,
    Scroll,
    Zoom,
    Annotate,
    Input,
    Extract
}

public sealed class FrameException : Exception
{
    public FrameException(string message)
        : base(message)
    {
    }
}

public sealed record FrameInputEvent(
    string FrameSessionId,
    string InputType,
    IReadOnlyDictionary<string, string> Data,
    DateTimeOffset Timestamp);

public sealed record FrameUpdate(
    string FrameSessionId,
    string ThingId,
    int PageNumber,
    string Representation,
    string ContentHash,
    bool ContainsOriginalFileBytes,
    DateTimeOffset Timestamp);

public sealed record FrameSession
{
    public required string FrameSessionId { get; init; }

    public required string LeaseId { get; init; }

    public required string SessionId { get; init; }

    public required string ThingId { get; init; }

    public required string OwnerAblageId { get; init; }

    public required string GuestAblageId { get; init; }

    public required FrameMode FrameMode { get; init; }

    public required bool InputAllowed { get; init; }

    public required bool EditAllowed { get; init; }

    public required bool ExtractAllowed { get; init; }

    public required bool ReturnRequired { get; init; }

    public required FrameSessionState State { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required string PolicyId { get; init; }

    public required int PolicyVersion { get; init; }

    public string? PolicyHash { get; init; }

    public static FrameSession Open(CarryLease lease, FrameMode mode, DateTimeOffset now)
    {
        return new FrameSession
        {
            FrameSessionId = $"frame-{Guid.NewGuid():N}",
            LeaseId = lease.LeaseId,
            SessionId = lease.SessionId,
            ThingId = lease.ThingId,
            OwnerAblageId = lease.OwnerAblageId,
            GuestAblageId = lease.GuestAblageId,
            FrameMode = mode,
            InputAllowed = mode is FrameMode.Interactive or FrameMode.Annotate,
            EditAllowed = false,
            ExtractAllowed = mode == FrameMode.ExtractAllowed,
            ReturnRequired = true,
            State = FrameSessionState.Opening,
            CreatedAt = now,
            UpdatedAt = now,
            PolicyId = lease.PolicyId,
            PolicyVersion = lease.PolicyVersion,
            PolicyHash = lease.PolicyHash
        };
    }

    public FrameSession Ready(DateTimeOffset now)
    {
        return this with { State = FrameSessionState.Ready, UpdatedAt = now };
    }

    public FrameSession Activate(DateTimeOffset now)
    {
        if (State != FrameSessionState.Ready)
        {
            throw new FrameException("Only a ready frame session can become active.");
        }

        return this with { State = FrameSessionState.Active, UpdatedAt = now };
    }

    public FrameSession Close(DateTimeOffset now)
    {
        return this with { State = FrameSessionState.Closed, UpdatedAt = now };
    }

    public FrameSession Revoke(DateTimeOffset now)
    {
        return this with { State = FrameSessionState.Revoked, UpdatedAt = now };
    }

    public FrameSession Expire(DateTimeOffset now)
    {
        return this with { State = FrameSessionState.Expired, UpdatedAt = now };
    }
}
