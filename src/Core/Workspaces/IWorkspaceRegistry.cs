using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Defines platform-neutral workspace registry operations.
/// </summary>
public interface IWorkspaceRegistry
{
    /// <summary>
    /// Registers a workspace.
    /// </summary>
    /// <param name="workspace">The workspace to register.</param>
    void RegisterWorkspace(IWorkspace workspace);

    /// <summary>
    /// Unregisters a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace id.</param>
    void UnregisterWorkspace(WorkspaceId workspaceId);

    /// <summary>
    /// Updates a workspace descriptor.
    /// </summary>
    /// <param name="descriptor">The updated descriptor.</param>
    void UpdateWorkspace(WorkspaceDescriptor descriptor);

    /// <summary>
    /// Gets a registered workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace id.</param>
    /// <returns>The workspace, or null when missing.</returns>
    IWorkspace? GetWorkspace(WorkspaceId workspaceId);

    /// <summary>
    /// Gets all registered workspaces.
    /// </summary>
    /// <returns>All workspaces.</returns>
    IReadOnlyCollection<IWorkspace> GetAllWorkspaces();

    /// <summary>
    /// Finds workspaces by position.
    /// </summary>
    /// <param name="position">The logical position.</param>
    /// <returns>Matching workspaces.</returns>
    IReadOnlyCollection<IWorkspace> FindByPosition(WorkspacePosition position);

    /// <summary>
    /// Finds workspaces by capability.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>Matching workspaces.</returns>
    IReadOnlyCollection<IWorkspace> FindByCapability(CapabilityId capabilityId);

    /// <summary>
    /// Finds workspaces matching a query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>Matching results.</returns>
    IReadOnlyCollection<WorkspaceMatchResult> FindMatching(WorkspaceQuery query);

    /// <summary>
    /// Gets the best target for a query.
    /// </summary>
    /// <param name="query">The target query.</param>
    /// <returns>The best matching workspace.</returns>
    IWorkspace GetBestTarget(WorkspaceQuery query);

    /// <summary>
    /// Gets whether a workspace is registered.
    /// </summary>
    /// <param name="workspaceId">The workspace id.</param>
    /// <returns>True when registered; otherwise false.</returns>
    bool ContainsWorkspace(WorkspaceId workspaceId);

    /// <summary>
    /// Creates a descriptor snapshot of the registry.
    /// </summary>
    /// <returns>Workspace descriptor snapshots.</returns>
    IReadOnlyCollection<WorkspaceDescriptor> CreateSnapshot();
}
