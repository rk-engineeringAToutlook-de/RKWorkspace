namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes a platform-neutral logical transfer direction.
/// </summary>
public enum TransferDirection
{
    /// <summary>
    /// Transfer to a workspace on the left.
    /// </summary>
    Left,

    /// <summary>
    /// Transfer to a workspace on the right.
    /// </summary>
    Right,

    /// <summary>
    /// Transfer to a workspace above.
    /// </summary>
    Above,

    /// <summary>
    /// Transfer to a workspace below.
    /// </summary>
    Below,

    /// <summary>
    /// Transfer to a workspace in front.
    /// </summary>
    Front,

    /// <summary>
    /// Transfer to a workspace behind.
    /// </summary>
    Back,

    /// <summary>
    /// Transfer to any matching workspace.
    /// </summary>
    Any,

    /// <summary>
    /// Unknown transfer direction.
    /// </summary>
    Unknown
}
