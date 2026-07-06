namespace RKWorkspace.Configuration;

public sealed record ConfigurationValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public static ConfigurationValidationResult Success(IReadOnlyList<string>? warnings = null) =>
        new(true, Array.Empty<string>(), warnings ?? Array.Empty<string>());

    public static ConfigurationValidationResult Failed(IEnumerable<string> errors, IReadOnlyList<string>? warnings = null)
    {
        return new ConfigurationValidationResult(false, errors.ToArray(), warnings ?? Array.Empty<string>());
    }
}
