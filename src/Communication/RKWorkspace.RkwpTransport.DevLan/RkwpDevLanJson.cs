using System.Text.Json;
using System.Text.Json.Serialization;
using RKWorkspace.Transport.Rkwp;

namespace RKWorkspace.RkwpTransport.DevLan;

internal static class RkwpDevLanJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    static RkwpDevLanJson()
    {
        Options.Converters.Add(new JsonStringEnumConverter());
    }

    public static string Serialize(RkwpTransportMessage message)
    {
        return JsonSerializer.Serialize(message.Validate(), Options);
    }

    public static RkwpTransportMessage Deserialize(string line)
    {
        var message = JsonSerializer.Deserialize<RkwpTransportMessage>(line, Options);
        if (message is null)
        {
            throw new RkwpDevLanTransportException("DevLan message was empty.");
        }

        return message.Validate();
    }
}
