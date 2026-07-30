using Models.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.AccountingPeriods;

/// <summary>
/// Retrieves accounting-period data exposed by the application.
/// </summary>
internal sealed class AccountingPeriodQueries(TestApiClient apiClient)
{
    /// <summary>
    /// Gets the balance snapshot for an accounting period.
    /// </summary>
    public async Task<AccountingPeriodBalanceSnapshot> GetBalanceAsync(AccountingPeriodHandle period)
    {
        AccountingPeriodWithBalanceModel model = await apiClient.GetAsync<AccountingPeriodWithBalanceModel>($"/accounting-periods/{period.Id}");
        return new AccountingPeriodBalanceSnapshot(model.OpeningBalance, model.ClosingBalance);
    }
}