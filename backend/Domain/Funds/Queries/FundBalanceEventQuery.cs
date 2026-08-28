namespace Domain.Funds.Queries;

/// <summary>
/// Criteria for querying Fund balance events over a date range.
/// </summary>
public sealed record FundBalanceEventQuery(
    DateOnly? Start,
    DateOnly? End,
    FundFilter Filter,
    FundBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria for querying Fund balance events over an Accounting Period range.
/// </summary>
public sealed record FundBalanceEventAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    FundFilter Filter,
    FundBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Supported Fund balance-event sort orders.
/// </summary>
public enum FundBalanceEventSort
{
    /// <summary>
    /// Sorts by Fund name ascending.
    /// </summary>
    FundName,

    /// <summary>
    /// Sorts by Fund name descending.
    /// </summary>
    FundNameDescending,

    /// <summary>
    /// Sorts by Accounting Period ascending.
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Sorts by Accounting Period descending.
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Sorts by date ascending.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by date descending.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts by event type ascending.
    /// </summary>
    Type,

    /// <summary>
    /// Sorts by event type descending.
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Sorts by amount ascending.
    /// </summary>
    Amount,

    /// <summary>
    /// Sorts by amount descending.
    /// </summary>
    AmountDescending,

    /// <summary>
    /// Sorts by the other party for the balance-event direction ascending.
    /// </summary>
    Counterparty,

    /// <summary>
    /// Sorts by the other party for the balance-event direction descending.
    /// </summary>
    CounterpartyDescending,

}
