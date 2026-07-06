namespace RKWorkspace.Configuration;

public sealed record AblageConfiguration
{
    public string AblageId { get; init; } = "ablage-windows-owner";

    public string DisplayName { get; init; } = "Ablage Windows";

    public string Platform { get; init; } = "Windows";
}
