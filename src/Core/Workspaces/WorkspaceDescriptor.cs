using RKWorkspace.Core.Capabilities;

namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Describes a platform-neutral workspace.
/// </summary>
public sealed record WorkspaceDescriptor
{
    /// <summary>
    /// Gets the workspace id.
    /// </summary>
    public required WorkspaceId WorkspaceId { get; init; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the workspace type.
    /// </summary>
    public required WorkspaceType WorkspaceType { get; init; }

    /// <summary>
    /// Gets the workspace state.
    /// </summary>
    public required WorkspaceState WorkspaceState { get; init; }

    /// <summary>
    /// Gets the logical workspace position.
    /// </summary>
    public required WorkspacePosition Position { get; init; }

    /// <summary>
    /// Gets the workspace capabilities.
    /// </summary>
    public required CapabilitySet Capabilities { get; init; }

    /// <summary>
    /// Gets whether this workspace is trusted.
    /// </summary>
    public bool IsTrusted { get; init; }

    /// <summary>
    /// Gets the selection priority. Higher values win.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Gets the last time this workspace was seen.
    /// </summary>
    public DateTimeOffset LastSeen { get; init; }

    /// <summary>
    /// Gets platform-neutral metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Creates an immutable snapshot of this descriptor.
    /// </summary>
    /// <returns>A descriptor snapshot.</returns>
    public WorkspaceDescriptor Snapshot()
    {
        Validate(this);

        return this with
        {
            Capabilities = Capabilities.Snapshot(),
            Metadata = Metadata.ToDictionary(
                item => item.Key,
                item => item.Value,
                StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Validates a workspace descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor to validate.</param>
    public static void Validate(WorkspaceDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidDescriptor,
                "Workspace descriptor is required.");
        }

        if (descriptor.WorkspaceId is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.MissingWorkspaceId,
                "Workspace id is required.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.DisplayName))
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidDescriptor,
                "Workspace display name is required.",
                descriptor.WorkspaceId.ToString());
        }

        if (descriptor.Capabilities is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidDescriptor,
                "Workspace capabilities are required.",
                descriptor.WorkspaceId.ToString());
        }

        if (descriptor.Metadata is null)
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.InvalidDescriptor,
                "Workspace metadata is required.",
                descriptor.WorkspaceId.ToString());
        }
    }
}
