namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record StudioLogEntry
{
    public required string Time { get; init; }

    public required string Action { get; init; }

    public required string Result { get; init; }

    public required string Error { get; init; }
}
