namespace RKWorkspace.Agent;

public sealed class AgentException : Exception
{
    public AgentException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
