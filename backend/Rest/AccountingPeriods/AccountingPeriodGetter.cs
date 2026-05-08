using System.Globalization;
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

        if (request.Search != null)
        {
            results = results.Where(accountingPeriod =>
                accountingPeriod.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                accountingPeriod.Year.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                accountingPeriod.Month.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
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