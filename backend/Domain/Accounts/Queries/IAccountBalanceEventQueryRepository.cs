using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Accounts.Queries;

/// <summary>
/// Defines persisted facts needed for Account balance-event queries.
/// </summary>
public interface IAccountBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Transactions in the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Periods with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetAccountingPeriodsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Account balance histories for the provided Accounts.
    /// </summary>
    Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(IReadOnlyCollection<AccountId> ids, CancellationToken cancellationToken = default);
}