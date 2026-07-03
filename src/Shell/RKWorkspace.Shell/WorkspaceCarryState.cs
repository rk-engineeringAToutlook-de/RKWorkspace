namespace RKWorkspace.Shell;

/// <summary>
/// Describes the human carry experience, not a computer transfer state.
/// </summary>
public enum WorkspaceCarryState
{
    Empty = 0,
    Candidate = 1,
    Picked = 2,
    Carried = 3,
    NearSurface = 4,
    Placed = 5,
    Cancelled = 6,
    Lost = 7
}
