namespace Domain.Accounts.Queries;

/// <summary>
/// Criteria for querying Account balance events over a date range.
/// </summary>
public sealed record AccountBalanceEventQuery(
    DateOnly Start,
    DateOnly End,
    AccountFilter Filter,
    AccountBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria for querying Account balance events over an Accounting Period range.
/// </summary>
public sealed record AccountBalanceEventAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    AccountFilter Filter,
    AccountBalanceEventSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Supported Account balance-event sort orders.
/// </summary>
public enum AccountBalanceEventSort
{
    /// <summary>
    /// Sorts by Account name ascending.
    /// </summary>
    AccountName,

    /// <summary>
    /// Sorts by Account name descending.
    /// </summary>
    AccountNameDescending,

    /// <summary>
    /// Sorts by Accounting Period ascending.
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Sorts by Accounting Period descending.
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Sorts by effective date ascending.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by effective date descending.
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
    /// Sorts by the direction-relative other party ascending.
    /// </summary>
    Counterparty,

    /// <summary>
    /// Sorts by the direction-relative other party descending.
    /// </summary>
    CounterpartyDescending,

}