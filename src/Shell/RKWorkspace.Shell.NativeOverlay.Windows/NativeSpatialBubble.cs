namespace RKWorkspace.Shell.NativeOverlay.Windows;

public sealed record NativeSpatialBubble
{
    public required string BubbleId { get; init; }

    public required string Label { get; init; }

    public required NativeSpatialBubbleEdge Edge { get; init; }

    public float X { get; init; }

    public float Y { get; init; }
}
