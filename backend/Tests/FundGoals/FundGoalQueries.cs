using Models;
using Models.Funds;
using Tests.Funds;
using Tests.Infrastructure;

namespace Tests.FundGoals;

/// <summary>
/// Retrieves fund-goal data exposed by the application.
/// </summary>
internal sealed class FundGoalQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets current availability for a fund goal.
    /// </summary>
    public async Task<FundGoalAvailabilitySnapshot> GetAvailabilityAsync(FundGoalHandle fundGoal)
    {
        CollectionModel<FundWithBalanceModel> response = await apiClient.GetAsync<CollectionModel<FundWithBalanceModel>>("/funds/with-balances");
        FundBalanceModel balance = response.Items.Single(fund => fund.Id == fundGoal.FundId).CurrentBalance;
        return new FundGoalAvailabilitySnapshot(balance.PostedBalance, balance.BalanceIncludingPending);
    }
}
