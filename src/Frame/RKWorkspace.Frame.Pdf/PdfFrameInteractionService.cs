using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed record PdfAnnotationDraft(
    PdfAnnotationOperationKind OperationKind,
    int PageNumber,
    double X,
    double Y,
    double Width,
    double Height,
    string? Text,
    string? Color,
    string CreatedByGuestAblage,
    string LeaseId,
    string FrameSessionId);

public sealed record PdfFrameAnnotationChangeSetResult(
    FrameInputValidationResult Start,
    FrameInputValidationResult Update,
    FrameInputValidationResult End,
    ChangeSet? ChangeSet)
{
    public bool Accepted => Start.Accepted && Update.Accepted && End.Accepted && ChangeSet is not null;
}

public sealed class PdfFrameInteractionService
{
    public FrameInputValidationResult Scroll(
        CarryLease lease,
        FrameSession frameSession,
        FramePolicy policy,
        double deltaY,
        long sequenceNumber,
        DateTimeOffset now)
    {
        return FrameInputValidator.Validate(
            Input(lease, frameSession, FrameInputType.Scroll, sequenceNumber, now, new FrameDelta(0.0, deltaY, 1.0)),
            lease,
            frameSession,
            policy);
    }

    public FrameInputValidationResult Zoom(
        CarryLease lease,
        FrameSession frameSession,
        FramePolicy policy,
        double scale,
        long sequenceNumber,
        DateTimeOffset now)
    {
        return FrameInputValidator.Validate(
            Input(lease, frameSession, FrameInputType.Zoom, sequenceNumber, now, new FrameDelta(0.0, 0.0, scale)),
            lease,
            frameSession,
            policy);
    }

    public PdfFrameAnnotationChangeSetResult CreateAnnotationChangeSet(
        CarryLease lease,
        FrameSession frameSession,
        FramePolicy framePolicy,
        ChangeSetPolicy changeSetPolicy,
        string baseVersionId,
        PdfAnnotationDraft draft,
        DateTimeOffset now)
    {
        var start = FrameInputValidator.Validate(
            AnnotationInput(lease, frameSession, FrameInputType.AnnotationStart, sequenceNumber: 1, now, draft),
            lease,
            frameSession,
            framePolicy);
        var update = FrameInputValidator.Validate(
            AnnotationInput(lease, frameSession, FrameInputType.AnnotationUpdate, sequenceNumber: 2, now.AddMilliseconds(10), draft),
            lease,
            frameSession,
            framePolicy);
        var end = FrameInputValidator.Validate(
            AnnotationInput(lease, frameSession, FrameInputType.AnnotationEnd, sequenceNumber: 3, now.AddMilliseconds(20), draft),
            lease,
            frameSession,
            framePolicy);

        if (!start.Accepted || !update.Accepted || !end.Accepted)
        {
            return new PdfFrameAnnotationChangeSetResult(start, update, end, null);
        }

        var annotation = new PdfAnnotationOperation(
            draft.OperationKind,
            draft.PageNumber,
            draft.X,
            draft.Y,
            draft.Width,
            draft.Height,
            draft.Text,
            draft.Color);
        var operation = annotation.ToChangeSetOperation(now.AddMilliseconds(30));
        var changeSet = ChangeSetService.Create(
            lease,
            frameSession,
            changeSetPolicy,
            baseVersionId,
            [operation],
            now.AddMilliseconds(30));

        return new PdfFrameAnnotationChangeSetResult(start, update, end, changeSet);
    }

    private static FrameInputEvent AnnotationInput(
        CarryLease lease,
        FrameSession frameSession,
        FrameInputType inputType,
        long sequenceNumber,
        DateTimeOffset now,
        PdfAnnotationDraft draft)
    {
        return new FrameInputEvent(
            $"input-{Guid.NewGuid():N}",
            frameSession.FrameSessionId,
            lease.LeaseId,
            lease.GuestAblageId,
            lease.OwnerAblageId,
            sequenceNumber,
            inputType,
            new FrameCoordinate(draft.X, draft.Y),
            new FrameDelta(draft.Width, draft.Height, 1.0),
            draft.Text,
            Array.Empty<string>(),
            null,
            FramePointerKind.Mouse,
            now);
    }

    private static FrameInputEvent Input(
        CarryLease lease,
        FrameSession frameSession,
        FrameInputType inputType,
        long sequenceNumber,
        DateTimeOffset now,
        FrameDelta delta)
    {
        return new FrameInputEvent(
            $"input-{Guid.NewGuid():N}",
            frameSession.FrameSessionId,
            lease.LeaseId,
            lease.GuestAblageId,
            lease.OwnerAblageId,
            sequenceNumber,
            inputType,
            new FrameCoordinate(0.5, 0.5),
            delta,
            null,
            Array.Empty<string>(),
            null,
            FramePointerKind.Mouse,
            now);
    }
}
