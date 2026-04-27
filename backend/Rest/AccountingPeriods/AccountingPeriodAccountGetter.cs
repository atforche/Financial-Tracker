using System.Globalization;
using Domain.AccountingPeriods;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Accounts for an Accounting period based on specified criteria
/// </summary>
public class AccountingPeriodAccountGetter(IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository)
{
    /// <summary>
    /// Gets the Accounts within the specified Accounting Period that match the specified criteria
    /// </summary>
    public CollectionModel<AccountingPeriodAccountModel> Get(
        AccountingPeriodId accountingPeriodId,
        AccountingPeriodAccountQueryParameterModel request)
    {
        AccountingPeriodBalanceHistory accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriodId);
        var results = accountingPeriodBalanceHistory.AccountBalances.Select(AccountingPeriodAccountConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(account =>
                account.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                account.Type.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                account.OpeningBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                account.ClosingBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodAccountSortOrderModel.Name)
        {
            results = results.OrderBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.Type)
        {
            results = results.OrderBy(account => account.Type).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.TypeDescending)
        {
            results = results.OrderByDescending(account => account.Type).ThenByDescending(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.OpeningBalance)
        {
            results = results.OrderBy(account => account.OpeningBalance).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.OpeningBalanceDescending)
        {
            results = results.OrderByDescending(account => account.OpeningBalance).ThenByDescending(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.ClosingBalance)
        {
            results = results.OrderBy(account => account.ClosingBalance).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrderModel.ClosingBalanceDescending)
        {
            results = results.OrderByDescending(account => account.ClosingBalance).ThenByDescending(account => account.Name).ToList();
        }
        return new CollectionModel<AccountingPeriodAccountModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}