namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying Accounts and their Balances.
/// </summary>
public sealed class AccountQueryService(
    IAccountRepository accountRepository,
    IAccountQueryRepository accountQueryRepository)
{
    /// <summary>
    /// Retrieves the Account with the specified ID, or null when it does not exist.
    /// </summary>
    public Account? GetById(Guid accountId)
    {
        if (accountRepository.TryGetById(accountId, out Account? account))
        {
            return account;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the Accounts matching the provided query.
    /// </summary>
    public Task<QueryPage<Account>> GetAsync(AccountQuery query, CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Accounts and their interpreted current balances.
    /// </summary>
    public Task<QueryPage<AccountBalance>> GetWithBalancesAsync(
        AccountBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetBalancesAsync(query, cancellationToken);
}