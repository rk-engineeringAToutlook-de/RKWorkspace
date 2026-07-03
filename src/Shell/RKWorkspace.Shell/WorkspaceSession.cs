namespace RKWorkspace.Shell;

/// <summary>
/// Describes the current workspace room of the human, not a device pairing or application window.
/// </summary>
public sealed record WorkspaceSession
{
    public required string SessionId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public WorkspaceCarryState CarryState { get; init; } = WorkspaceCarryState.Empty;

    public IReadOnlyList<WorkspaceObject> Objects { get; init; } = Array.Empty<WorkspaceObject>();

    public WorkspaceSession WithCarryState(WorkspaceCarryState carryState)
    {
        return this with { CarryState = carryState };
    }

    public WorkspaceSession AddObject(WorkspaceObject workspaceObject)
    {
        ArgumentNullException.ThrowIfNull(workspaceObject);
        return this with { Objects = Objects.Concat(new[] { workspaceObject }).ToArray() };
    }
}
