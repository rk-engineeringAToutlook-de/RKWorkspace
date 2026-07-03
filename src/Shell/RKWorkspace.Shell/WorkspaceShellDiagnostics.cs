namespace RKWorkspace.Shell;

/// <summary>
/// Reports the prepared product-mode state of the Workspace Shell.
/// </summary>
public sealed record WorkspaceShellDiagnostics
{
    public required string ProductMode { get; init; }

    public required WorkspaceShellRuntimeState State { get; init; }

    public required string Session { get; init; }

    public required WorkspaceCarryState CarryState { get; init; }

    public required string Overlay { get; init; }

    public required string RuntimeVersion { get; init; }

    public required DateTimeOffset? StartedAt { get; init; }

    public required TimeSpan Uptime { get; init; }

    public required IReadOnlyCollection<string> HumanExperienceReferences { get; init; }

    public required IReadOnlyCollection<string> Errors { get; init; }

    public required IReadOnlyCollection<string> Warnings { get; init; }
}
