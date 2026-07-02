namespace RKWorkspace.Core.Models;

public sealed record PayloadReference(PayloadReferenceKind Kind, string Value)
{
    public static PayloadReference InlineText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new PayloadReference(PayloadReferenceKind.InlineText, text);
    }

    public static PayloadReference LocalPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new PayloadReference(PayloadReferenceKind.LocalPath, path);
    }

    public static PayloadReference Url(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return new PayloadReference(PayloadReferenceKind.Url, url);
    }
}
