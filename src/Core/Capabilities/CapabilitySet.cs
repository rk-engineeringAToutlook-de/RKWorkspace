namespace RKWorkspace.Core.Capabilities;

/// <summary>
/// Represents a duplicate-free set of capabilities.
/// </summary>
public sealed class CapabilitySet
{
    private readonly Dictionary<CapabilityId, Capability> _capabilities;

    /// <summary>
    /// Initializes an empty capability set.
    /// </summary>
    public CapabilitySet()
    {
        _capabilities = new Dictionary<CapabilityId, Capability>();
    }

    private CapabilitySet(Dictionary<CapabilityId, Capability> capabilities)
    {
        _capabilities = new Dictionary<CapabilityId, Capability>(capabilities);
    }

    /// <summary>
    /// Gets an empty capability set.
    /// </summary>
    public static CapabilitySet Empty => new();

    /// <summary>
    /// Gets the capabilities in this set.
    /// </summary>
    public IReadOnlyCollection<Capability> Capabilities => _capabilities.Values.ToArray();

    /// <summary>
    /// Gets the number of capabilities in this set.
    /// </summary>
    public int Count => _capabilities.Count;

    /// <summary>
    /// Adds or replaces a capability by id.
    /// </summary>
    /// <param name="capability">The capability to add.</param>
    public void Add(Capability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        if (capability.CapabilityId == CapabilityId.Unknown)
        {
            throw new CapabilityException(
                CapabilityErrorCode.InvalidCapabilityId,
                "Capability id must not be Unknown.",
                capabilityId: capability.CapabilityId);
        }

        _capabilities[capability.CapabilityId] = capability;
    }

    /// <summary>
    /// Adds capabilities to the set.
    /// </summary>
    /// <param name="capabilities">The capabilities to add.</param>
    public void AddRange(IEnumerable<Capability> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        foreach (var capability in capabilities)
        {
            Add(capability);
        }
    }

    /// <summary>
    /// Removes a capability by id.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>True when removed; otherwise false.</returns>
    public bool Remove(CapabilityId capabilityId)
    {
        return _capabilities.Remove(capabilityId);
    }

    /// <summary>
    /// Gets whether a capability id exists in the set.
    /// </summary>
    /// <param name="capabilityId">The capability id.</param>
    /// <returns>True when the capability is present; otherwise false.</returns>
    public bool Contains(CapabilityId capabilityId)
    {
        return _capabilities.ContainsKey(capabilityId);
    }

    /// <summary>
    /// Gets whether all capability ids exist in the set.
    /// </summary>
    /// <param name="capabilityIds">The capability ids.</param>
    /// <returns>True when all ids are present; otherwise false.</returns>
    public bool ContainsAll(IEnumerable<CapabilityId> capabilityIds)
    {
        ArgumentNullException.ThrowIfNull(capabilityIds);
        return capabilityIds.All(Contains);
    }

    /// <summary>
    /// Gets whether any capability id exists in the set.
    /// </summary>
    /// <param name="capabilityIds">The capability ids.</param>
    /// <returns>True when any id is present; otherwise false.</returns>
    public bool ContainsAny(IEnumerable<CapabilityId> capabilityIds)
    {
        ArgumentNullException.ThrowIfNull(capabilityIds);
        return capabilityIds.Any(Contains);
    }

    /// <summary>
    /// Creates the intersection of this set and another set.
    /// </summary>
    /// <param name="other">The other set.</param>
    /// <returns>A new set containing shared capabilities.</returns>
    public CapabilitySet Intersect(CapabilitySet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var result = new CapabilitySet();
        foreach (var capability in _capabilities.Values)
        {
            if (other.Contains(capability.CapabilityId))
            {
                result.Add(capability);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a difference set containing capabilities present here but absent in another set.
    /// </summary>
    /// <param name="other">The other set.</param>
    /// <returns>A new difference set.</returns>
    public CapabilitySet Difference(CapabilitySet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var result = new CapabilitySet();
        foreach (var capability in _capabilities.Values)
        {
            if (!other.Contains(capability.CapabilityId))
            {
                result.Add(capability);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a copy of this set.
    /// </summary>
    /// <returns>A duplicate-free snapshot.</returns>
    public CapabilitySet Snapshot()
    {
        return new CapabilitySet(_capabilities);
    }

    /// <summary>
    /// Creates a capability set from capability ids.
    /// </summary>
    /// <param name="capabilityIds">The capability ids.</param>
    /// <returns>A capability set.</returns>
    public static CapabilitySet FromIds(params CapabilityId[] capabilityIds)
    {
        return FromIds((IEnumerable<CapabilityId>)capabilityIds);
    }

    /// <summary>
    /// Creates a capability set from capability ids.
    /// </summary>
    /// <param name="capabilityIds">The capability ids.</param>
    /// <returns>A capability set.</returns>
    public static CapabilitySet FromIds(IEnumerable<CapabilityId> capabilityIds)
    {
        ArgumentNullException.ThrowIfNull(capabilityIds);

        var set = new CapabilitySet();
        foreach (var capabilityId in capabilityIds)
        {
            set.Add(Capability.Create(capabilityId));
        }

        return set;
    }
}
