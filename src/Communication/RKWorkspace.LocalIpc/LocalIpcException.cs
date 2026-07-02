namespace RKWorkspace.LocalIpc;

public sealed class LocalIpcException : Exception
{
    public LocalIpcException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
