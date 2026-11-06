using Models;
using Models.AccountGoals;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;

namespace Tests.Support;

/// <summary>
/// Reads the pieces of the API's combined responses that individual tests assert on.
/// </summary>
internal static class EntityQueryExtensions
{
    /// <summary>
    /// Gets the Account balance events at the provided with-totals URI.
    /// </summary>
    public static async Task<CollectionModel<AccountBalanceEventModel>> GetAccountEventsAsync(this TestApiClient api, string uri)
    {
        AccountBalanceEventsWithTotalsModel model = await api.GetAsync<AccountBalanceEventsWithTotalsModel>(uri);
        return new CollectionModel<AccountBalanceEventModel> { Items = model.Events, TotalCount = model.TotalCount };
    }

    /// <summary>
    /// Gets the Fund balance events at the provided with-totals URI.
    /// </summary>
    public static async Task<CollectionModel<FundBalanceEventModel>> GetFundEventsAsync(this TestApiClient api, string uri)
    {
        FundBalanceEventsWithTotalsModel model = await api.GetAsync<FundBalanceEventsWithTotalsModel>(uri);
        return new CollectionModel<FundBalanceEventModel> { Items = model.Events, TotalCount = model.TotalCount };
    }

    /// <summary>
    /// Gets the Fund totals at the provided with-totals URI.
    /// </summary>
    public static async Task<BalanceEventTotalsModel> GetFundTotalsAsync(this TestApiClient api, string uri) =>
        (await api.GetAsync<FundBalanceEventsWithTotalsModel>(uri)).Totals;

    /// <summary>
    /// Gets a Fund's goal for an Accounting Period, or its only goal when no period is provided.
    /// </summary>
    public static async Task<FundGoalModel> GetFundGoalAsync(this TestApiClient api, Guid fundId, Guid? accountingPeriodId = null)
    {
        string periodFilter = accountingPeriodId == null ? "" : $"&filter.accountingPeriodIds={accountingPeriodId}";
        CollectionModel<FundGoalModel> goals = await api.GetAsync<CollectionModel<FundGoalModel>>($"/fund-goals?filter.fundIds={fundId}{periodFilter}");
        return Assert.Single(goals.Items);
    }

    /// <summary>
    /// Gets an Account's goal for an Accounting Period.
    /// </summary>
    public static async Task<AccountGoalModel> GetAccountGoalAsync(this TestApiClient api, Guid accountId, Guid accountingPeriodId)
    {
        PeriodAccountDetailModel detail = await api.GetAsync<PeriodAccountDetailModel>($"/accounting-periods/{accountingPeriodId}/accounts/{accountId}");
        return Assert.IsType<AccountGoalModel>(detail.Goal);
    }
}
