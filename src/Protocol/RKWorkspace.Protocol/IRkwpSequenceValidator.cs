namespace RKWorkspace.Protocol;

public interface IRkwpSequenceValidator
{
    void ValidateAndRecord(RkwpMessage message);
}
