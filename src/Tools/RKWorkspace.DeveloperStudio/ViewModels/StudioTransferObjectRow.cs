namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioTransferObjectRow
{
    public required string ObjectType { get; init; }

    public required string DisplayName { get; init; }

    public required string State { get; init; }

    public required string Source { get; init; }

    public required string Target { get; init; }
}
