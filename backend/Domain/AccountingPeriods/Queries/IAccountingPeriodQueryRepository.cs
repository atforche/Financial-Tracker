namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Defines read-only persistence operations for Accounting Periods.
/// </summary>
public interface IAccountingPeriodQueryRepository
{
    /// <summary>
    /// Retrieves Accounting Periods matching the provided query.
    /// </summary>
    Task<QueryPage<AccountingPeriod>> GetAsync(AccountingPeriodQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Period balances matching the provided query.
    /// </summary>
    Task<QueryPage<AccountingPeriodBalance>> GetBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID, or null when it does not exist.
    /// </summary>
    Task<AccountingPeriodBalance?> GetBalanceByIdAsync(
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default);
}