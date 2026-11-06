using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Defines persisted facts needed for Fund Goal balance-event queries.
/// </summary>
public interface IFundGoalBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves Funds with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetFundsAsync(
        IReadOnlyCollection<FundId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ordered Fund Goal totals histories for the provided Funds, optionally narrowed to Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FundGoalTotalsHistory>> GetFundGoalHistoriesAsync(
        IReadOnlyCollection<FundId> ids,
        IReadOnlyCollection<AccountingPeriodId>? accountingPeriodIds,
        CancellationToken cancellationToken = default);
}
