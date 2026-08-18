namespace Domain.Locations;

/// <summary>
/// Entity representing an external party used by a Transaction endpoint.
/// </summary>
public sealed class Location : Entity<LocationId>
{
    /// <summary>
    /// Canonical display name for this Location.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Normalized name used to enforce Location identity.
    /// </summary>
    public string NormalizedName { get; internal set; }

    /// <summary>
    /// Constructs a new Location.
    /// </summary>
    internal Location(string name, string normalizedName)
        : base(new LocationId(Guid.NewGuid()))
    {
        Name = name;
        NormalizedName = normalizedName;
    }

    /// <summary>
    /// Constructs a default Location for persistence.
    /// </summary>
    private Location()
        : base()
    {
        Name = "";
        NormalizedName = "";
    }
}

/// <summary>
/// Value object representing the ID of a <see cref="Location"/>.
/// </summary>
public sealed record LocationId : EntityId
{
    /// <summary>
    /// Constructs a Location ID.
    /// </summary>
    internal LocationId(Guid value)
        : base(value)
    {
    }
}
