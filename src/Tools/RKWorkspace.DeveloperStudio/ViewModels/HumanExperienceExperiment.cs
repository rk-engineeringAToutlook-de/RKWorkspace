namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperienceExperiment
{
    public required string HumanExperienceId { get; init; }

    public required string ExperimentId { get; init; }

    public required int Number { get; init; }

    public required string Title { get; init; }

    public required string Goal { get; init; }

    public required string EvolutionNote { get; init; }

    public string? SourceExperimentId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public string DisplayName => $"Experiment {Number:000}";
}
