using Models.AccountingPeriods;

namespace Tests.Support;

/// <summary>
/// Builds an accounting period.
/// </summary>
internal sealed class AccountingPeriodBuilder(TestApiClient apiClient, int year, int month)
{
    /// <summary>
    /// Creates the accounting period.
    /// </summary>
    public async Task<AccountingPeriodHandle> CreateAsync()
    {
        AccountingPeriodWithBalanceModel model = await apiClient.PostAsync<CreateAccountingPeriodModel, AccountingPeriodWithBalanceModel>("/accounting-periods", new CreateAccountingPeriodModel
        {
            Year = year,
            Month = month,
        });
        return new AccountingPeriodHandle(model.Id, model.Name, year, month);
    }
}
