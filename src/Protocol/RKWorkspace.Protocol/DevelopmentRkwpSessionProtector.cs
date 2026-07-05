using System.Security.Cryptography;
using System.Text;

namespace RKWorkspace.Protocol;

public sealed class DevelopmentRkwpSessionProtector : IRkwpSessionProtector
{
    public bool IsDevelopmentOnly => true;

    public RkwpMessage Protect(RkwpMessage message, RkwpSession session)
    {
        if (session.SecureSessionRequired)
        {
            throw new RkwpProtocolException("Development protector cannot satisfy a secure production session.");
        }

        return message with { AuthTag = CreateDevelopmentTag(message) };
    }

    public bool Verify(RkwpMessage message, RkwpSession session)
    {
        if (session.SecureSessionRequired)
        {
            return false;
        }

        return string.Equals(message.AuthTag, CreateDevelopmentTag(message), StringComparison.Ordinal);
    }

    private static string CreateDevelopmentTag(RkwpMessage message)
    {
        var input = $"{message.MessageId}|{message.SessionId}|{message.SequenceNumber}|{message.Nonce}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return $"DEV-{Convert.ToHexString(hash)[..24]}";
    }
}
