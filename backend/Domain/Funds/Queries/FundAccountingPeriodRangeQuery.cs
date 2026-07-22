namespace Domain.Funds.Queries;

/// <summary>
/// Criteria for querying Fund balances over an Accounting Period range.
/// </summary>
public sealed record FundAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    FundFilter Filter,
    FundRangeSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Supported Fund balance-range sort orders.
/// </summary>
public enum FundRangeSort
{
    /// <summary>
    /// Sorts by name ascending.
    /// </summary>
    Name,

    /// <summary>
    /// Sorts by name descending.
    /// </summary>
    NameDescending,

    /// <summary>
    /// Sorts by starting balance ascending.
    /// </summary>
    StartingBalance,

    /// <summary>
    /// Sorts by starting balance descending.
    /// </summary>
    StartingBalanceDescending,

    /// <summary>
    /// Sorts by ending balance ascending.
    /// </summary>
    EndingBalance,

    /// <summary>
    /// Sorts by ending balance descending.
    /// </summary>
    EndingBalanceDescending,

    /// <summary>
    /// Sorts by net change ascending.
    /// </summary>
    NetChange,

    /// <summary>
    /// Sorts by net change descending.
    /// </summary>
    NetChangeDescending,
}