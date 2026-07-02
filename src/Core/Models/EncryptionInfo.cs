namespace RKWorkspace.Core.Models;

public sealed record EncryptionInfo(
    string Algorithm,
    string KeyId,
    string Nonce,
    string? AdditionalData)
{
    public static EncryptionInfo NotEncrypted { get; } = new("none", "none", string.Empty, null);
}
