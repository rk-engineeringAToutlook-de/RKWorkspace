using RKWorkspace.Core.TransferObjects;

internal sealed class FakeTransferObject : ITransferObject
{
    private readonly List<TransferHistoryEntry> _history = new();
    private TransferMetadata _metadata;

    private FakeTransferObject(TransferObjectType objectType, TransferMetadata metadata)
    {
        ObjectType = objectType;
        _metadata = metadata.Snapshot();
        State = TransferObjectState.Created;
        AddHistory("Created");
    }

    public TransferObjectId Id => _metadata.ObjectId;

    public TransferObjectType ObjectType { get; }

    public TransferMetadata Metadata => _metadata.Snapshot();

    public TransferObjectState State { get; private set; }

    public IReadOnlyCollection<TransferHistoryEntry> History => _history.ToArray();

    public void Validate()
    {
        TransferMetadata.Validate(_metadata);
        State = TransferObjectState.Validated;
        AddHistory("Validated");
    }

    public ITransferObject Clone()
    {
        var clone = new FakeTransferObject(
            ObjectType,
            _metadata with
            {
                ObjectId = TransferObjectId.NewId(),
                DisplayName = $"{_metadata.DisplayName} Copy"
            });
        clone.AddHistory("Cloned");

        return clone;
    }

    public void Archive()
    {
        State = TransferObjectState.Archived;
        AddHistory("Archived");
    }

    public static FakeTransferObject Create(TransferObjectType objectType, TransferMetadata metadata)
    {
        return new FakeTransferObject(objectType, metadata);
    }

    private void AddHistory(string action)
    {
        _history.Add(new TransferHistoryEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Action = action,
            User = _metadata.Owner,
            Workspace = _metadata.SourceWorkspace,
            Description = action
        });
    }
}
