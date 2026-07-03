namespace RKWorkspace.Shell;

/// <summary>
/// Keeps visible overlay states compatible with the human carry state model.
/// </summary>
public static class WorkspaceOverlayStateMapper
{
    public static WorkspaceCarryState ToCarryState(WorkspaceOverlayState overlayState)
    {
        return overlayState switch
        {
            WorkspaceOverlayState.CarryCandidate => WorkspaceCarryState.Candidate,
            WorkspaceOverlayState.Picked => WorkspaceCarryState.Picked,
            WorkspaceOverlayState.Carried => WorkspaceCarryState.Carried,
            WorkspaceOverlayState.NearAblage => WorkspaceCarryState.NearSurface,
            WorkspaceOverlayState.Placed => WorkspaceCarryState.Placed,
            WorkspaceOverlayState.Cancelled => WorkspaceCarryState.Cancelled,
            WorkspaceOverlayState.Failed => WorkspaceCarryState.Lost,
            _ => WorkspaceCarryState.Empty
        };
    }

    public static WorkspaceOverlayState FromCarryState(WorkspaceCarryState carryState)
    {
        return carryState switch
        {
            WorkspaceCarryState.Candidate => WorkspaceOverlayState.CarryCandidate,
            WorkspaceCarryState.Picked => WorkspaceOverlayState.Picked,
            WorkspaceCarryState.Carried => WorkspaceOverlayState.Carried,
            WorkspaceCarryState.NearSurface => WorkspaceOverlayState.NearAblage,
            WorkspaceCarryState.Placed => WorkspaceOverlayState.Placed,
            WorkspaceCarryState.Cancelled => WorkspaceOverlayState.Cancelled,
            WorkspaceCarryState.Lost => WorkspaceOverlayState.Failed,
            _ => WorkspaceOverlayState.Inactive
        };
    }
}
