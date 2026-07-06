using System.Text;
using RKWorkspace.Protocol;

namespace RKWorkspace.Frame.Pdf;

public enum MalformedPdfKind
{
    EmptyFile,
    InvalidPdf,
    HugeMetadata,
    CorruptHeader,
    EncryptedPdfPlanned
}

public sealed record MalformedPdfHandlingResult(
    MalformedPdfKind Kind,
    bool Rejected,
    bool OwnerUnlocked,
    bool AuditWritten,
    string ErrorMessage)
{
    public bool IsSafeFailure => Rejected && OwnerUnlocked && !string.IsNullOrWhiteSpace(ErrorMessage);
}

public static class PdfFrameValidation
{
    public static MalformedPdfHandlingResult ValidateMalformed(
        MalformedPdfKind kind,
        byte[] bytes,
        IRkwpAuditSink? auditSink = null)
    {
        try
        {
            ValidateBytes(kind, bytes);
            return new MalformedPdfHandlingResult(kind, Rejected: false, OwnerUnlocked: true, AuditWritten: false, ErrorMessage: string.Empty);
        }
        catch (PdfFrameException ex)
        {
            var auditWritten = false;
            if (auditSink is not null)
            {
                auditSink.Write(RkwpAuditEvent.Create(
                    RkwpAuditEventType.PolicyDenied,
                    "pdf-validation",
                    null,
                    null,
                    $"Malformed PDF rejected: {kind}.",
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, string>
                    {
                        ["kind"] = kind.ToString(),
                        ["error"] = ex.Message
                    }));
                auditWritten = true;
            }

            return new MalformedPdfHandlingResult(kind, Rejected: true, OwnerUnlocked: true, auditWritten, ex.Message);
        }
    }

    private static void ValidateBytes(MalformedPdfKind kind, byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            throw new PdfFrameException("Malformed PDF rejected: empty file.");
        }

        if (bytes.Length > 256 * 1024 && kind == MalformedPdfKind.HugeMetadata)
        {
            throw new PdfFrameException("Malformed PDF rejected: metadata exceeds pilot limit.");
        }

        if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-")
        {
            throw new PdfFrameException("Malformed PDF rejected: corrupt or invalid header.");
        }

        if (kind == MalformedPdfKind.EncryptedPdfPlanned)
        {
            throw new PdfFrameException("Encrypted PDF handling is planned and currently rejected.");
        }
    }
}
