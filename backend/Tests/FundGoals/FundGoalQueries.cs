using Models.FundGoals;
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
        FundAvailabilityModel model = await apiClient.GetAsync<FundAvailabilityModel>($"/fund-goals/{fundGoal.Id}/availability");
        return new FundGoalAvailabilitySnapshot(model.AvailableBalance, model.AvailableBalanceIncludingPending);
    }
}