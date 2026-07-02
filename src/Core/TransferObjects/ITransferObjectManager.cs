namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Defines in-memory transfer object manager operations.
/// </summary>
public interface ITransferObjectManager
{
    /// <summary>
    /// Creates and registers a transfer object.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The created object.</returns>
    ITransferObject Create(TransferObjectType objectType, TransferMetadata metadata);

    /// <summary>
    /// Deletes a transfer object.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    void Delete(TransferObjectId objectId);

    /// <summary>
    /// Archives a transfer object.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    /// <returns>The archived object.</returns>
    ITransferObject Archive(TransferObjectId objectId);

    /// <summary>
    /// Clones a transfer object and registers the clone.
    /// </summary>
    /// <param name="objectId">The source object id.</param>
    /// <returns>The cloned object.</returns>
    ITransferObject Clone(TransferObjectId objectId);

    /// <summary>
    /// Gets a transfer object.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    /// <returns>The object, or null when not registered.</returns>
    ITransferObject? Get(TransferObjectId objectId);

    /// <summary>
    /// Gets all transfer objects.
    /// </summary>
    /// <returns>All objects.</returns>
    IReadOnlyCollection<ITransferObject> GetAll();

    /// <summary>
    /// Updates transfer object metadata.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    /// <param name="metadata">The updated metadata.</param>
    /// <returns>The updated object.</returns>
    ITransferObject UpdateMetadata(TransferObjectId objectId, TransferMetadata metadata);

    /// <summary>
    /// Updates transfer object state.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    /// <param name="state">The new state.</param>
    /// <returns>The updated object.</returns>
    ITransferObject UpdateState(TransferObjectId objectId, TransferObjectState state);

    /// <summary>
    /// Finds transfer objects.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Matching objects.</returns>
    IReadOnlyCollection<ITransferObject> Find(Func<ITransferObject, bool> predicate);

    /// <summary>
    /// Creates an object snapshot.
    /// </summary>
    /// <returns>Transfer object snapshots.</returns>
    IReadOnlyCollection<ITransferObject> Snapshot();

    /// <summary>
    /// Validates a transfer object.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    void Validate(TransferObjectId objectId);
}
