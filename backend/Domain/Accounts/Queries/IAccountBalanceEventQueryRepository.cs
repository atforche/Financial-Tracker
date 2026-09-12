using Domain.AccountingPeriods;

namespace Domain.Accounts.Queries;

/// <summary>
/// Defines persisted facts needed for Account balance-event queries.
/// </summary>
public interface IAccountBalanceEventQueryRepository
{
    /// <summary>
    /// Retrieves ordered Account balance histories for the provided Accounts.
    /// </summary>
    Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(IReadOnlyCollection<AccountId> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all posted period-scoped balance checkpoints.
    /// </summary>
    Task<IReadOnlyCollection<AccountBalanceHistory>> GetPeriodHistoriesAsync(
        AccountId accountId,
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default);
}
