using RKWorkspace.Transport;

namespace RKWorkspace.RkwpTransport.DevLan;

public static class RkwpDevLanEndpoint
{
    public static TransportEndpoint Create(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new RkwpDevLanTransportException("DevLan host is required.");
        }

        if (port is <= 0 or > 65535)
        {
            throw new RkwpDevLanTransportException("DevLan port must be between 1 and 65535.");
        }

        return new TransportEndpoint
        {
            EndpointId = $"rkwp-dev-lan:{host}:{port}",
            Address = $"{host}:{port}",
            TransportKind = "RkwpDevLan",
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["host"] = host,
                ["port"] = port.ToString()
            }
        }.Validate();
    }

    public static (string Host, int Port) Parse(TransportEndpoint endpoint)
    {
        if (endpoint.Metadata.TryGetValue("host", out var hostValue) &&
            endpoint.Metadata.TryGetValue("port", out var portValue) &&
            int.TryParse(portValue, out var metadataPort))
        {
            return (hostValue, metadataPort);
        }

        var parts = endpoint.Address.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
        {
            throw new RkwpDevLanTransportException($"Invalid DevLan endpoint address: {endpoint.Address}");
        }

        return (parts[0], port);
    }
}
