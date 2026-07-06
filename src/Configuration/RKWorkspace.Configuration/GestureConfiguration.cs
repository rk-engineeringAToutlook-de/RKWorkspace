namespace RKWorkspace.Configuration;

public sealed record GestureConfiguration
{
    public string PickGesture { get; init; } = "PointerHold";

    public string EdgeGesture { get; init; } = "CarryToGlassEdge";

    public int LongPressMs { get; init; } = 450;
}
