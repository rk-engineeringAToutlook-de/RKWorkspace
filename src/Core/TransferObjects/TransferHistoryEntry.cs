namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Describes one transfer object history entry.
/// </summary>
public sealed record TransferHistoryEntry
{
    /// <summary>
    /// Gets the entry timestamp.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the action name.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the user responsible for the action.
    /// </summary>
    public string User { get; init; } = string.Empty;

    /// <summary>
    /// Gets the related workspace id value.
    /// </summary>
    public string Workspace { get; init; } = string.Empty;

    /// <summary>
    /// Gets a human-readable description.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
