using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Defines persisted facts needed for Fund Plan balance-event queries.
/// </summary>
public interface IFundPlanBalanceEventQueryRepository
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
    /// Retrieves Accounting Periods with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetAccountingPeriodsAsync(
        IReadOnlyCollection<AccountingPeriodId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Periods between the provided chronological indexes.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetAccountingPeriodsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Funds with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Fund Plan totals histories for the provided Funds.
    /// </summary>
    Task<IReadOnlyCollection<FundPlanTotalsHistory>> GetFundPlanHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);
}