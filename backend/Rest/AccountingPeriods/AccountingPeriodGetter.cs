using Domain.AccountingPeriods;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Accounting Periods based on specified criteria
/// </summary>
public class AccountingPeriodGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Gets the Accounting Periods that match the specified criteria
    /// </summary>
    public CollectionModel<AccountingPeriodModel> Get(AccountingPeriodQueryParameterModel request)
    {
        var results = accountingPeriodRepository.GetAll().Select(accountingPeriodConverter.ToModel).ToList();

        if (request.Years != null && request.Years.Count > 0)
        {
            results = results.Where(accountingPeriod => request.Years.Contains(accountingPeriod.Year)).ToList();
        }
        if (request.Months != null && request.Months.Count > 0)
        {
            results = results.Where(accountingPeriod => request.Months.Contains(accountingPeriod.Month)).ToList();
        }
        if (request.Sort is null or AccountingPeriodSortOrderModel.DateDescending)
        {
            results = results.OrderByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.Date)
        {
            results = results.OrderBy(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.IsOpen)
        {
            results = results.OrderBy(accountingPeriod => accountingPeriod.IsOpen).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.IsOpenDescending)
        {
            results = results.OrderByDescending(accountingPeriod => accountingPeriod.IsOpen).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.OpeningBalance)
        {
            results = results.OrderBy(accountingPeriod => accountingPeriod.OpeningBalance).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.OpeningBalanceDescending)
        {
            results = results.OrderByDescending(accountingPeriod => accountingPeriod.OpeningBalance).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.ClosingBalance)
        {
            results = results.OrderBy(accountingPeriod => accountingPeriod.ClosingBalance).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        else if (request.Sort == AccountingPeriodSortOrderModel.ClosingBalanceDescending)
        {
            results = results.OrderByDescending(accountingPeriod => accountingPeriod.ClosingBalance).ThenByDescending(accountingPeriod => (accountingPeriod.Year, accountingPeriod.Month)).ToList();
        }
        return new CollectionModel<AccountingPeriodModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}