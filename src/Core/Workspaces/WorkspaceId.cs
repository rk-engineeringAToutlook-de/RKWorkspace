namespace RKWorkspace.Core.Workspaces;

/// <summary>
/// Strong identifier for a workspace.
/// </summary>
public sealed class WorkspaceId : IEquatable<WorkspaceId>, IComparable<WorkspaceId>, IComparable
{
    /// <summary>
    /// Initializes a workspace id.
    /// </summary>
    /// <param name="value">The id value.</param>
    public WorkspaceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new WorkspaceException(
                WorkspaceErrorCode.MissingWorkspaceId,
                "Workspace id is required.");
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the raw id value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a workspace id.
    /// </summary>
    /// <param name="value">The id value.</param>
    /// <returns>A workspace id.</returns>
    public static WorkspaceId Create(string value)
    {
        return new WorkspaceId(value);
    }

    /// <inheritdoc />
    public int CompareTo(WorkspaceId? other)
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
            WorkspaceId other => CompareTo(other),
            _ => throw new ArgumentException("Object must be a WorkspaceId.", nameof(obj))
        };
    }

    /// <inheritdoc />
    public bool Equals(WorkspaceId? other)
    {
        return other is not null
            && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as WorkspaceId);
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

    public static bool operator ==(WorkspaceId? left, WorkspaceId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WorkspaceId? left, WorkspaceId? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(WorkspaceId workspaceId)
    {
        return workspaceId.Value;
    }
}
