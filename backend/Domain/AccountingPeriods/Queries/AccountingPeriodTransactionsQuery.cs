using Domain.Transactions.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Criteria for querying an Accounting Period with its Transactions.
/// </summary>
public sealed record AccountingPeriodTransactionsQuery(
    Guid AccountingPeriodId,
    TransactionSort Sort,
    int Offset,
    int? Limit);
