namespace RKWorkspace.Shell;

/// <summary>
/// Keeps the future overlay contract invisible until a human experience needs it.
/// </summary>
public sealed class WorkspaceOverlayManager
{
    private WorkspaceOverlay _overlay = WorkspaceOverlay.Hidden;

    public WorkspaceOverlay Current => _overlay;

    public WorkspaceOverlay ShowForHumanExperience(
        WorkspaceCarryState carryState,
        string humanExperienceReference,
        string reason)
    {
        if (carryState == WorkspaceCarryState.Empty)
        {
            return Hide();
        }

        _overlay = new WorkspaceOverlay
        {
            IsVisible = true,
            CarryState = carryState,
            HumanExperienceReference = humanExperienceReference,
            Reason = reason
        };
        return _overlay;
    }

    public WorkspaceOverlay Hide()
    {
        _overlay = WorkspaceOverlay.Hidden;
        return _overlay;
    }
}
