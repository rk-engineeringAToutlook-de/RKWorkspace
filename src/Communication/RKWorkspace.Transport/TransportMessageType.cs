namespace RKWorkspace.Transport;

public enum TransportMessageType
{
    Unknown,
    AgentHello,
    AgentStatusRequest,
    AgentStatusResponse,
    WorkspaceAdvertisement,
    TransferRequest,
    TransferResponse,
    ShutdownRequest,
    ErrorResponse,
    LiveSessionEvent,
    WorkspaceWindowFrame,
    WorkspaceObjectUpdate,
    InputEvent
}
