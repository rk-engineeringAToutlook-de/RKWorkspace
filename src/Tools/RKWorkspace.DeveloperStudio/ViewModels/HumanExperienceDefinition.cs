namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperienceDefinition
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public string DisplayName => $"{Id} - {Title}";
}
