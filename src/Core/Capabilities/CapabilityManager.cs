namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Provides platform-neutral capability provider registration and requirement matching.
/// </summary>
public sealed class CapabilityManager : ICapabilityManager
{
    private readonly Dictionary<string, ICapabilityProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void RegisterProvider(ICapabilityProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (string.IsNullOrWhiteSpace(provider.ProviderId))
        {
            throw new CapabilityException(
                CapabilityErrorCode.MissingProviderId,
                "Provider id is required.",
                provider.ProviderId);
        }

        if (_providers.ContainsKey(provider.ProviderId))
        {
            throw new CapabilityException(
                CapabilityErrorCode.ProviderAlreadyRegistered,
                $"Provider '{provider.ProviderId}' is already registered.",
                provider.ProviderId);
        }

        _providers.Add(provider.ProviderId, provider);
    }

    /// <inheritdoc />
    public void UnregisterProvider(string providerId)
    {
        EnsureProviderId(providerId);

        if (!_providers.Remove(providerId))
        {
            throw new CapabilityException(
                CapabilityErrorCode.ProviderNotRegistered,
                $"Provider '{providerId}' is not registered.",
                providerId);
        }
    }

    /// <inheritdoc />
    public ICapabilityProvider? GetProvider(string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            return null;
        }

        return _providers.TryGetValue(providerId, out var provider)
            ? provider
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ICapabilityProvider> GetAllProviders()
    {
        return _providers.Values.ToArray();
    }

    /// <inheritdoc />
    public CapabilitySet GetCapabilitiesForProvider(string providerId)
    {
        EnsureProviderId(providerId);

        if (!_providers.TryGetValue(providerId, out var provider))
        {
            throw new CapabilityException(
                CapabilityErrorCode.ProviderNotRegistered,
                $"Provider '{providerId}' is not registered.",
                providerId);
        }

        return provider.GetCapabilities().Snapshot();
    }

    /// <inheritdoc />
    public CapabilitySet GetCombinedCapabilities()
    {
        var result = new CapabilitySet();
        foreach (var provider in _providers.Values)
        {
            result.AddRange(provider.GetCapabilities().Capabilities);
        }

        return result;
    }

    /// <inheritdoc />
    public CapabilityMatchResult MatchRequirement(CapabilitySet capabilities, CapabilityRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(requirement);

        requirement.Validate();

        var missingRequired = requirement.RequiredCapabilities
            .Where(capabilityId => !capabilities.Contains(capabilityId))
            .Distinct()
            .ToArray();

        var presentOptional = requirement.OptionalCapabilities
            .Where(capabilities.Contains)
            .Distinct()
            .ToArray();

        var presentForbidden = requirement.ForbiddenCapabilities
            .Where(capabilities.Contains)
            .Distinct()
            .ToArray();

        var isMatch = missingRequired.Length == 0 && presentForbidden.Length == 0;
        var score = isMatch
            ? requirement.RequiredCapabilities.Distinct().Count() * 10 + presentOptional.Length
            : 0;

        var reason = BuildReason(isMatch, missingRequired, presentForbidden);

        return new CapabilityMatchResult
        {
            IsMatch = isMatch,
            MissingRequiredCapabilities = missingRequired,
            PresentOptionalCapabilities = presentOptional,
            PresentForbiddenCapabilities = presentForbidden,
            Score = score,
            Reason = reason
        };
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ICapabilityProvider> FindProvidersMatching(CapabilityRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(requirement);
        requirement.Validate();

        return _providers.Values
            .Select(provider => new
            {
                Provider = provider,
                Match = MatchRequirement(provider.GetCapabilities(), requirement)
            })
            .Where(item => item.Match.IsMatch)
            .OrderByDescending(item => item.Match.Score)
            .ThenBy(item => item.Provider.ProviderId, StringComparer.OrdinalIgnoreCase)
            .Select(item => item.Provider)
            .ToArray();
    }

    /// <inheritdoc />
    public bool IsCapabilityAvailable(CapabilityId capabilityId)
    {
        if (capabilityId == CapabilityId.Unknown)
        {
            return false;
        }

        return _providers.Values.Any(provider => provider.GetCapabilities().Contains(capabilityId));
    }

    private static void EnsureProviderId(string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            throw new CapabilityException(
                CapabilityErrorCode.MissingProviderId,
                "Provider id is required.",
                providerId);
        }
    }

    private static string BuildReason(
        bool isMatch,
        IReadOnlyCollection<CapabilityId> missingRequired,
        IReadOnlyCollection<CapabilityId> presentForbidden)
    {
        if (isMatch)
        {
            return "Requirement matched.";
        }

        var reasons = new List<string>();
        if (missingRequired.Count > 0)
        {
            reasons.Add($"Missing required: {string.Join(", ", missingRequired)}.");
        }

        if (presentForbidden.Count > 0)
        {
            reasons.Add($"Forbidden present: {string.Join(", ", presentForbidden)}.");
        }

        return string.Join(" ", reasons);
    }
}
