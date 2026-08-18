using Domain.AccountingPeriods;
namespace Domain.Transactions.Queries;

/// <summary>
/// Persisted facts required to interpret a Transaction response.
/// </summary>
public sealed record TransactionDetailsFacts(
    Transaction Transaction,
    AccountingPeriod AccountingPeriod);
