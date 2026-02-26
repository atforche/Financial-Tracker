using Domain.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Filter class responsible for filtering Accounting Periods based on the specified criteria
/// </summary>
internal sealed class AccountingPeriodFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Accounting Periods that match the specified criteria
    /// </summary>
    public List<AccountingPeriodSortModel> Get(GetAccountingPeriodsRequest request)
    {
        IQueryable<AccountingPeriod> results = databaseContext.AccountingPeriods.AsQueryable();
        if (request.Years != null)
        {
            results = results.Where(accountingPeriod => request.Years.Contains(accountingPeriod.Year));
        }
        if (request.Months != null)
        {
            results = results.Where(accountingPeriod => request.Months.Contains(accountingPeriod.Month));
        }
        if (request.IsOpen != null)
        {
            results = results.Where(accountingPeriod => accountingPeriod.IsOpen == request.IsOpen);
        }
        return results.Select(accountingPeriod => new AccountingPeriodSortModel { AccountingPeriod = accountingPeriod }).ToList();
    }
}