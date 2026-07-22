namespace Domain.Accounts.Queries;

/// <summary>
/// Interface for retrieving persisted Account facts used by read operations.
/// </summary>
public interface IAccountQueryRepository
{
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
    /// Retrieves Account balance facts for the provided Accounting Periods.
    /// </summary>
    Task<IReadOnlyCollection<AccountPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);
}