using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Accounts.Queries;

/// <summary>
/// Defines persisted facts needed for Account balance-event queries.
/// </summary>
public interface IAccountBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Transactions affecting the provided Account in the supplied date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        AccountId accountId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves currently unposted Transactions affecting the provided Account.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetPendingTransactionsAsync(
        AccountId accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions in the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions in the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Account balance histories for the provided Accounts.
    /// </summary>
    Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(IReadOnlyCollection<AccountId> ids, CancellationToken cancellationToken = default);
}