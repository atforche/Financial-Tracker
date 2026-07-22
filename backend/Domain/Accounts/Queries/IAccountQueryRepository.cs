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
}