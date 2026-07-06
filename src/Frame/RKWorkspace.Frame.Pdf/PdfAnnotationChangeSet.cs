using System.Text.Json;
using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public enum PdfAnnotationOperationKind
{
    Highlight,
    Note,
    Rectangle,
    FreeTextPlanned
}

public sealed record PdfAnnotationOperation(
    PdfAnnotationOperationKind OperationKind,
    int PageNumber,
    double X,
    double Y,
    double Width,
    double Height,
    string? Text,
    string? Color)
{
    public ChangeSetOperation ToChangeSetOperation(DateTimeOffset now)
    {
        return new ChangeSetOperation(
            $"operation-pdf-annotation-{Guid.NewGuid():N}",
            OperationKind == PdfAnnotationOperationKind.FreeTextPlanned
                ? ChangeSetOperationKind.TextInserted
                : ChangeSetOperationKind.AnnotationAdded,
            $"PDF annotation {OperationKind} requested by guest.",
            PageNumber,
            JsonSerializer.Serialize(this),
            now);
    }
}

public sealed record PdfAnnotationChangeSet(
    string ChangeSetId,
    IReadOnlyList<PdfAnnotationOperation> Operations,
    ChangeSet SourceChangeSet)
{
    public bool Submitted => SourceChangeSet.State == ChangeSetState.Submitted;
}
