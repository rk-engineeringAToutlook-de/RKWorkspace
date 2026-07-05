namespace RKWorkspace.Protocol;

public interface IRkwpNonceProvider
{
    string CreateNonce(RkwpSession session);
}
