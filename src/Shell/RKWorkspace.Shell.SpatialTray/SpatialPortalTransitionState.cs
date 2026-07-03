namespace RKWorkspace.Shell.SpatialTray;

public enum SpatialPortalTransitionState
{
    None = 0,
    Entering = 1,
    InBetween = 2,
    Emerging = 3,
    ReadyToPlace = 4,
    Placed = 5,
    Cancelled = 6
}
