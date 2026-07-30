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

    /// <summary>
    /// Retrieves an Accounting Period by ID, or null when it does not exist.
    /// </summary>
    Task<AccountingPeriod?> GetByIdAsync(
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Periods with the provided IDs.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetByIdsAsync(
        IReadOnlyCollection<AccountingPeriodId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Periods between the provided chronological indexes.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetRangeAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounting Period balances between the provided chronological indexes.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriodBalance>> GetRangeBalancesAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves persisted income destination facts for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetRangeIncomeFactsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves persisted spending facts for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetRangeSpendingFactsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);
}