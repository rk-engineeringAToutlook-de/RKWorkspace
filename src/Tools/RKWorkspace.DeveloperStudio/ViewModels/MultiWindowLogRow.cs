namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record MultiWindowLogRow
{
    public required string Time { get; init; }

    public required string Workspace { get; init; }

    public required string Action { get; init; }

    public required string Result { get; init; }

    public required string Error { get; init; }
}
