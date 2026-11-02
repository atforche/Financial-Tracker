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
    /// Retrieves Accounting Periods with their stored balances matching the provided query.
    /// </summary>
    Task<QueryPage<StoredAccountingPeriodBalance>> GetBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an Accounting Period by ID, or null when it does not exist.
    /// </summary>
    Task<AccountingPeriod?> GetByIdAsync(
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Accounting Periods immediately before and after the provided one, when they exist.
    /// </summary>
    Task<(AccountingPeriod? Previous, AccountingPeriod? Next)> GetAdjacentAsync(
        AccountingPeriod accountingPeriod,
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
    /// Retrieves Accounting Periods with their stored balances between the provided chronological indexes.
    /// </summary>
    Task<IReadOnlyCollection<StoredAccountingPeriodBalance>> GetRangeBalancesAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves persisted income destination facts for the provided Accounting Periods, keyed by Accounting Period ID.
    /// </summary>
    Task<ILookup<Guid, FinancialRangeIncomeFact>> GetIncomeFactsByAccountingPeriodAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the provided Accounting Periods with their Expected Income Sources loaded.
    /// </summary>
    Task<IReadOnlyCollection<AccountingPeriod>> GetWithExpectedIncomeSourcesByIdsAsync(
        IReadOnlyCollection<AccountingPeriodId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an Expected Income Source with its Accounting Period and its linked Income Transactions in date
    /// order, or null when the source does not exist in that Accounting Period.
    /// </summary>
    Task<ExpectedIncomeSourceDetail?> GetExpectedIncomeSourceDetailAsync(
        AccountingPeriodId accountingPeriodId,
        Guid expectedIncomeSourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves persisted spending facts for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetRangeSpendingFactsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);
}
