using Models;
using Models.FundGoals;
using Models.Funds;

namespace Tests.Support;

/// <summary>
/// Retrieves fund data exposed by the application.
/// </summary>
internal sealed class FundQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets the current balance for a fund.
    /// </summary>
    public async Task<BalanceSnapshot> GetBalanceAsync(FundHandle fund)
    {
        CollectionModel<FundWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundWithBalanceModel model = response.Items.Single(item => item.Id == fund.Id);
        return new BalanceSnapshot(model.CurrentBalance.PostedBalance, model.PendingAmount);
    }

    /// <summary>
    /// Gets the Unassigned fund and its goal for an accounting period.
    /// </summary>
    public async Task<FundHandle> GetUnassignedAsync(AccountingPeriodHandle period)
    {
        CollectionModel<FundWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundWithBalanceModel model = response.Items.Single(item => item.Name == "Unassigned");
        FundGoalModel goal = await apiClient.GetAsync<FundGoalModel>($"/fund-goals/fund/{model.Id}?accountingPeriodId={period.Id}");
        return new FundHandle(model.Id, model.Name, new FundGoalHandle(goal.Id, model.Id));
    }
}
