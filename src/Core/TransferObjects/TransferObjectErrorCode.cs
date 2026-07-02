namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Identifies transfer object manager error cases.
/// </summary>
public enum TransferObjectErrorCode
{
    /// <summary>
    /// The transfer object id is invalid.
    /// </summary>
    InvalidId,

    /// <summary>
    /// Transfer object metadata is missing or invalid.
    /// </summary>
    MissingMetadata,

    /// <summary>
    /// The transfer object state is invalid for the operation.
    /// </summary>
    InvalidState,

    /// <summary>
    /// The transfer object does not exist.
    /// </summary>
    ObjectNotFound,

    /// <summary>
    /// The transfer object already exists.
    /// </summary>
    ObjectAlreadyExists,

    /// <summary>
    /// The transfer object could not be archived.
    /// </summary>
    ArchiveFailed,

    /// <summary>
    /// Transfer object validation failed.
    /// </summary>
    ValidationFailed
}
