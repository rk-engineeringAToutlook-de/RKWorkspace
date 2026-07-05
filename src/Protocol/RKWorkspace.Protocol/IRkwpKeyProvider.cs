namespace RKWorkspace.Protocol;

public interface IRkwpKeyProvider
{
    string GetKeyId(RkwpSession session);

    ReadOnlyMemory<byte> GetKeyMaterial(RkwpSession session);
}
