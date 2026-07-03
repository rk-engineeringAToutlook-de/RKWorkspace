namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperiencePlaygroundLogEntry
{
    public required string Variant { get; init; }

    public required string Perception { get; init; }

    public required HumanExperiencePlaygroundRating Rating { get; init; }

    public required string Comment { get; init; }
}
