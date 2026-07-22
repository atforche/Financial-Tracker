namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Service for querying Accounting Periods and their balances.
/// </summary>
public sealed class AccountingPeriodQueryService(IAccountingPeriodQueryRepository accountingPeriodQueryRepository)
{
    /// <summary>
    /// Retrieves Accounting Periods matching the provided query.
    /// </summary>
    public Task<QueryPage<AccountingPeriod>> GetAsync(
        AccountingPeriodQuery query,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Accounting Periods and their balances.
    /// </summary>
    public Task<QueryPage<AccountingPeriodBalance>> GetWithBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetBalancesAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID, or null when it does not exist.
    /// </summary>
    public Task<AccountingPeriodBalance?> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountingPeriodQueryRepository.GetBalanceByIdAsync(new AccountingPeriodId(accountingPeriodId), cancellationToken);
}