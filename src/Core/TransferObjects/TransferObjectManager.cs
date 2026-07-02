namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Provides in-memory transfer object lifecycle management.
/// </summary>
public sealed class TransferObjectManager : ITransferObjectManager
{
    private readonly Dictionary<TransferObjectId, TransferObject> _objects = new();

    /// <inheritdoc />
    public ITransferObject Create(TransferObjectType objectType, TransferMetadata metadata)
    {
        var transferObject = TransferObject.Create(objectType, metadata);

        if (_objects.ContainsKey(transferObject.Id))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ObjectAlreadyExists,
                $"Transfer object '{transferObject.Id}' already exists.",
                transferObject.Id.ToString());
        }

        _objects.Add(transferObject.Id, transferObject);
        return transferObject;
    }

    /// <inheritdoc />
    public void Delete(TransferObjectId objectId)
    {
        EnsureObjectId(objectId);

        if (!_objects.Remove(objectId))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ObjectNotFound,
                $"Transfer object '{objectId}' does not exist.",
                objectId.ToString());
        }
    }

    /// <inheritdoc />
    public ITransferObject Archive(TransferObjectId objectId)
    {
        var transferObject = GetRequired(objectId);
        transferObject.Archive();
        return transferObject;
    }

    /// <inheritdoc />
    public ITransferObject Clone(TransferObjectId objectId)
    {
        var source = GetRequired(objectId);
        var clone = (TransferObject)source.Clone();

        if (_objects.ContainsKey(clone.Id))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ObjectAlreadyExists,
                $"Transfer object clone '{clone.Id}' already exists.",
                clone.Id.ToString());
        }

        _objects.Add(clone.Id, clone);
        return clone;
    }

    /// <inheritdoc />
    public ITransferObject? Get(TransferObjectId objectId)
    {
        if (objectId is null)
        {
            return null;
        }

        return _objects.TryGetValue(objectId, out var transferObject)
            ? transferObject
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ITransferObject> GetAll()
    {
        return OrderObjects(_objects.Values).Cast<ITransferObject>().ToArray();
    }

    /// <inheritdoc />
    public ITransferObject UpdateMetadata(TransferObjectId objectId, TransferMetadata metadata)
    {
        var transferObject = GetRequired(objectId);
        transferObject.UpdateMetadata(metadata);
        return transferObject;
    }

    /// <inheritdoc />
    public ITransferObject UpdateState(TransferObjectId objectId, TransferObjectState state)
    {
        var transferObject = GetRequired(objectId);
        transferObject.UpdateState(
            state,
            transferObject.Metadata.Owner,
            transferObject.Metadata.TargetWorkspace,
            $"State changed to {state}.");

        return transferObject;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ITransferObject> Find(Func<ITransferObject, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return OrderObjects(_objects.Values)
            .Cast<ITransferObject>()
            .Where(predicate)
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ITransferObject> Snapshot()
    {
        return OrderObjects(_objects.Values)
            .Select(transferObject => transferObject.Snapshot())
            .Cast<ITransferObject>()
            .ToArray();
    }

    /// <inheritdoc />
    public void Validate(TransferObjectId objectId)
    {
        GetRequired(objectId).Validate();
    }

    private TransferObject GetRequired(TransferObjectId objectId)
    {
        EnsureObjectId(objectId);

        if (!_objects.TryGetValue(objectId, out var transferObject))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ObjectNotFound,
                $"Transfer object '{objectId}' does not exist.",
                objectId.ToString());
        }

        return transferObject;
    }

    private static void EnsureObjectId(TransferObjectId objectId)
    {
        if (objectId is null)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.InvalidId,
                "Transfer object id is required.");
        }
    }

    private static IEnumerable<TransferObject> OrderObjects(IEnumerable<TransferObject> objects)
    {
        return objects.OrderBy(transferObject => transferObject.Id.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
