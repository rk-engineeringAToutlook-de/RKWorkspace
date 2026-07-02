namespace RKWorkspace.LocalIpc;

public static class LocalIpcEndpoint
{
    private static readonly char[] InvalidPipeNameCharacters =
    {
        '\\',
        '/',
        ':',
        '*',
        '?',
        '"',
        '<',
        '>',
        '|'
    };

    public static string ValidatePipeName(string pipeName)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new LocalIpcException("IPC pipe name is required.");
        }

        if (pipeName.IndexOfAny(InvalidPipeNameCharacters) >= 0)
        {
            throw new LocalIpcException($"IPC pipe name '{pipeName}' contains invalid characters.");
        }

        if (pipeName.Length > 120)
        {
            throw new LocalIpcException("IPC pipe name must be 120 characters or less.");
        }

        return pipeName;
    }
}
