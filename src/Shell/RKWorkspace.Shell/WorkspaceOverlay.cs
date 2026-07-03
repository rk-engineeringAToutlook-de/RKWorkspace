namespace RKWorkspace.Shell;

/// <summary>
/// Describes the future visible shell layer without creating a window, menu, dialog, or toolbar.
/// </summary>
public sealed record WorkspaceOverlay
{
    public bool IsVisible { get; init; }

    public WorkspaceOverlayState State { get; init; } = WorkspaceOverlayState.Inactive;

    public WorkspaceCarryState CarryState { get; init; } = WorkspaceCarryState.Empty;

    public string? HumanExperienceReference { get; init; }

    public string? Reason { get; init; }

    public static WorkspaceOverlay Hidden { get; } = new();
}
