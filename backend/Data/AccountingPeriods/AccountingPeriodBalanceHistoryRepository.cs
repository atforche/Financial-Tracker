using System.Globalization;
using Domain.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Repository that allows Accounting Period Balance Histories to be persisted to the database
/// </summary>
public class AccountingPeriodBalanceHistoryRepository(DatabaseContext databaseContext) : IAccountingPeriodBalanceHistoryRepository
{
    #region AccountingPeriodBalanceHistoryRepository

    /// <inheritdoc/>
    public AccountingPeriodBalanceHistory GetForAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.AccountingPeriodBalanceHistories
            .SingleOrDefault(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod.Id == accountingPeriodId)
        ?? databaseContext.AccountingPeriodBalanceHistories.Local
            .Single(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod.Id == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => databaseContext.Add(accountingPeriodBalanceHistory);

    /// <inheritdoc/>
    public void Delete(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => databaseContext.Remove(accountingPeriodBalanceHistory);

    #endregion

    /// <summary>
    /// Gets the Funds within the specified Accounting Period that match the specified criteria
    /// </summary>
    public PaginatedCollection<FundAccountingPeriodBalanceHistory> GetManyFunds(AccountingPeriodId accountingPeriodId, GetAccountingPeriodFundsRequest request)
    {
        AccountingPeriodBalanceHistory accountingPeriodBalanceHistory = GetForAccountingPeriod(accountingPeriodId);
        var fundBalanceHistories = accountingPeriodBalanceHistory.FundBalances.ToList();
        if (request.Search != null)
        {
            fundBalanceHistories = fundBalanceHistories.Where(fundBalanceHistory =>
                fundBalanceHistory.Fund.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fundBalanceHistory.Fund.Type.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fundBalanceHistory.Fund.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodFundSortOrder.Name)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.NameDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.Type)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.Fund.Type).ThenBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.TypeDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Type).ThenByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.OpeningBalance)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.OpeningBalance).ThenBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.OpeningBalanceDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.OpeningBalance).ThenByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.AmountAssigned)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.AmountAssigned).ThenBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.AmountAssignedDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.AmountAssigned).ThenByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.AmountSpent)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.AmountSpent).ThenBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.AmountSpentDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.AmountSpent).ThenByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.ClosingBalance)
        {
            fundBalanceHistories = fundBalanceHistories.OrderBy(fundBalanceHistory => fundBalanceHistory.ClosingBalance).ThenBy(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodFundSortOrder.ClosingBalanceDescending)
        {
            fundBalanceHistories = fundBalanceHistories.OrderByDescending(fundBalanceHistory => fundBalanceHistory.ClosingBalance).ThenByDescending(fundBalanceHistory => fundBalanceHistory.Fund.Name).ToList();
        }
        return new PaginatedCollection<FundAccountingPeriodBalanceHistory>
        {
            Items = fundBalanceHistories.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = fundBalanceHistories.Count,
        };
    }

    /// <summary>
    /// Gets the Accounts within the specified Accounting Period that match the specified criteria
    /// </summary>
    public PaginatedCollection<AccountAccountingPeriodBalanceHistory> GetManyAccounts(AccountingPeriodId accountingPeriodId, GetAccountingPeriodAccountsRequest request)
    {
        AccountingPeriodBalanceHistory accountingPeriodBalanceHistory = GetForAccountingPeriod(accountingPeriodId);
        var accountBalanceHistories = accountingPeriodBalanceHistory.AccountBalances.ToList();
        if (request.Search != null)
        {
            accountBalanceHistories = accountBalanceHistories.Where(accountBalanceHistory =>
                accountBalanceHistory.Account.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                accountBalanceHistory.OpeningBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                accountBalanceHistory.ClosingBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodAccountSortOrder.Name)
        {
            accountBalanceHistories = accountBalanceHistories.OrderBy(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.NameDescending)
        {
            accountBalanceHistories = accountBalanceHistories.OrderByDescending(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.Type)
        {
            accountBalanceHistories = accountBalanceHistories.OrderBy(accountBalanceHistory => accountBalanceHistory.Account.Type).ThenBy(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.TypeDescending)
        {
            accountBalanceHistories = accountBalanceHistories.OrderByDescending(accountBalanceHistory => accountBalanceHistory.Account.Type).ThenByDescending(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.OpeningBalance)
        {
            accountBalanceHistories = accountBalanceHistories.OrderBy(accountBalanceHistory => accountBalanceHistory.OpeningBalance).ThenBy(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.OpeningBalanceDescending)
        {
            accountBalanceHistories = accountBalanceHistories.OrderByDescending(accountBalanceHistory => accountBalanceHistory.OpeningBalance).ThenByDescending(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.ClosingBalance)
        {
            accountBalanceHistories = accountBalanceHistories.OrderBy(accountBalanceHistory => accountBalanceHistory.ClosingBalance).ThenBy(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        else if (request.Sort == AccountingPeriodAccountSortOrder.ClosingBalanceDescending)
        {
            accountBalanceHistories = accountBalanceHistories.OrderByDescending(accountBalanceHistory => accountBalanceHistory.ClosingBalance).ThenByDescending(accountBalanceHistory => accountBalanceHistory.Account.Name).ToList();
        }
        return new PaginatedCollection<AccountAccountingPeriodBalanceHistory>
        {
            Items = accountBalanceHistories.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = accountBalanceHistories.Count,
        };
    }
}