namespace RKWorkspace.Protocol;

public sealed record RkwpError(
    string Code,
    string Message,
    bool Recoverable);
