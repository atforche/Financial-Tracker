using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Funds.Queries;

/// <summary>
/// Defines persisted facts needed for Fund balance-event queries.
/// </summary>
public interface IFundBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Transactions in the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Transactions in the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Funds with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Fund balance histories for the provided Funds.
    /// </summary>
    Task<IReadOnlyCollection<FundBalanceHistory>> GetFundHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);
}