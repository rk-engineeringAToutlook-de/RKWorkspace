using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.Configuration;

public sealed record RKWorkspaceConfiguration
{
    public AblageConfiguration Ablage { get; init; } = new();

    public RkwpConfiguration Rkwp { get; init; } = new();

    public SecurityConfiguration Security { get; init; } = new();

    public PolicyConfiguration Policy { get; init; } = new();

    public ProximityConfiguration Proximity { get; init; } = new();

    public FrameConfiguration Frame { get; init; } = new();

    public SurfaceConfiguration Surface { get; init; } = new();

    public GestureConfiguration Gesture { get; init; } = new();

    public static RKWorkspaceConfiguration Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ConfigurationException("Configuration path is required.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new ConfigurationException($"Configuration file does not exist: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        var configuration = JsonSerializer.Deserialize<RKWorkspaceConfiguration>(json, Json.Options);
        return configuration ?? throw new ConfigurationException("Configuration file was empty.");
    }

    public string ToJson() => JsonSerializer.Serialize(this, Json.Options);
}

public static class Json
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
