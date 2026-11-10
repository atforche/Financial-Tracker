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

    /// <summary>
    /// Converts the requested name search, sort and page into a Location range query.
    /// </summary>
    public LocationRangeQuery ToDomain(string? nameSearch, LocationRangeSortModel? sort, PaginationModel page) => new(
        nameSearch,
        sort switch
        {
            LocationRangeSortModel.Name => LocationRangeSort.Name,
            LocationRangeSortModel.NameDescending => LocationRangeSort.NameDescending,
            LocationRangeSortModel.Spent => LocationRangeSort.Spent,
            LocationRangeSortModel.Received => LocationRangeSort.Received,
            LocationRangeSortModel.ReceivedDescending => LocationRangeSort.ReceivedDescending,
            LocationRangeSortModel.TransactionCount => LocationRangeSort.TransactionCount,
            LocationRangeSortModel.TransactionCountDescending => LocationRangeSort.TransactionCountDescending,
            LocationRangeSortModel.LastActivity => LocationRangeSort.LastActivity,
            LocationRangeSortModel.LastActivityDescending => LocationRangeSort.LastActivityDescending,
            LocationRangeSortModel.SpentDescending or null or _ => LocationRangeSort.SpentDescending,
        },
        page.Offset ?? 0,
        page.Limit);

    /// <summary>
    /// Converts a Location's activity over a range.
    /// </summary>
    public LocationRangeSummaryModel ToModel(LocationRangeSummary summary) => new()
    {
        Id = summary.Location.Id.Value,
        Name = summary.Location.Name,
        Spent = summary.Spent,
        Received = summary.Received,
        TransactionCount = summary.TransactionCount,
        LastActivity = summary.LastActivity,
    };

    /// <summary>
    /// Converts the Locations over a range.
    /// </summary>
    public LocationsInRangeModel ToModel(LocationRange range) => new()
    {
        Locations = new CollectionModel<LocationRangeSummaryModel>
        {
            Items = range.Locations.Items.Select(ToModel).ToList(),
            TotalCount = range.Locations.TotalCount,
        },
        TotalSpent = range.TotalSpent,
        TotalReceived = range.TotalReceived,
        ActiveLocationCount = range.ActiveLocationCount,
        TopFiveSpent = range.TopFiveSpent,
    };

    /// <summary>
    /// Converts one Location's activity over a range.
    /// </summary>
    public LocationDetailRangeModel ToModel(LocationDetailRange range) => new()
    {
        Summary = ToModel(range.Summary),
        Months = range.Months
            .Select(month => new LocationMonthModel { Year = month.Year, Month = month.Month, Spent = month.Spent, Received = month.Received })
            .ToList(),
        Accounts = range.Accounts.Select(ToModel).ToList(),
        Funds = range.Funds.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts an amount paid from one Account or Fund.
    /// </summary>
    private static LocationPaymentShareModel ToModel(LocationPaymentShare share) => new()
    {
        Id = share.Id,
        Name = share.Name,
        Amount = share.Amount,
    };
}
