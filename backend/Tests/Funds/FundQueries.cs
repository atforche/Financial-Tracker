using Models;
using Models.Funds;
using Tests.Infrastructure;

namespace Tests.Funds;

/// <summary>
/// Retrieves fund data exposed by the application.
/// </summary>
internal sealed class FundQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets the current balance for a fund.
    /// </summary>
    public async Task<FundBalanceSnapshot> GetBalanceAsync(FundHandle fund)
    {
        CollectionModel<FundWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundWithBalanceModel model = response.Items.Single(item => item.Id == fund.Id);
        return new FundBalanceSnapshot(model.CurrentBalance.PostedBalance, model.CurrentBalance.BalanceIncludingPending);
    }
}
