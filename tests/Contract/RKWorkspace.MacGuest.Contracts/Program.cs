using System.Text.Json;

namespace RKWorkspace.MacGuest.Contracts;

internal static class Program
{
    private static readonly string[] RequiredFlowNames =
    [
        "AblageHello",
        "AblageCapabilities",
        "DevPairing",
        "SecureDevHandshake",
        "FrameSessionOpen",
        "FrameUpdate",
        "FrameInput",
        "Heartbeat",
        "Return",
        "Revocation",
        "Error"
    ];

    private static readonly string[] RequiredNoFileIngressKeys =
    [
        "pdfBytes",
        "originalBytes",
        "originalFileBytes",
        "originalPath",
        "localPdfPath",
        "downloadPath",
        "filePath"
    ];

    public static int Main(string[] args)
    {
        try
        {
            var root = FindRepositoryRoot(AppContext.BaseDirectory);
            var contractPath = GetArgumentValue(args, "--contract")
                ?? Path.Combine(root, "contracts", "macOS-guest", "rkwp-macos-guest-contract-v0.1.json");
            var schemaPath = GetArgumentValue(args, "--schema")
                ?? Path.Combine(root, "release", "schema", "rkwp-envelope-schema-v0.1.json");

            using var contract = JsonDocument.Parse(File.ReadAllText(contractPath));
            using var schema = JsonDocument.Parse(File.ReadAllText(schemaPath));

            var contractRoot = contract.RootElement;
            Ensure(contractRoot.GetProperty("platform").GetString() == "macOS", "Contract platform must be macOS.");
            Ensure(contractRoot.GetProperty("role").GetString() == "FrameGuestSurface", "Contract role must be FrameGuestSurface.");
            Ensure(contractRoot.GetProperty("noFileIngressRequired").GetBoolean(), "No File Ingress must be required.");

            var schemaMessageTypes = ReadSchemaMessageTypes(schema.RootElement);
            var sequence = ReadStringArray(contractRoot.GetProperty("sequence"));
            Ensure(RequiredFlowNames.SequenceEqual(sequence), "Contract sequence does not match the required macOS flow order.");

            var flowElements = contractRoot.GetProperty("requiredFlows").EnumerateArray().ToArray();
            var flowsByName = flowElements.ToDictionary(flow => RequiredString(flow, "name"), StringComparer.Ordinal);
            foreach (var requiredFlow in RequiredFlowNames)
            {
                Ensure(flowsByName.ContainsKey(requiredFlow), $"Missing required flow: {requiredFlow}.");
            }

            foreach (var flow in flowElements)
            {
                ValidateFlow(flow, schemaMessageTypes);
            }

            ValidateNoFileIngress(contractRoot.GetProperty("noFileIngress"));

            Console.WriteLine("RK Workspace macOS Guest Contract Test");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"Contract: {Path.GetRelativePath(root, contractPath)}");
            Console.WriteLine($"Schema: {Path.GetRelativePath(root, schemaPath)}");
            Console.WriteLine("MessageOrder: OK");
            Console.WriteLine("RequiredFields: OK");
            Console.WriteLine("Schema: OK");
            foreach (var flow in RequiredFlowNames)
            {
                Console.WriteLine($"{flow}: OK");
            }

            Console.WriteLine("NoFileIngress: SUCCESS");
            Console.WriteLine("MissingMacOSMessages: NONE");
            Console.WriteLine("RESULT: SUCCESS");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("RK Workspace macOS Guest Contract Test");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"RESULT: FAILED - {ex.Message}");
            return 1;
        }
    }

    private static void ValidateFlow(JsonElement flow, IReadOnlySet<string> schemaMessageTypes)
    {
        var name = RequiredString(flow, "name");
        var messageType = RequiredString(flow, "messageType");
        Ensure(schemaMessageTypes.Contains(messageType), $"Flow {name} references unknown RKWP message type {messageType}.");
        Ensure(flow.TryGetProperty("direction", out var direction) && !string.IsNullOrWhiteSpace(direction.GetString()), $"Flow {name} needs direction.");
        Ensure(flow.TryGetProperty("requiredPayload", out var payload) && payload.GetArrayLength() > 0, $"Flow {name} needs required payload fields.");

        if (flow.TryGetProperty("requiredFields", out var fields))
        {
            var requiredFields = ReadStringArray(fields).ToHashSet(StringComparer.Ordinal);
            foreach (var requiredField in new[] { "messageId", "messageType", "sessionId", "sourceAblageId", "targetAblageId" })
            {
                Ensure(requiredFields.Contains(requiredField), $"Flow {name} is missing required field {requiredField}.");
            }
        }
    }

    private static void ValidateNoFileIngress(JsonElement noFileIngress)
    {
        var forbiddenKeys = ReadStringArray(noFileIngress.GetProperty("forbiddenPayloadKeys")).ToHashSet(StringComparer.Ordinal);
        foreach (var key in RequiredNoFileIngressKeys)
        {
            Ensure(forbiddenKeys.Contains(key), $"No File Ingress forbidden key is missing: {key}.");
        }

        var assertions = noFileIngress.GetProperty("requiredAssertions");
        foreach (var assertion in assertions.EnumerateObject())
        {
            Ensure(assertion.Value.ValueKind == JsonValueKind.False, $"No File Ingress assertion must be false: {assertion.Name}.");
        }

        var requiredPairs = noFileIngress.GetProperty("requiredPayloadPairs");
        Ensure(requiredPairs.GetProperty("containsOriginalFileBytes").GetString() == "false", "containsOriginalFileBytes must be false.");
        Ensure(requiredPairs.GetProperty("hasOriginalPath").GetString() == "false", "hasOriginalPath must be false.");
        Ensure(requiredPairs.GetProperty("noFileIngress").GetString() == "true", "noFileIngress must be true.");
    }

    private static HashSet<string> ReadSchemaMessageTypes(JsonElement schemaRoot)
    {
        var messageTypeElement = schemaRoot
            .GetProperty("properties")
            .GetProperty("message")
            .GetProperty("properties")
            .GetProperty("messageType")
            .GetProperty("enum");

        return ReadStringArray(messageTypeElement).ToHashSet(StringComparer.Ordinal);
    }

    private static string[] ReadStringArray(JsonElement element)
    {
        return element.EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToArray();
    }

    private static string RequiredString(JsonElement element, string propertyName)
    {
        Ensure(element.TryGetProperty(propertyName, out var property), $"Missing required property: {propertyName}.");
        var value = property.GetString();
        Ensure(!string.IsNullOrWhiteSpace(value), $"Required property is empty: {propertyName}.");
        return value!;
    }

    private static string FindRepositoryRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be located.");
    }

    private static string? GetArgumentValue(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
