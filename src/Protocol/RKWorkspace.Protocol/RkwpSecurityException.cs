namespace RKWorkspace.Protocol;

public sealed class RkwpSecurityException : Exception
{
    public RkwpSecurityException(string message)
        : base(message)
    {
    }
}
