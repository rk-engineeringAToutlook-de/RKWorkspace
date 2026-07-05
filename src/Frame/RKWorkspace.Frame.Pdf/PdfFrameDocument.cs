using System.Security.Cryptography;
using System.Text;

namespace RKWorkspace.Frame.Pdf;

public sealed record PdfFrameDocument(
    string ThingId,
    string FileName,
    string OwnerPath,
    long LengthBytes,
    string Sha256,
    int PageCount)
{
    public static PdfFrameDocument Load(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            throw new PdfFrameException("A PDF path is required.");
        }

        var fullPath = Path.GetFullPath(pdfPath);
        if (!File.Exists(fullPath))
        {
            throw new PdfFrameException($"PDF file does not exist: {fullPath}");
        }

        var bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
        {
            throw new PdfFrameException("The selected file is not a PDF frame source.");
        }

        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var text = Encoding.Latin1.GetString(bytes);
        var pageCount = Math.Max(1, CountPageObjects(text));

        return new PdfFrameDocument(
            $"pdf-{hash[..16]}",
            Path.GetFileName(fullPath),
            fullPath,
            bytes.Length,
            hash,
            pageCount);
    }

    private static int CountPageObjects(string pdfText)
    {
        var count = 0;
        var index = 0;
        while ((index = pdfText.IndexOf("/Type /Page", index, StringComparison.Ordinal)) >= 0)
        {
            var follows = pdfText.Substring(index, Math.Min(12, pdfText.Length - index));
            if (!follows.Contains("/Pages", StringComparison.Ordinal))
            {
                count++;
            }

            index += "/Type /Page".Length;
        }

        return count;
    }
}

public sealed class PdfFrameException : Exception
{
    public PdfFrameException(string message)
        : base(message)
    {
    }
}
