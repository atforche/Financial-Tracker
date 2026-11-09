namespace Models.Locations;

/// <summary>
/// Orders the Locations in a range.
/// </summary>
public enum LocationRangeSortModel
{
    /// <summary>Name, A to Z.</summary>
    Name,

    /// <summary>Name, Z to A.</summary>
    NameDescending,

    /// <summary>Least spent first.</summary>
    Spent,

    /// <summary>Most spent first.</summary>
    SpentDescending,

    /// <summary>Least received first.</summary>
    Received,

    /// <summary>Most received first.</summary>
    ReceivedDescending,

    /// <summary>Fewest transactions first.</summary>
    TransactionCount,

    /// <summary>Most transactions first.</summary>
    TransactionCountDescending,

    /// <summary>Least recent activity first.</summary>
    LastActivity,

    /// <summary>Most recent activity first.</summary>
    LastActivityDescending,
}

/// <summary>
/// Query parameters for the Locations over a date range.
/// </summary>
public sealed class LocationsInDateRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Dates to summarize, inclusive.
    /// </summary>
    public required DateRangeModel Range { get; init; }

    /// <summary>
    /// Text that Location names must contain.
    /// </summary>
    public string? NameSearch { get; init; }

    /// <summary>
    /// Order of the Locations. Defaults to most spent first.
    /// </summary>
    public LocationRangeSortModel? Sort { get; init; }
}

/// <summary>
/// Query parameters for the Locations over an Accounting Period range.
/// </summary>
public sealed class LocationsInAccountingPeriodRangeQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Accounting Periods to summarize, inclusive.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }

    /// <summary>
    /// Text that Location names must contain.
    /// </summary>
    public string? NameSearch { get; init; }

    /// <summary>
    /// Order of the Locations. Defaults to most spent first.
    /// </summary>
    public LocationRangeSortModel? Sort { get; init; }
}

/// <summary>
/// Query parameters for one Location over a date range.
/// </summary>
public sealed class LocationDateRangeQueryParameterModel
{
    /// <summary>
    /// Dates to summarize, inclusive.
    /// </summary>
    public required DateRangeModel Range { get; init; }
}

/// <summary>
/// Query parameters for one Location over an Accounting Period range.
/// </summary>
public sealed class LocationAccountingPeriodRangeQueryParameterModel
{
    /// <summary>
    /// Accounting Periods to summarize, inclusive.
    /// </summary>
    public required AccountingPeriodRangeModel Range { get; init; }
}

/// <summary>
/// A Location's activity over a range.
/// </summary>
public sealed class LocationRangeSummaryModel
{
    /// <summary>ID of the Location.</summary>
    public required Guid Id { get; init; }

    /// <summary>Name of the Location.</summary>
    public required string Name { get; init; }

    /// <summary>Money paid to the Location in the range.</summary>
    public required decimal Spent { get; init; }

    /// <summary>Money received from the Location in the range.</summary>
    public required decimal Received { get; init; }

    /// <summary>Number of transaction lines with the Location in the range.</summary>
    public required int TransactionCount { get; init; }

    /// <summary>Date of the latest transaction with the Location in the range.</summary>
    public DateOnly? LastActivity { get; init; }
}

/// <summary>
/// The Locations over a range, and totals across all of them.
/// </summary>
public sealed class LocationsInRangeModel
{
    /// <summary>The requested page of Locations.</summary>
    public required CollectionModel<LocationRangeSummaryModel> Locations { get; init; }

    /// <summary>Money paid to all matching Locations in the range.</summary>
    public required decimal TotalSpent { get; init; }

    /// <summary>Money received from all matching Locations in the range.</summary>
    public required decimal TotalReceived { get; init; }

    /// <summary>Number of matching Locations with any activity in the range.</summary>
    public required int ActiveLocationCount { get; init; }

    /// <summary>Money paid to the five Locations with the most spending.</summary>
    public required decimal TopFiveSpent { get; init; }
}

/// <summary>
/// A Location's activity in one month.
/// </summary>
public sealed class LocationMonthModel
{
    /// <summary>Year of the month.</summary>
    public required int Year { get; init; }

    /// <summary>Month of the year, from 1.</summary>
    public required int Month { get; init; }

    /// <summary>Money paid to the Location in the month.</summary>
    public required decimal Spent { get; init; }

    /// <summary>Money received from the Location in the month.</summary>
    public required decimal Received { get; init; }
}

/// <summary>
/// An amount paid to a Location from one Account or Fund.
/// </summary>
public sealed class LocationPaymentShareModel
{
    /// <summary>ID of the Account or Fund.</summary>
    public required Guid Id { get; init; }

    /// <summary>Name of the Account or Fund.</summary>
    public required string Name { get; init; }

    /// <summary>Amount paid.</summary>
    public required decimal Amount { get; init; }
}

/// <summary>
/// One Location's activity over a range, by month, and how it was paid for.
/// </summary>
public sealed class LocationDetailRangeModel
{
    /// <summary>The Location's totals over the range.</summary>
    public required LocationRangeSummaryModel Summary { get; init; }

    /// <summary>The Location's activity in each month of the range.</summary>
    public required IReadOnlyCollection<LocationMonthModel> Months { get; init; }

    /// <summary>Accounts the Location was paid from, most first.</summary>
    public required IReadOnlyCollection<LocationPaymentShareModel> Accounts { get; init; }

    /// <summary>Funds the Location was paid with, most first.</summary>
    public required IReadOnlyCollection<LocationPaymentShareModel> Funds { get; init; }
}
