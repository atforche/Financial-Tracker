using Domain.AccountingPeriods;

namespace Domain.Overview;

/// <summary>
/// Read-only persistence operations for the overview.
/// </summary>
public interface IOverviewQueryRepository
{
    /// <summary>
    /// Retrieves the most recent open Accounting Period with its Expected Income Sources, or null when none is open.
    /// </summary>
    Task<AccountingPeriod?> GetLatestOpenAccountingPeriodAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the effects of every Transaction that has not yet posted to one of its Accounts.
    /// </summary>
    Task<IReadOnlyCollection<OverviewUnpostedEffect>> GetUnpostedEffectsAsync(CancellationToken cancellationToken = default);
}
