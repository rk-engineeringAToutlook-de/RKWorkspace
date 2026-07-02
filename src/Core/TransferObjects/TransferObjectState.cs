namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Describes the manager-level state of a transfer object.
/// </summary>
public enum TransferObjectState
{
    /// <summary>
    /// The object was created.
    /// </summary>
    Created,

    /// <summary>
    /// The object metadata was validated.
    /// </summary>
    Validated,

    /// <summary>
    /// The object was queued.
    /// </summary>
    Queued,

    /// <summary>
    /// The object was prepared.
    /// </summary>
    Prepared,

    /// <summary>
    /// The object was locked.
    /// </summary>
    Locked,

    /// <summary>
    /// The object completed its lifecycle.
    /// </summary>
    Completed,

    /// <summary>
    /// The object was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The object failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The object was archived.
    /// </summary>
    Archived
}
