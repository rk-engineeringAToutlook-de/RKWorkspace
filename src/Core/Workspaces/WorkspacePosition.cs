namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Describes a logical workspace position for V1 target selection.
/// </summary>
public enum WorkspacePosition
{
    /// <summary>
    /// The center workspace.
    /// </summary>
    Center,

    /// <summary>
    /// A workspace to the left.
    /// </summary>
    Left,

    /// <summary>
    /// A workspace to the right.
    /// </summary>
    Right,

    /// <summary>
    /// A workspace above.
    /// </summary>
    Above,

    /// <summary>
    /// A workspace below.
    /// </summary>
    Below,

    /// <summary>
    /// A workspace in front.
    /// </summary>
    Front,

    /// <summary>
    /// A workspace behind.
    /// </summary>
    Back,

    /// <summary>
    /// An unknown position.
    /// </summary>
    Unknown
}
