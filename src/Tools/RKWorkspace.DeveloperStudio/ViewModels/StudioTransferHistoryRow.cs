namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioTransferHistoryRow
{
    public required string Time { get; init; }

    public required string Action { get; init; }

    public required string Workspace { get; init; }

    public required string Description { get; init; }
}
