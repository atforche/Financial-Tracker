namespace Domain.Transactions.Queries;

/// <summary>
/// Criteria for querying Transactions.
/// </summary>
public sealed record TransactionQuery(
    TransactionFilter Filter,
    TransactionSort Sort,
    int Offset,
    int? Limit);

/// <summary>
/// Criteria used to filter Transactions.
/// </summary>
public sealed record TransactionFilter(
    IReadOnlyCollection<Guid> AccountingPeriodIds,
    IReadOnlyCollection<Guid> AccountIds,
    IReadOnlyCollection<Guid> FundIds,
    IReadOnlyCollection<TransactionType> Types);

/// <summary>
/// Supported Transaction sort orders.
/// </summary>
public enum TransactionSort
{
    /// <summary>
    /// Sorts by date ascending.
    /// </summary>
    Date,

    /// <summary>
    /// Sorts by date descending.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sorts by description ascending.
    /// </summary>
    Description,

    /// <summary>
    /// Sorts by description descending.
    /// </summary>
    DescriptionDescending,

    /// <summary>
    /// Sorts by amount ascending.
    /// </summary>
    Amount,

    /// <summary>
    /// Sorts by amount descending.
    /// </summary>
    AmountDescending,

    /// <summary>
    /// Sorts by Accounting Period ascending.
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Sorts by Accounting Period descending.
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Sorts by source ascending.
    /// </summary>
    Source,

    /// <summary>
    /// Sorts by source descending.
    /// </summary>
    SourceDescending,

    /// <summary>
    /// Sorts by destination ascending.
    /// </summary>
    Destination,

    /// <summary>
    /// Sorts by destination descending.
    /// </summary>
    DestinationDescending,

    /// <summary>
    /// Sorts by fully posted status ascending.
    /// </summary>
    FullyPosted,

    /// <summary>
    /// Sorts by fully posted status descending.
    /// </summary>
    FullyPostedDescending,
}