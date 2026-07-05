namespace RKWorkspace.Protocol;

public readonly record struct RkwpVersion(int Major, int Minor)
{
    public static RkwpVersion Current { get; } = new(0, 1);

    public override string ToString() => $"{Major}.{Minor}";
}
