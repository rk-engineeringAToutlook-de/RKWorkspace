namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record WorkspaceExperienceLabOption
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }

    public required int Generation { get; init; }

    public required string Traits { get; init; }
}
