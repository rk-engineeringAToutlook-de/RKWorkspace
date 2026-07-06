namespace RKWorkspace.Frame.Pdf;

public enum FrameCacheEvictionReason
{
    FrameClose,
    Revocation,
    Recovery,
    PolicyChanged,
    ManualClear
}
