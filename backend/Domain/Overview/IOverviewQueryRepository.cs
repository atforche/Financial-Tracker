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
    /// Retrieves the ID of the most recent open Accounting Period, or null when none is open.
    /// </summary>
    Task<AccountingPeriodId?> GetLatestOpenAccountingPeriodIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the effects of every Transaction that has not yet posted to one of its Accounts.
    /// </summary>
    Task<IReadOnlyCollection<OverviewUnpostedEffect>> GetUnpostedEffectsAsync(CancellationToken cancellationToken = default);
}
