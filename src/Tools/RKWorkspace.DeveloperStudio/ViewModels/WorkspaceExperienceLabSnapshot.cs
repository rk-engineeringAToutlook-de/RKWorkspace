namespace RKWorkspace.DeveloperStudio.ViewModels;

internal sealed record WorkspaceExperienceLabSnapshot
{
    public required string GripVariantId { get; init; }

    public required string EdgeVariantId { get; init; }

    public required string TransitionVariantId { get; init; }

    public required string DropVariantId { get; init; }

    public required string PreviewVariantId { get; init; }

    public required bool AnimationEnabled { get; init; }

    public required int Speed { get; init; }

    public required IReadOnlyDictionary<string, WorkspaceExperienceLabRating> Ratings { get; init; }

    public required int TestedVariants { get; init; }

    public required int LikedVariants { get; init; }

    public required int NearlyLikedVariants { get; init; }

    public required int RejectedVariants { get; init; }

    public required int EvolutionStep { get; init; }

    public required IReadOnlyList<string> CombinationHistory { get; init; }

    public required IReadOnlyDictionary<string, int> TestedByVariant { get; init; }

    public static WorkspaceExperienceLabSnapshot Default { get; } = new()
    {
        GripVariantId = "grip-generation-04",
        EdgeVariantId = "edge-generation-02",
        TransitionVariantId = "transition-generation-03",
        DropVariantId = "drop-generation-02",
        PreviewVariantId = "preview-generation-03",
        AnimationEnabled = true,
        Speed = 5,
        Ratings = new Dictionary<string, WorkspaceExperienceLabRating>(),
        TestedVariants = 0,
        LikedVariants = 0,
        NearlyLikedVariants = 0,
        RejectedVariants = 0,
        EvolutionStep = 0,
        CombinationHistory = Array.Empty<string>(),
        TestedByVariant = new Dictionary<string, int>()
    };
}
