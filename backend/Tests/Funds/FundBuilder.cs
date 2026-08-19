using Models.FundGoals;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Funds;

/// <summary>
/// Builds a fund in an accounting period.
/// </summary>
internal sealed class FundBuilder(TestApiClient apiClient, string name)
{
    private AccountingPeriodHandle? _period;

    /// <summary>
    /// Sets the fund's opening accounting period.
    /// </summary>
    public FundBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Creates the fund.
    /// </summary>
    public async Task<FundHandle> CreateAsync()
    {
        AccountingPeriodHandle period = _period ?? throw new InvalidOperationException("A fund must belong to an accounting period.");
        FundModel model = await apiClient.PostAsync<CreateFundModel, FundModel>("/funds", new CreateFundModel
        {
            Name = name,
            Description = name,
            AccountingPeriodId = period.Id
        });
        FundGoalModel fundGoal = await apiClient.GetAsync<FundGoalModel>($"/fund-goals/fund/{model.Id}?accountingPeriodId={period.Id}");
        return new FundHandle(model.Id, model.Name, new FundGoalHandle(fundGoal.Id));
    }
}
