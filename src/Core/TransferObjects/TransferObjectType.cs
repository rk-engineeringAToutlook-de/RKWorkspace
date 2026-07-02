namespace RKWorkspace.Core.TransferObjects;

/// <summary>
/// Classifies platform-neutral transfer objects.
/// </summary>
public enum TransferObjectType
{
    /// <summary>
    /// Plain text.
    /// </summary>
    Text,

    /// <summary>
    /// A single file.
    /// </summary>
    File,

    /// <summary>
    /// A folder.
    /// </summary>
    Folder,

    /// <summary>
    /// A PDF document.
    /// </summary>
    PDF,

    /// <summary>
    /// An image.
    /// </summary>
    Image,

    /// <summary>
    /// Clipboard content.
    /// </summary>
    Clipboard,

    /// <summary>
    /// A link or URL.
    /// </summary>
    Link,

    /// <summary>
    /// Future workspace or application context.
    /// </summary>
    Context,

    /// <summary>
    /// Binary data.
    /// </summary>
    Binary,

    /// <summary>
    /// Unknown object type.
    /// </summary>
    Unknown
}
