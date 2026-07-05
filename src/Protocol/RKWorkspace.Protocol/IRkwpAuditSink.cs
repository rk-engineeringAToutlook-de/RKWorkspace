namespace RKWorkspace.Protocol;

public interface IRkwpAuditSink
{
    void Write(RkwpAuditEvent auditEvent);
}
