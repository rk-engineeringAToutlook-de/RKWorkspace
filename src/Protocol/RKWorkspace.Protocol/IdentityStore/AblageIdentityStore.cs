using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RKWorkspace.Protocol.Identity;

namespace RKWorkspace.Protocol.IdentityStore;

public sealed record AblageIdentityStoreOptions(
    string StoreRoot,
    string AblageName,
    string Platform,
    string SurfaceType = "DevelopmentSurface");

public sealed record AblageKeyPair(
    string KeyId,
    string Algorithm,
    string PublicKey,
    string PrivateKey,
    bool DevelopmentOnly,
    DateTimeOffset CreatedAt)
{
    public static AblageKeyPair CreateDevelopment(string ablageId)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var seed = $"{ablageId}|{createdAt:O}|{Guid.NewGuid():N}";
        return new AblageKeyPair(
            $"dev-key-{ablageId}-{Guid.NewGuid():N}",
            "DEV-KEY-STRUCTURAL",
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"public|{seed}"))),
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"private|{seed}"))),
            DevelopmentOnly: true,
            createdAt);
    }
}

public sealed record AblageCertificateReference(
    string CertificateId,
    string Thumbprint,
    string PublicKeyId,
    DateTimeOffset CreatedAt,
    bool DevelopmentOnly);

public sealed record AblageIdentityRecord(
    string AblageId,
    string DisplayName,
    string Platform,
    string SurfaceType,
    DateTimeOffset CreatedAt,
    AblagePublicKey PublicKey,
    string PrivateKeyPath,
    AblageTrustLevel TrustLevel,
    AblagePairingState PairingState,
    AblageCertificateReference Certificate,
    bool DevelopmentOnly)
{
    public AblageIdentity ToIdentity()
    {
        return new AblageIdentity
        {
            AblageId = new AblageIdentityId(AblageId),
            DisplayName = DisplayName,
            Platform = Platform,
            SurfaceType = SurfaceType,
            PublicKey = PublicKey,
            TrustLevel = TrustLevel,
            PairingState = PairingState,
            Capabilities = ["FrameOnly", "Heartbeat", "NoFileIngress"],
            CreatedAt = CreatedAt,
            LastSeen = CreatedAt
        };
    }
}

public sealed record AblageIdentityStoreResult(
    AblageIdentityRecord Record,
    string IdentityPath,
    string PrivateKeyPath,
    bool Created,
    bool Overwritten);

public sealed class AblageIdentityStoreException : Exception
{
    public AblageIdentityStoreException(string message)
        : base(message)
    {
    }
}

public sealed class AblageIdentityStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly HashSet<string> AllowedPlatforms = new(StringComparer.OrdinalIgnoreCase)
    {
        "Windows",
        "macOS",
        "MacOS",
        "iOS",
        "IOS",
        "iPadOS",
        "IPadOS",
        "Android",
        "Linux"
    };

    private readonly AblageIdentityStoreOptions options;

    public AblageIdentityStore(AblageIdentityStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.StoreRoot))
        {
            throw new AblageIdentityStoreException("Identity store root is required.");
        }

        if (string.IsNullOrWhiteSpace(options.AblageName))
        {
            throw new AblageIdentityStoreException("AblageName is required.");
        }

        if (!AllowedPlatforms.Contains(options.Platform))
        {
            throw new AblageIdentityStoreException($"Unsupported platform: {options.Platform}.");
        }

        this.options = options;
    }

    public AblageIdentityStoreResult CreateOrLoad(bool force = false)
    {
        Directory.CreateDirectory(options.StoreRoot);
        var ablageId = CreateAblageId(options.AblageName, options.Platform);
        var identityPath = Path.Combine(options.StoreRoot, $"{ablageId}.identity.json");
        var privateKeyPath = Path.Combine(options.StoreRoot, $"{ablageId}.private-key.dev.json");

        if (File.Exists(identityPath) && !force)
        {
            return new AblageIdentityStoreResult(
                Load(identityPath),
                identityPath,
                privateKeyPath,
                Created: false,
                Overwritten: false);
        }

        var overwritten = File.Exists(identityPath);
        var key = AblageKeyPair.CreateDevelopment(ablageId);
        var record = CreateRecord(ablageId, identityPath, privateKeyPath, key);
        File.WriteAllText(identityPath, JsonSerializer.Serialize(record, JsonOptions), Encoding.UTF8);
        File.WriteAllText(privateKeyPath, JsonSerializer.Serialize(key, JsonOptions), Encoding.UTF8);
        return new AblageIdentityStoreResult(
            record,
            identityPath,
            privateKeyPath,
            Created: true,
            Overwritten: overwritten);
    }

    public static AblageIdentityRecord Load(string identityPath)
    {
        var record = JsonSerializer.Deserialize<AblageIdentityRecord>(File.ReadAllText(identityPath, Encoding.UTF8), JsonOptions);
        return record ?? throw new AblageIdentityStoreException("Identity record could not be loaded.");
    }

    public static string CreateAblageId(string ablageName, string platform)
    {
        var normalized = new string(ablageName
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());
        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        normalized = normalized.Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "ablage";
        }

        return $"ablage-{platform.ToLowerInvariant()}-{normalized}";
    }

    private AblageIdentityRecord CreateRecord(string ablageId, string identityPath, string privateKeyPath, AblageKeyPair key)
    {
        var publicKey = new AblagePublicKey(key.KeyId, key.Algorithm, key.PublicKey, key.CreatedAt);
        var thumbprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{ablageId}|{publicKey.EncodedPublicKey}|{key.CreatedAt:O}")));
        var certificate = new AblageCertificateReference(
            $"dev-cert-{ablageId}-{Guid.NewGuid():N}",
            thumbprint,
            key.KeyId,
            key.CreatedAt,
            DevelopmentOnly: true);
        return new AblageIdentityRecord(
            ablageId,
            options.AblageName,
            options.Platform,
            options.SurfaceType,
            key.CreatedAt,
            publicKey,
            Path.GetRelativePath(Path.GetDirectoryName(identityPath)!, privateKeyPath),
            AblageTrustLevel.DevTrusted,
            AblagePairingState.Paired,
            certificate,
            DevelopmentOnly: true);
    }
}
