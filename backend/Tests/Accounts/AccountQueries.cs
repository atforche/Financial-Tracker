using Models;
using Models.Accounts;
using Tests.Infrastructure;
using Tests.Transactions;

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

    /// <summary>
    /// Gets a balance event for an account and transaction within a date range.
    /// </summary>
    public async Task<AccountBalanceEventSnapshot> GetBalanceEventAsync(
        AccountHandle account,
        TransactionHandle transaction,
        DateOnly start,
        DateOnly end)
    {
        CollectionModel<AccountBalanceEventModel> response = await apiClient.GetAsync<CollectionModel<AccountBalanceEventModel>>(
            $"/accounts/{account.Id}/balance-events?range.start={start:yyyy-MM-dd}&range.end={end:yyyy-MM-dd}");
        AccountBalanceEventModel model = response.Items.Single(item => item.TransactionId == transaction.Id);
        return new AccountBalanceEventSnapshot(
            new AccountBalanceSnapshot(model.PreviousBalance.PostedBalance, model.PreviousBalance.BalanceIncludingPending),
            new AccountBalanceSnapshot(model.NewBalance.PostedBalance, model.NewBalance.BalanceIncludingPending),
            model.IsPosted);
    }
}
