namespace RKWorkspace.Core.Models;

public enum PayloadReferenceKind
{
    InlineText,
    LocalPath,
    ContentAddress,
    Url,
    TemporaryCache,
    Unknown
}
