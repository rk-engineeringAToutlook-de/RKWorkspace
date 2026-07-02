namespace RKWorkspace.LocalIpc;

public enum LocalIpcMessageType
{
    AgentHello,
    AgentStatusRequest,
    AgentStatusResponse,
    WorkspaceAdvertisement,
    TransferRequest,
    TransferResponse,
    ShutdownRequest,
    ErrorResponse
}
