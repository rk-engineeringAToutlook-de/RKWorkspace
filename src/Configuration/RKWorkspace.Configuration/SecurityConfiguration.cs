namespace RKWorkspace.Configuration;

public sealed record SecurityConfiguration
{
    public string SecurityMode { get; init; } = "DevelopmentInsecure";

    public bool SecureSessionRequired { get; init; }

    public bool AuditRequired { get; init; } = true;
}
