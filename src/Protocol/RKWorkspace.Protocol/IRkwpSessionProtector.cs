namespace RKWorkspace.Protocol;

public interface IRkwpSessionProtector
{
    bool IsDevelopmentOnly { get; }

    RkwpSecurityMode SecurityMode { get; }

    string SecurityNotice { get; }

    RkwpMessage Protect(RkwpMessage message, RkwpSession session);

    bool Verify(RkwpMessage message, RkwpSession session);
}
