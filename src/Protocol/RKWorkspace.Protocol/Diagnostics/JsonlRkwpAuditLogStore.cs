using System.Text.Json;
using System.Text.Json.Serialization;

namespace RKWorkspace.Protocol.Diagnostics;

public sealed class JsonlRkwpAuditLogStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    public void WriteAll(string path, IEnumerable<RkwpAuditLogRecord> records)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(path, append: false);
        foreach (var record in records)
        {
            writer.WriteLine(JsonSerializer.Serialize(record, JsonOptions));
        }
    }

    public void Append(string path, RkwpAuditLogRecord record)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(path, JsonSerializer.Serialize(record, JsonOptions) + Environment.NewLine);
    }

    public IReadOnlyList<RkwpAuditLogRecord> ReadAll(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("RKWP audit log not found.", path);
        }

        var records = new List<RkwpAuditLogRecord>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<RkwpAuditLogRecord>(line, JsonOptions)
                ?? throw new InvalidDataException("RKWP audit log contains an invalid JSONL record.");
            records.Add(record);
        }

        return records;
    }
}
