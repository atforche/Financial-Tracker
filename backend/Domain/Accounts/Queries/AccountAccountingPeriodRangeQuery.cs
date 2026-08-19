namespace Domain.Accounts.Queries;

/// <summary>
/// Criteria for querying Account balances over an Accounting Period range.
/// </summary>
public sealed record AccountAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    AccountFilter Filter,
    AccountRangeSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Supported Account balance-range sort orders.
/// </summary>
public enum AccountRangeSort
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
    /// Sorts by type ascending.
    /// </summary>
    Type,

    /// <summary>
    /// Sorts by type descending.
    /// </summary>
    TypeDescending,

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
