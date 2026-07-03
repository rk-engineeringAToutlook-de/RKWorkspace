namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperienceLabSnapshot
{
    public required string ActiveHumanExperienceId { get; init; }

    public required string ActiveExperimentId { get; init; }

    public required int EvolutionStep { get; init; }

    public required IReadOnlyList<HumanExperienceDefinition> HumanExperiences { get; init; }

    public required IReadOnlyList<HumanExperienceExperiment> Experiments { get; init; }

    public required IReadOnlyList<HumanExperienceObservation> Observations { get; init; }
}
