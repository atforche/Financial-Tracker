using Models.FundGoals;
using Models.Funds;

namespace Tests.Support;

/// <summary>
/// Builds a fund in an accounting period.
/// </summary>
internal sealed class FundBuilder(TestApiClient apiClient, string name)
{
    private AccountingPeriodHandle? _period;
    private decimal? _plannedMonthlyContribution;
    private decimal? _minimumEndingBalance;
    private decimal? _maximumEndingBalance;

    /// <summary>
    /// Sets the fund's opening accounting period.
    /// </summary>
    public FundBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Configures the fund's goal for its opening accounting period.
    /// </summary>
    public FundBuilder WithGoal(decimal? plannedMonthlyContribution, decimal? minimumEndingBalance = null, decimal? maximumEndingBalance = null)
    {
        _plannedMonthlyContribution = plannedMonthlyContribution;
        _minimumEndingBalance = minimumEndingBalance;
        _maximumEndingBalance = maximumEndingBalance;
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
            AccountingPeriodId = period.Id,
            PlannedMonthlyContribution = _plannedMonthlyContribution,
            MinimumEndingBalance = _minimumEndingBalance,
            MaximumEndingBalance = _maximumEndingBalance
        });
        FundGoalModel fundGoal = await apiClient.GetAsync<FundGoalModel>($"/fund-goals/fund/{model.Id}?accountingPeriodId={period.Id}");
        return new FundHandle(model.Id, model.Name, new FundGoalHandle(fundGoal.Id, model.Id));
    }
}
