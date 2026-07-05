namespace RKWorkspace.Surface.Abstractions;

public sealed class SurfaceException : Exception
{
    public SurfaceException(string message)
        : base(message)
    {
    }
}
