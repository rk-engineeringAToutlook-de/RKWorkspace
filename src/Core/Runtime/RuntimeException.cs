namespace RKWorkspace.Core.Runtime;

/// <summary>
/// Represents a platform-neutral runtime lifecycle error.
/// </summary>
public sealed class RuntimeException : Exception
{
    /// <summary>
    /// Initializes a runtime exception.
    /// </summary>
    /// <param name="operation">The lifecycle operation.</param>
    /// <param name="state">The runtime state at the time of the error.</param>
    /// <param name="message">A human-readable message.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    public RuntimeException(
        string operation,
        RuntimeState state,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Operation = operation;
        State = state;
    }

    /// <summary>
    /// Gets the lifecycle operation.
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Gets the runtime state at the time of the error.
    /// </summary>
    public RuntimeState State { get; }
}
