namespace RKWorkspace.Shell;

/// <summary>
/// Represents a thing inside a workspace session. It does not belong to an app, device, or operating system.
/// </summary>
public sealed record WorkspaceObject
{
    public required string ObjectId { get; init; }

    public required string DisplayName { get; init; }

    public required string Kind { get; init; }

    public string? SourceAdapterId { get; init; }
}
