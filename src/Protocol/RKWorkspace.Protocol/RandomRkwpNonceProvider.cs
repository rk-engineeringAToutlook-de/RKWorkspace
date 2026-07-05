using System.Security.Cryptography;

namespace RKWorkspace.Protocol;

public sealed class RandomRkwpNonceProvider : IRkwpNonceProvider
{
    public string CreateNonce(RkwpSession session)
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }
}
