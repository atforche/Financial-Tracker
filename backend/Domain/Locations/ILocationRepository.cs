using System.Diagnostics.CodeAnalysis;

namespace Domain.Locations;

/// <summary>
/// Persistence operations for Locations.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Attempts to get a Location by its ID.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out Location? location);

    /// <summary>
    /// Attempts to get a Location by its normalized name.
    /// </summary>
    bool TryGetByNormalizedName(string normalizedName, [NotNullWhen(true)] out Location? location);

    /// <summary>
    /// Adds a Location.
    /// </summary>
    void Add(Location location);

    /// <summary>
    /// Deletes a Location.
    /// </summary>
    void Delete(Location location);

    /// <summary>
    /// Returns whether a Location is referenced by any Transaction.
    /// </summary>
    bool IsReferenced(LocationId locationId);

    /// <summary>
    /// Replaces references to one Location with another and removes the source.
    /// </summary>
    void Consolidate(Location source, Location target);
}