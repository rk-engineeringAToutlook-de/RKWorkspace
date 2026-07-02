namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowTransferObjectRow
{
    public required string ObjectId { get; init; }

    public required string ObjectType { get; init; }

    public required string Symbol { get; init; }

    public required string DisplayName { get; init; }

    public required string Preview { get; init; }

    public required string State { get; init; }

    public required string Source { get; init; }

    public required string Target { get; init; }

    public required string Location { get; init; }

    public required string MimeType { get; init; }

    public required bool IsBeingDragged { get; init; }
}
