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

    public static WorkspaceExperienceLabSnapshot Default { get; } = new()
    {
        GripVariantId = "grip-lift",
        EdgeVariantId = "edge-pulse",
        TransitionVariantId = "transition-ghost",
        DropVariantId = "drop-soft",
        PreviewVariantId = "preview-workspace",
        AnimationEnabled = true,
        Speed = 5,
        Ratings = new Dictionary<string, WorkspaceExperienceLabRating>()
    };
}
