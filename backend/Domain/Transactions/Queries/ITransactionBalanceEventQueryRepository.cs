using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Queries;

/// <summary>
/// Defines Transaction fact retrieval shared by balance-event queries.
/// </summary>
public interface ITransactionBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Transactions affecting the provided Account in the supplied date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetForAccountAsync(
        AccountId accountId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions in the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions in the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions with unposted portions affecting any of the provided Accounts.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetPendingForAccountsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions with unposted Fund assignments affecting any of the provided Funds.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetPendingForFundsAsync(
        IReadOnlyCollection<FundId> fundIds,
        CancellationToken cancellationToken = default);
}
