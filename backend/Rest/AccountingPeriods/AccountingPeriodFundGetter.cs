using Domain.AccountingPeriods;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Funds for an Accounting Period based on specified criteria
/// </summary>
public class AccountingPeriodFundGetter(IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository)
{
    /// <summary>
    /// Gets the Funds within the specified Accounting Period that match the specified criteria
    /// </summary>
    public CollectionModel<AccountingPeriodFundModel> Get(
        AccountingPeriodId accountingPeriodId,
        AccountingPeriodFundQueryParameterModel request)
    {
        AccountingPeriodBalanceHistory accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriodId);
        var results = accountingPeriodBalanceHistory.FundBalances.Select(AccountingPeriodFundConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(fund =>
                fund.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fund.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodFundSortOrderModel.Name)
        {
            results = results.OrderBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.OpeningBalance)
        {
            results = results.OrderBy(fund => fund.OpeningBalance).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.OpeningBalanceDescending)
        {
            results = results.OrderByDescending(fund => fund.OpeningBalance).ThenByDescending(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.AmountAssigned)
        {
            results = results.OrderBy(fund => fund.AmountAssigned).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.AmountAssignedDescending)
        {
            results = results.OrderByDescending(fund => fund.AmountAssigned).ThenByDescending(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.AmountSpent)
        {
            results = results.OrderBy(fund => fund.AmountSpent).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.AmountSpentDescending)
        {
            results = results.OrderByDescending(fund => fund.AmountSpent).ThenByDescending(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.ClosingBalance)
        {
            results = results.OrderBy(fund => fund.ClosingBalance).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrderModel.ClosingBalanceDescending)
        {
            results = results.OrderByDescending(fund => fund.ClosingBalance).ThenByDescending(fund => fund.Name).ToList();
        }
        return new CollectionModel<AccountingPeriodFundModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count
        };
    }
}