using Models;
using Models.Accounts;

namespace Tests.Support;

/// <summary>
/// Retrieves account data exposed by the application.
/// </summary>
internal sealed class AccountQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets the current balance for an account.
    /// </summary>
    public async Task<BalanceSnapshot> GetBalanceAsync(AccountHandle account)
    {
        CollectionModel<AccountWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        AccountWithBalanceModel model = response.Items.Single(item => item.Id == account.Id);
        return new BalanceSnapshot(model.CurrentBalance.PostedBalance, model.PendingAmount);
    }
}
