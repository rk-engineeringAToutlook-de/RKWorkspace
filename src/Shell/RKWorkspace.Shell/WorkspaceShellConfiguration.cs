namespace RKWorkspace.Shell;

/// <summary>
/// Configuration for the invisible workspace shell foundation.
/// </summary>
public sealed record WorkspaceShellConfiguration
{
    public static WorkspaceShellConfiguration Default { get; } = new();

    public bool OverlayEnabled { get; init; } = true;

    public bool HumanExperienceValidationMode { get; init; }

    public bool DiagnosticsEnabled { get; init; }

    public WorkspaceShellConfiguration Snapshot()
    {
        return this with { };
    }
}
