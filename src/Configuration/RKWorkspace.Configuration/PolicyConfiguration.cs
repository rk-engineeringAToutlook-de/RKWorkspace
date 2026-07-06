namespace RKWorkspace.Configuration;

public sealed record PolicyConfiguration
{
    public string PolicyProfile { get; init; } = "DevelopmentLab";

    public bool NoFileIngress { get; init; } = true;

    public bool OwnershipTransferAllowed { get; init; }
}
