namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Defines a platform-neutral transfer object.
/// </summary>
public interface ITransferObject
{
    /// <summary>
    /// Gets the transfer object id.
    /// </summary>
    TransferObjectId Id { get; }

    /// <summary>
    /// Gets the transfer object type.
    /// </summary>
    TransferObjectType ObjectType { get; }

    /// <summary>
    /// Gets the transfer metadata.
    /// </summary>
    TransferMetadata Metadata { get; }

    /// <summary>
    /// Gets the transfer object state.
    /// </summary>
    TransferObjectState State { get; }

    /// <summary>
    /// Gets object history.
    /// </summary>
    IReadOnlyCollection<TransferHistoryEntry> History { get; }

    /// <summary>
    /// Validates the object.
    /// </summary>
    void Validate();

    /// <summary>
    /// Clones the object with a new id.
    /// </summary>
    /// <returns>The cloned object.</returns>
    ITransferObject Clone();

    /// <summary>
    /// Archives the object.
    /// </summary>
    void Archive();
}
