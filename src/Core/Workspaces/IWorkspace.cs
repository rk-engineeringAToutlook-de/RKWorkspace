using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Defines a platform-neutral workspace instance.
/// </summary>
public interface IWorkspace
{
    /// <summary>
    /// Gets the workspace id.
    /// </summary>
    WorkspaceId Id { get; }

    /// <summary>
    /// Gets the workspace descriptor.
    /// </summary>
    WorkspaceDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the workspace capabilities.
    /// </summary>
    CapabilitySet Capabilities { get; }

    /// <summary>
    /// Gets the workspace state.
    /// </summary>
    WorkspaceState State { get; }

    /// <summary>
    /// Updates the workspace descriptor.
    /// </summary>
    /// <param name="descriptor">The new descriptor.</param>
    void UpdateDescriptor(WorkspaceDescriptor descriptor);
}
