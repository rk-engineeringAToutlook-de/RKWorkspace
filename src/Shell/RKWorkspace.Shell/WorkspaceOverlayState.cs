namespace RKWorkspace.Shell;

/// <summary>
/// Describes the visible overlay lifecycle without depending on a platform windowing system.
/// </summary>
public enum WorkspaceOverlayState
{
    Inactive = 0,
    Listening = 1,
    CarryCandidate = 2,
    Picked = 3,
    Carried = 4,
    NearAblage = 5,
    Placed = 6,
    Cancelled = 7,
    Failed = 8
}
