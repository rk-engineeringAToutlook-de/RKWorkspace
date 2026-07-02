namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Describes why a logical transfer could not complete.
/// </summary>
public enum TransferFailureReason
{
    /// <summary>
    /// No failure occurred.
    /// </summary>
    None,

    /// <summary>
    /// The transfer request was invalid.
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// The transfer direction was invalid.
    /// </summary>
    InvalidDirection,

    /// <summary>
    /// The source workspace was missing.
    /// </summary>
    SourceWorkspaceMissing,

    /// <summary>
    /// No matching target workspace was found.
    /// </summary>
    TargetWorkspaceMissing,

    /// <summary>
    /// The transfer object was missing.
    /// </summary>
    TransferObjectMissing,

    /// <summary>
    /// Required target capabilities were missing.
    /// </summary>
    RequiredCapabilitiesMissing,

    /// <summary>
    /// Forbidden target capabilities were present.
    /// </summary>
    ForbiddenCapabilitiesPresent,

    /// <summary>
    /// The plan was invalid.
    /// </summary>
    InvalidPlan,

    /// <summary>
    /// The transfer object state did not allow the requested operation.
    /// </summary>
    InvalidObjectState,

    /// <summary>
    /// The transfer was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The transfer failed explicitly.
    /// </summary>
    Failed,

    /// <summary>
    /// An unknown transfer failure occurred.
    /// </summary>
    Unknown
}
