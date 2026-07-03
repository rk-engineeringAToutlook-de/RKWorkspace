namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperiencePlaygroundSnapshot
{
    public required string ActiveHypothesisId { get; init; }

    public required IReadOnlyList<HumanExperiencePlaygroundHypothesis> Hypotheses { get; init; }

    public required IReadOnlyList<HumanExperiencePlaygroundLogEntry> Log { get; init; }
}
