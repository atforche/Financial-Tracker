using Domain.AccountingPeriods;
using Domain.Accounts.Queries;
using Domain.Funds.Queries;

namespace Domain.Overview;

/// <summary>
/// Read-only persistence operations for the overview.
/// </summary>
public interface IOverviewQueryRepository
{
    /// <summary>
    /// Retrieves every Account with its posted balance before the provided date.
    /// </summary>
    Task<IReadOnlyCollection<OverviewAccountOpeningBalance>> GetAccountOpeningBalancesAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves posted Account balance history within the provided dates, inclusive.
    /// </summary>
    Task<IReadOnlyCollection<AccountDateBalanceFact>> GetAccountBalanceHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves every Fund with its posted balance before the provided date.
    /// </summary>
    Task<IReadOnlyCollection<OverviewFundOpeningBalance>> GetFundOpeningBalancesAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves posted Fund balance history within the provided dates, inclusive.
    /// </summary>
    Task<IReadOnlyCollection<FundDateBalanceFact>> GetFundBalanceHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account balances for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<OverviewAccountPeriodBalance>> GetAccountPeriodBalancesAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund balances for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<OverviewFundPeriodBalance>> GetFundPeriodBalancesAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default);
}
