using System.Security.Cryptography;

namespace RKWorkspace.Protocol;

public sealed class InMemoryRkwpKeyProvider : IRkwpKeyProvider
{
    private readonly byte[] keyMaterial;

    public InMemoryRkwpKeyProvider()
    {
        keyMaterial = RandomNumberGenerator.GetBytes(32);
    }

    public string GetKeyId(RkwpSession session)
    {
        return $"dev-key-{session.SessionId}";
    }

    public ReadOnlyMemory<byte> GetKeyMaterial(RkwpSession session)
    {
        return keyMaterial;
    }
}
