using Domain.AccountingPeriods.Queries;

namespace Domain.Accounts.Queries;

/// <summary>
/// Interface for retrieving persisted Account facts used by read operations.
/// </summary>
public interface IAccountQueryRepository
{
    /// <summary>
    /// Retrieves the Account with the specified ID, or null when it does not exist.
    /// </summary>
    Task<Account?> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Accounts matching the provided query.
    /// </summary>
    Task<QueryPage<Account>> GetAsync(AccountQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account Balances matching the provided query.
    /// </summary>
    Task<QueryPage<AccountBalance>> GetBalancesAsync(
        AccountBalanceQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Accounts matching the provided range filter.
    /// </summary>
    Task<IReadOnlyCollection<Account>> GetRangeAccountsAsync(
        AccountFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available Account names.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetAllNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available Account financial institutions.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetAllFinancialInstitutionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account balance facts for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<AccountPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account boundary balances for a date range.
    /// </summary>
    Task<IReadOnlyCollection<AccountRangeBalance>> GetDateRangeBalancesAsync(
        AccountFilter filter,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Account balance history facts for the provided date range and Accounts.
    /// </summary>
    Task<IReadOnlyCollection<AccountDateBalanceFact>> GetDateBalanceFactsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves income destination facts for the Accounts within the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetDateRangeIncomeFactsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves spending facts for the Accounts within the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetDateRangeSpendingFactsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves income destination facts for the Accounts within the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetAccountingPeriodRangeIncomeFactsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves spending facts for the Accounts within the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetAccountingPeriodRangeSpendingFactsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);
}
