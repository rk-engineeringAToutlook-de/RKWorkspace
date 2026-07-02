namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Default platform-neutral transfer object implementation.
/// </summary>
public sealed class TransferObject : ITransferObject
{
    private readonly List<TransferHistoryEntry> _history = new();
    private TransferMetadata _metadata;

    /// <summary>
    /// Initializes a transfer object.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <param name="metadata">The transfer metadata.</param>
    public TransferObject(TransferObjectType objectType, TransferMetadata metadata)
    {
        if (objectType == TransferObjectType.Unknown)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ValidationFailed,
                "Transfer object type must not be Unknown.",
                metadata?.ObjectId?.ToString());
        }

        TransferMetadata.Validate(metadata);

        ObjectType = objectType;
        _metadata = metadata.Snapshot();
        State = TransferObjectState.Created;
        AddHistory("Created", _metadata.Owner, _metadata.SourceWorkspace, "Transfer object created.");
    }

    private TransferObject(
        TransferObjectType objectType,
        TransferMetadata metadata,
        TransferObjectState state,
        IEnumerable<TransferHistoryEntry> history)
    {
        ObjectType = objectType;
        _metadata = metadata.Snapshot();
        State = state;
        _history.AddRange(history);
    }

    /// <inheritdoc />
    public TransferObjectId Id => _metadata.ObjectId;

    /// <inheritdoc />
    public TransferObjectType ObjectType { get; }

    /// <inheritdoc />
    public TransferMetadata Metadata => _metadata.Snapshot();

    /// <inheritdoc />
    public TransferObjectState State { get; private set; }

    /// <inheritdoc />
    public IReadOnlyCollection<TransferHistoryEntry> History => _history.ToArray();

    /// <summary>
    /// Creates a transfer object.
    /// </summary>
    /// <param name="objectType">The object type.</param>
    /// <param name="metadata">The transfer metadata.</param>
    /// <returns>A transfer object.</returns>
    public static TransferObject Create(TransferObjectType objectType, TransferMetadata metadata)
    {
        return new TransferObject(objectType, metadata);
    }

    /// <inheritdoc />
    public void Validate()
    {
        if (State == TransferObjectState.Archived)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ValidationFailed,
                "Archived transfer objects cannot be validated.",
                Id.ToString());
        }

        TransferMetadata.Validate(_metadata);
        UpdateState(TransferObjectState.Validated, _metadata.Owner, _metadata.SourceWorkspace, "Transfer object validated.");
    }

    /// <inheritdoc />
    public ITransferObject Clone()
    {
        var now = DateTimeOffset.UtcNow;
        var cloneMetadata = _metadata with
        {
            ObjectId = TransferObjectId.NewId(),
            CreatedAt = now,
            ModifiedAt = now,
            DisplayName = $"{_metadata.DisplayName} Copy"
        };

        var clone = new TransferObject(ObjectType, cloneMetadata);
        clone.AddHistory("Cloned", _metadata.Owner, _metadata.SourceWorkspace, $"Cloned from {Id}.");

        return clone;
    }

    /// <inheritdoc />
    public void Archive()
    {
        if (State == TransferObjectState.Archived)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.ArchiveFailed,
                "Transfer object is already archived.",
                Id.ToString());
        }

        UpdateState(TransferObjectState.Archived, _metadata.Owner, _metadata.TargetWorkspace, "Transfer object archived.");
    }

    /// <summary>
    /// Updates the transfer metadata.
    /// </summary>
    /// <param name="metadata">The updated metadata.</param>
    public void UpdateMetadata(TransferMetadata metadata)
    {
        TransferMetadata.Validate(metadata);
        if (metadata.ObjectId != Id)
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.InvalidId,
                "Metadata updates must keep the same transfer object id.",
                Id.ToString());
        }

        _metadata = metadata.Snapshot();
        AddHistory("MetadataUpdated", _metadata.Owner, _metadata.SourceWorkspace, "Transfer metadata updated.");
    }

    /// <summary>
    /// Updates the transfer object state.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <param name="user">The user responsible for the change.</param>
    /// <param name="workspace">The related workspace.</param>
    /// <param name="description">A description.</param>
    public void UpdateState(
        TransferObjectState state,
        string user = "",
        string workspace = "",
        string description = "")
    {
        if (!CanTransition(State, state))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.InvalidState,
                $"Cannot move transfer object from {State} to {state}.",
                Id.ToString());
        }

        State = state;
        AddHistory(
            $"State:{state}",
            user,
            workspace,
            string.IsNullOrWhiteSpace(description) ? $"State changed to {state}." : description);
    }

    /// <summary>
    /// Creates an immutable object snapshot.
    /// </summary>
    /// <returns>A transfer object snapshot.</returns>
    public TransferObject Snapshot()
    {
        return new TransferObject(ObjectType, _metadata.Snapshot(), State, History);
    }

    private static bool CanTransition(TransferObjectState from, TransferObjectState to)
    {
        if (from == to)
        {
            return true;
        }

        if (from == TransferObjectState.Archived)
        {
            return false;
        }

        if (to == TransferObjectState.Archived)
        {
            return from is TransferObjectState.Completed
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed
                or TransferObjectState.Locked
                or TransferObjectState.Prepared
                or TransferObjectState.Queued
                or TransferObjectState.Validated
                or TransferObjectState.Created;
        }

        return from switch
        {
            TransferObjectState.Created => to is TransferObjectState.Validated
                or TransferObjectState.Queued
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed,
            TransferObjectState.Validated => to is TransferObjectState.Queued
                or TransferObjectState.Prepared
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed
                or TransferObjectState.Completed,
            TransferObjectState.Queued => to is TransferObjectState.Prepared
                or TransferObjectState.Locked
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed
                or TransferObjectState.Completed,
            TransferObjectState.Prepared => to is TransferObjectState.Locked
                or TransferObjectState.Completed
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed,
            TransferObjectState.Locked => to is TransferObjectState.Completed
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed,
            TransferObjectState.Completed
                or TransferObjectState.Cancelled
                or TransferObjectState.Failed => false,
            _ => false
        };
    }

    private void AddHistory(string action, string user, string workspace, string description)
    {
        _history.Add(new TransferHistoryEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Action = action,
            User = user,
            Workspace = workspace,
            Description = description
        });
    }
}
