namespace RKWorkspace.Core.Models;

[Flags]
public enum WorkspaceCapability
{
    None = 0,
    TextTransfer = 1,
    FileTransfer = 2,
    PdfTransfer = 4,
    ImageTransfer = 8,
    LinkTransfer = 16,
    ClipboardRead = 32,
    ClipboardWrite = 64,
    DirectionalTransfer = 128,
    Pairing = 256,
    Discovery = 512,
    FolderTransfer = 1024,
    ContextTransfer = 2048,
    ApplicationTransfer = 4096
}
