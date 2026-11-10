using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Locations.Queries;

/// <summary>
/// Money that moved to or from a Location in one transaction line.
/// </summary>
public sealed record LocationFlow(TransactionId TransactionId, LocationId LocationId, DateOnly Date, AccountingPeriodId AccountingPeriodId, decimal Received, decimal Spent);

/// <summary>
/// Transactions to read Location flows from: those dated within a range, or those in a set of Accounting Periods,
/// optionally for one Location.
/// </summary>
public sealed record LocationFlowScope(
    DateOnly? Start,
    DateOnly? End,
    IReadOnlyCollection<AccountingPeriodId>? AccountingPeriodIds,
    LocationId? LocationId);

/// <summary>
/// One Location's totals over the scoped transactions.
/// </summary>
public sealed record LocationTotals(
    LocationId LocationId,
    decimal Spent,
    decimal Received,
    int TransactionCount,
    int SpendingTransactionCount,
    DateOnly LastActivity);

/// <summary>
/// How much each Account and each Fund paid a Location.
/// </summary>
public sealed record LocationPaymentShares(
    IReadOnlyCollection<LocationPaymentShare> Accounts,
    IReadOnlyCollection<LocationPaymentShare> Funds);

/// <summary>
/// Orders the Locations in a range.
/// </summary>
public enum LocationRangeSort
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
/// Requests the Locations active over a date range or an Accounting Period range.
/// </summary>
public sealed record LocationRangeQuery(string? NameSearch, LocationRangeSort Sort, int Offset, int? Limit);

/// <summary>
/// One Location's activity over a range.
/// </summary>
public sealed record LocationRangeSummary(
    Location Location,
    decimal Spent,
    decimal Received,
    int TransactionCount,
    int SpendingTransactionCount,
    DateOnly? LastActivity);

/// <summary>
/// The Locations over a range, and totals across all of them.
/// </summary>
public sealed record LocationRange(
    QueryPage<LocationRangeSummary> Locations,
    decimal TotalSpent,
    decimal TotalReceived,
    int ActiveLocationCount,
    decimal TopFiveSpent);

/// <summary>
/// A Location's activity in one month of a range.
/// </summary>
public sealed record LocationMonth(int Year, int Month, decimal Spent, decimal Received);

/// <summary>
/// An amount paid to a Location from one Account or Fund.
/// </summary>
public sealed record LocationPaymentShare(Guid Id, string Name, decimal Amount);

/// <summary>
/// One Location's activity over a range, by month, and how it was paid for.
/// </summary>
public sealed record LocationDetailRange(
    LocationRangeSummary Summary,
    IReadOnlyCollection<LocationMonth> Months,
    IReadOnlyCollection<LocationPaymentShare> Accounts,
    IReadOnlyCollection<LocationPaymentShare> Funds);
