using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Default platform-neutral workspace implementation.
/// </summary>
public sealed class Workspace : IWorkspace
{
    private WorkspaceDescriptor _descriptor;

    /// <summary>
    /// Initializes a workspace from a descriptor.
    /// </summary>
    /// <param name="descriptor">The workspace descriptor.</param>
    public Workspace(WorkspaceDescriptor descriptor)
    {
        WorkspaceDescriptor.Validate(descriptor);
        _descriptor = descriptor.Snapshot();
    }

    /// <inheritdoc />
    public WorkspaceId Id => _descriptor.WorkspaceId;

    /// <inheritdoc />
    public WorkspaceDescriptor Descriptor => _descriptor.Snapshot();

    /// <inheritdoc />
    public CapabilitySet Capabilities => _descriptor.Capabilities.Snapshot();

    /// <inheritdoc />
    public WorkspaceState State => _descriptor.WorkspaceState;

    /// <summary>
    /// Creates a workspace from a descriptor.
    /// </summary>
    /// <param name="descriptor">The workspace descriptor.</param>
    /// <returns>A workspace instance.</returns>
    public static Workspace FromDescriptor(WorkspaceDescriptor descriptor)
    {
        return new Workspace(descriptor);
    }

    /// <inheritdoc />
    public void UpdateDescriptor(WorkspaceDescriptor descriptor)
    {
        WorkspaceDescriptor.Validate(descriptor);

        if (descriptor.WorkspaceId != Id)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidOperation,
                "A workspace descriptor update must keep the same workspace id.",
                Id.ToString());
        }

        _descriptor = descriptor.Snapshot();
    }
}
