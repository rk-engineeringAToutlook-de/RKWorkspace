namespace RKWorkspace.Surface.Abstractions;

public interface ISurfaceObjectAdapter
{
    bool CanRepresentAsWorkspaceObject(string nativeObjectKind);

    string CreateThingId(string nativeObjectId);
}
