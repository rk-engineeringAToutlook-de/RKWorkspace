namespace RKWorkspace.RkwpTransport.DevLan;

public sealed class RkwpDevLanTransportException : Exception
{
    public RkwpDevLanTransportException(string message)
        : base(message)
    {
    }

    public RkwpDevLanTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
