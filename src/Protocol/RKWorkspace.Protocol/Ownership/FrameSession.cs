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

public enum FrameInputType
{
    PointerMove,
    PointerDown,
    PointerUp,
    Tap,
    DoubleTap,
    Scroll,
    Zoom,
    KeyboardText,
    KeyboardCommand,
    AnnotationStart,
    AnnotationUpdate,
    AnnotationEnd
}

public enum FramePointerKind
{
    Unknown,
    Mouse,
    Touch,
    Pen
}

public sealed class FrameException : Exception
{
    public FrameException(string message)
        : base(message)
    {
    }
}

public sealed class FrameInputPolicyException : Exception
{
    public FrameInputPolicyException(string message)
        : base(message)
    {
    }
}

public sealed record FrameCoordinate(double X, double Y);

public sealed record FrameDelta(double X, double Y, double Scale);

public sealed record FrameInputEvent(
    string InputEventId,
    string FrameSessionId,
    string LeaseId,
    string SourceAblageId,
    string TargetAblageId,
    long SequenceNumber,
    FrameInputType InputType,
    FrameCoordinate? Coordinates,
    FrameDelta? Delta,
    string? Text,
    IReadOnlyList<string> Modifiers,
    double? Pressure,
    FramePointerKind PointerKind,
    DateTimeOffset Timestamp);

public sealed record FrameInputValidationResult(
    bool Accepted,
    string Reason,
    FrameInputEvent InputEvent)
{
    public static FrameInputValidationResult Accept(FrameInputEvent inputEvent)
    {
        return new FrameInputValidationResult(true, "Input accepted.", inputEvent);
    }

    public static FrameInputValidationResult Deny(FrameInputEvent inputEvent, string reason)
    {
        return new FrameInputValidationResult(false, reason, inputEvent);
    }
}

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

public static class FrameInputValidator
{
    public static FrameInputValidationResult Validate(
        FrameInputEvent inputEvent,
        CarryLease lease,
        FrameSession frameSession,
        FramePolicy policy,
        IRkwpAuditSink? auditSink = null)
    {
        if (!string.Equals(inputEvent.LeaseId, lease.LeaseId, StringComparison.Ordinal) ||
            !string.Equals(frameSession.LeaseId, lease.LeaseId, StringComparison.Ordinal))
        {
            return Deny(inputEvent, auditSink, lease, "Input LeaseId does not match active lease.");
        }

        if (!string.Equals(inputEvent.FrameSessionId, frameSession.FrameSessionId, StringComparison.Ordinal))
        {
            return Deny(inputEvent, auditSink, lease, "Input FrameSessionId does not match active frame session.");
        }

        if (inputEvent.SequenceNumber <= 0)
        {
            return Deny(inputEvent, auditSink, lease, "Input SequenceNumber must be positive.");
        }

        if (!frameSession.InputAllowed && inputEvent.InputType is not FrameInputType.Scroll and not FrameInputType.Zoom)
        {
            return Deny(inputEvent, auditSink, lease, "Frame session does not allow input.");
        }

        if (!policy.InputAllowed && inputEvent.InputType is not FrameInputType.Scroll and not FrameInputType.Zoom)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow input.");
        }

        if (IsPointer(inputEvent.InputType) && !policy.AllowPointer)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow pointer input.");
        }

        if (inputEvent.InputType == FrameInputType.Scroll && !policy.AllowScroll)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow scroll input.");
        }

        if (inputEvent.InputType == FrameInputType.Zoom && !policy.AllowZoom)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow zoom input.");
        }

        if (IsKeyboard(inputEvent.InputType) && !policy.AllowKeyboard)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow keyboard input.");
        }

        if (inputEvent.InputType == FrameInputType.KeyboardText && (!policy.AllowTextInput || !frameSession.EditAllowed))
        {
            return Deny(inputEvent, auditSink, lease, "Text input is not allowed for this frame.");
        }

        if (IsAnnotation(inputEvent.InputType) && !policy.AllowAnnotation)
        {
            return Deny(inputEvent, auditSink, lease, "Frame policy does not allow annotation input.");
        }

        return FrameInputValidationResult.Accept(inputEvent);
    }

    private static FrameInputValidationResult Deny(FrameInputEvent inputEvent, IRkwpAuditSink? auditSink, CarryLease lease, string reason)
    {
        auditSink?.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.PolicyDenied,
            lease.SessionId,
            lease.LeaseId,
            lease.ThingId,
            reason,
            inputEvent.Timestamp,
            new Dictionary<string, string>
            {
                ["input-event-id"] = inputEvent.InputEventId,
                ["input-type"] = inputEvent.InputType.ToString()
            }));

        return FrameInputValidationResult.Deny(inputEvent, reason);
    }

    private static bool IsPointer(FrameInputType inputType)
    {
        return inputType is FrameInputType.PointerMove or FrameInputType.PointerDown or FrameInputType.PointerUp or FrameInputType.Tap or FrameInputType.DoubleTap;
    }

    private static bool IsKeyboard(FrameInputType inputType)
    {
        return inputType is FrameInputType.KeyboardText or FrameInputType.KeyboardCommand;
    }

    private static bool IsAnnotation(FrameInputType inputType)
    {
        return inputType is FrameInputType.AnnotationStart or FrameInputType.AnnotationUpdate or FrameInputType.AnnotationEnd;
    }
}
