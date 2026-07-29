using Models;
using Models.Accounts;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Retrieves account data exposed by the application.
/// </summary>
internal sealed class AccountQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets the current balance for an account.
    /// </summary>
    public async Task<AccountBalanceSnapshot> GetBalanceAsync(AccountHandle account)
    {
        CollectionModel<AccountWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        AccountWithBalanceModel model = response.Items.Single(item => item.Id == account.Id);
        return new AccountBalanceSnapshot(model.CurrentBalance.PostedBalance, model.CurrentBalance.BalanceIncludingPending);
    }
}