namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTrayException : Exception
{
    public SpatialTrayException(string message)
        : base(message)
    {
    }

    public SpatialTrayException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
