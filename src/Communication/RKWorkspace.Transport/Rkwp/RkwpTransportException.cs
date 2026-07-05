namespace RKWorkspace.Transport.Rkwp;

public sealed class RkwpTransportException : Exception
{
    public RkwpTransportException(string message)
        : base(message)
    {
    }

    public RkwpTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
