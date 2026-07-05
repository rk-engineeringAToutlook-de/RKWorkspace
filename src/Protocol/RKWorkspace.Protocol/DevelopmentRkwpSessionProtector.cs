using System.Security.Cryptography;
using System.Text;

namespace RKWorkspace.Protocol;

public sealed class DevelopmentRkwpSessionProtector : IRkwpSessionProtector
{
    public bool IsDevelopmentOnly => true;

    public RkwpSecurityMode SecurityMode => RkwpSecurityMode.DevelopmentInsecure;

    public string SecurityNotice => "Development only: no real encryption, no production authentication, no production security.";

    public RkwpMessage Protect(RkwpMessage message, RkwpSession session)
    {
        if (session.SecureSessionRequired || session.SecurityMode is not RkwpSecurityMode.DevelopmentInsecure)
        {
            throw new RkwpProtocolException("Development protector cannot satisfy a secure production session.");
        }

        return message with { AuthTag = CreateDevelopmentTag(message) };
    }

    public bool Verify(RkwpMessage message, RkwpSession session)
    {
        if (session.SecureSessionRequired || session.SecurityMode is not RkwpSecurityMode.DevelopmentInsecure)
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
