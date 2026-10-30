using Models.AccountingPeriods;
using Models.Transactions.Create;

namespace Tests.Support;

/// <summary>
/// Adds Expected Income Sources to accounting periods, so tests can plan income as one of them.
/// </summary>
internal static class ExpectedIncomeSourceSetup
{
    /// <summary>
    /// Adds an Expected Income Source with one income line, and optionally one expected payment date and an untracked
    /// transfer, to the provided period and returns its ID.
    /// </summary>
    public static async Task<Guid> AddExpectedIncomeSourceAsync(
        this TestApiClient api,
        AccountingPeriodHandle period,
        string name,
        decimal amount,
        DateOnly? date = null,
        decimal untrackedAmount = 0m)
    {
        AccountingPeriodWithBalanceModel updated = await api.PostAsync<ExpectedIncomeSourceRequestModel, AccountingPeriodWithBalanceModel>(
            $"/accounting-periods/{period.Id}/expected-income-sources",
            new ExpectedIncomeSourceRequestModel
            {
                Name = name,
                IncomeLines = [new CreateIncomeLineModel { Description = name, Amount = amount }],
                IncomeDeductions = [],
                UntrackedTransfers = untrackedAmount == 0m
                    ? []
                    : [new ExpectedUntrackedIncomeTransferRequestModel { Description = "Retirement", Amount = untrackedAmount }],
                ExpectedDates = date is DateOnly expectedDate ? [expectedDate] : [],
            });
        return updated.ExpectedIncomeSources.Single(source => source.Name == name).Id;
    }
}
