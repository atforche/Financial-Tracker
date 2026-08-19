using Domain;
using Domain.Locations;
using Domain.Locations.Queries;
using Models;
using Models.Locations;

namespace Rest.Locations;

/// <summary>
/// Converts Location Domain entities to API models.
/// </summary>
public sealed class LocationConverter
{
    /// <summary>
    /// Converts a Location query model to Domain criteria.
    /// </summary>
    public LocationQuery ToDomain(LocationQueryParameterModel model) => new(
        new LocationFilter(model.Filter?.NameSearch, model.Filter?.Ids ?? []),
        model.Sort == LocationSortModel.NameDescending ? LocationSort.NameDescending : LocationSort.Name,
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts a Location to its API model.
    /// </summary>
    public LocationModel ToModel(Location location) => new()
    {
        Id = location.Id.Value,
        Name = location.Name,
    };

    /// <summary>
    /// Converts a Location endpoint and its signed Transaction impact to an API model.
    /// </summary>
    public LocationWithAmountModel ToModel(Location location, decimal amount) => new()
    {
        Id = location.Id.Value,
        Name = location.Name,
        Amount = amount,
    };

    /// <summary>
    /// Converts a page of Locations to an API collection.
    /// </summary>
    public CollectionModel<LocationModel> ToModel(QueryPage<Location> page) => new()
    {
        Items = page.Items.Select(location => ToModel(location)).ToList(),
        TotalCount = page.TotalCount,
    };
}
