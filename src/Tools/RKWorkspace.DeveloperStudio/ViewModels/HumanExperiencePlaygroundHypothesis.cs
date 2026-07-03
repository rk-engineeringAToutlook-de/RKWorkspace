namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperiencePlaygroundHypothesis
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Perception { get; init; }

    public required string Intent { get; init; }

    public string DisplayName => $"Hypothese {Id} - {Title}";
}
