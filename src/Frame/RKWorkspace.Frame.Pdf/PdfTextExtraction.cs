using RKWorkspace.Protocol;
using RKWorkspace.Protocol.Ownership;

namespace RKWorkspace.Frame.Pdf;

public sealed record PdfTextExtractionRequest(
    string FrameSessionId,
    string LeaseId,
    int PageNumber,
    string RequestedByAblageId);

public sealed record PdfTextExtractionResult(
    bool Allowed,
    string Reason,
    string? Text,
    bool OwnershipTransferred,
    bool AuditWritten)
{
    public static PdfTextExtractionResult Denied(string reason, bool auditWritten)
    {
        return new PdfTextExtractionResult(false, reason, null, OwnershipTransferred: false, auditWritten);
    }

    public static PdfTextExtractionResult Success(string text, bool auditWritten)
    {
        return new PdfTextExtractionResult(true, "Text extraction allowed by policy.", text, OwnershipTransferred: false, auditWritten);
    }
}

public sealed class PdfTextExtractionService
{
    public PdfTextExtractionResult Extract(
        PdfFrameDocument document,
        CarryLease lease,
        FrameSession frameSession,
        ExtractionPolicy policy,
        PdfTextExtractionRequest request,
        DateTimeOffset now,
        IRkwpAuditSink? auditSink = null)
    {
        var auditWritten = WriteAudit(document, lease, request, now, auditSink);

        if (!string.Equals(request.LeaseId, lease.LeaseId, StringComparison.Ordinal) ||
            !string.Equals(request.FrameSessionId, frameSession.FrameSessionId, StringComparison.Ordinal))
        {
            return PdfTextExtractionResult.Denied("Text extraction denied because lease and frame are not bound.", auditWritten);
        }

        if (request.PageNumber < 1 || request.PageNumber > document.PageCount)
        {
            return PdfTextExtractionResult.Denied("Text extraction denied because page is outside the frame.", auditWritten);
        }

        if (!policy.TextAllowed || policy.FileIngressAllowed)
        {
            return PdfTextExtractionResult.Denied("Text extraction denied by policy.", auditWritten);
        }

        var text = $"Text frame for {document.FileName}, page {request.PageNumber}. Original remains owner-owned.";
        return PdfTextExtractionResult.Success(text, auditWritten);
    }

    private static bool WriteAudit(
        PdfFrameDocument document,
        CarryLease lease,
        PdfTextExtractionRequest request,
        DateTimeOffset now,
        IRkwpAuditSink? auditSink)
    {
        if (auditSink is null)
        {
            return false;
        }

        auditSink.Write(RkwpAuditEvent.Create(
            RkwpAuditEventType.FrameInput,
            lease.SessionId,
            lease.LeaseId,
            document.ThingId,
            "PDF text extraction requested.",
            now,
            new Dictionary<string, string>
            {
                ["frame-session-id"] = request.FrameSessionId,
                ["page-number"] = request.PageNumber.ToString(),
                ["requested-by"] = request.RequestedByAblageId,
                ["ownership-transferred"] = bool.FalseString
            }));
        return true;
    }
}
