namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Classifies a workspace without deciding platform behavior.
/// </summary>
public enum WorkspaceType
{
    /// <summary>
    /// A smart device workspace.
    /// </summary>
    SmartDevice,

    /// <summary>
    /// A display node workspace.
    /// </summary>
    DisplayNode,

    /// <summary>
    /// A headless node workspace.
    /// </summary>
    HeadlessNode,

    /// <summary>
    /// A KVM node workspace.
    /// </summary>
    KvmNode,

    /// <summary>
    /// A remote workspace.
    /// </summary>
    RemoteWorkspace,

    /// <summary>
    /// A cloud workspace.
    /// </summary>
    CloudWorkspace,

    /// <summary>
    /// A hybrid workspace.
    /// </summary>
    HybridWorkspace,

    /// <summary>
    /// An unknown workspace type.
    /// </summary>
    Unknown
}
