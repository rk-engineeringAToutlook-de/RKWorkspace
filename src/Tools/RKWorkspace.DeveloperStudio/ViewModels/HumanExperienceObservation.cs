namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record HumanExperienceObservation
{
    public required string HumanExperienceId { get; init; }

    public required string ExperimentId { get; init; }

    public required DateTime ObservedAtUtc { get; init; }

    public required HumanExperienceLabRating Rating { get; init; }

    public required string Comment { get; init; }

    public required int DurationSeconds { get; init; }

    public required int Repetitions { get; init; }
}
