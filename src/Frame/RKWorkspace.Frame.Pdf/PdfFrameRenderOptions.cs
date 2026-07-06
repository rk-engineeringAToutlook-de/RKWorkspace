namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameRenderOptions(
    double Scale = 1.0,
    int Rotation = 0,
    int RequestedWidth = 1024,
    int RequestedHeight = 1448);
