namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Strong identifier for transfer objects.
/// </summary>
public sealed class TransferObjectId : IEquatable<TransferObjectId>, IComparable<TransferObjectId>, IComparable
{
    /// <summary>
    /// Initializes a transfer object id.
    /// </summary>
    /// <param name="value">The id value.</param>
    public TransferObjectId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new TransferObjectException(
                TransferObjectErrorCode.InvalidId,
                "Transfer object id is required.");
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the raw id value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new generated transfer object id.
    /// </summary>
    /// <returns>A transfer object id.</returns>
    public static TransferObjectId NewId()
    {
        return new TransferObjectId($"rkws-obj-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a transfer object id from a value.
    /// </summary>
    /// <param name="value">The id value.</param>
    /// <returns>A transfer object id.</returns>
    public static TransferObjectId Create(string value)
    {
        return new TransferObjectId(value);
    }

    /// <inheritdoc />
    public int CompareTo(TransferObjectId? other)
    {
        return other is null
            ? 1
            : StringComparer.OrdinalIgnoreCase.Compare(Value, other.Value);
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            TransferObjectId other => CompareTo(other),
            _ => throw new ArgumentException("Object must be a TransferObjectId.", nameof(obj))
        };
    }

    /// <inheritdoc />
    public bool Equals(TransferObjectId? other)
    {
        return other is not null
            && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as TransferObjectId);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(TransferObjectId? left, TransferObjectId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TransferObjectId? left, TransferObjectId? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(TransferObjectId objectId)
    {
        return objectId.Value;
    }
}
