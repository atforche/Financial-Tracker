using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.Funds;

namespace Data;

/// <summary>
/// Read-only range queries shared by Account and Fund workspaces.
/// </summary>
public sealed class FinancialRangeQueryService(DatabaseContext databaseContext)
{
    /// <summary>
    /// Retrieves Account balances over an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<AccountsInAccountingPeriodRangeModel?> GetAccountsAsync(AccountsInAccountingPeriodRangeQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        List<AccountingPeriod>? periods = await GetPeriodsAsync(request.Range, cancellationToken);
        if (periods == null)
        {
            return null;
        }

        List<AccountingPeriodBalanceHistory> histories = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .Include(history => history.AccountingPeriod)
            .Include(history => history.AccountBalances).ThenInclude(history => history.Account)
            .Where(history => periods.Select(period => period.Id).Contains(history.AccountingPeriod.Id))
            .ToListAsync(cancellationToken);
        if (histories.Count != periods.Count)
        {
            return null;
        }

        IQueryable<Account> accounts = ApplyFilter(databaseContext.Accounts.AsNoTracking(), request.Filter);
        List<Account> matching = await accounts.ToListAsync(cancellationToken);
        var matchingIds = matching.Select(account => account.Id).ToHashSet();
        AccountingPeriodBalanceHistory first = histories.Single(history => history.AccountingPeriod.Id == periods[0].Id);
        AccountingPeriodBalanceHistory last = histories.Single(history => history.AccountingPeriod.Id == periods[^1].Id);
        var rows = matching.Select(account => new AccountWithBalanceRangeModel
        {
            Id = account.Id.Value,
            Name = account.Name,
            Type = (AccountTypeModel)account.Type,
            StartingBalance = first.AccountBalances.SingleOrDefault(balance => balance.Account.Id == account.Id)?.OpeningBalance ?? account.OnboardedBalance ?? 0,
            EndingBalance = last.AccountBalances.SingleOrDefault(balance => balance.Account.Id == account.Id)?.ClosingBalance ?? account.OnboardedBalance ?? 0,
        }).ToList();
        rows = Sort(rows, request.Sort).ToList();
        (IncomeAmountModel income, decimal spending) = await GetTotalsAsync(periods.Select(period => period.Id.Value).ToList(), cancellationToken);
        return new AccountsInAccountingPeriodRangeModel
        {
            Accounts = new CollectionModel<AccountWithBalanceRangeModel> { Items = rows.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(), TotalCount = rows.Count },
            AvailableAccountNames = await databaseContext.Accounts.AsNoTracking().OrderBy(account => account.Name).Select(account => account.Name).ToListAsync(cancellationToken),
            TotalIncome = income,
            TotalSpending = spending,
            AccountingPeriods = periods.Select(period =>
            {
                AccountingPeriodBalanceHistory history = histories.Single(item => item.AccountingPeriod.Id == period.Id);
                return new AccountBalanceSummaryByPeriodModel { AccountingPeriod = ToModel(period), OpeningBalance = SummarizeAccounts(history.AccountBalances.Where(item => matchingIds.Contains(item.Account.Id)), true), ClosingBalance = SummarizeAccounts(history.AccountBalances.Where(item => matchingIds.Contains(item.Account.Id)), false) };
            }).ToList(),
        };
    }

    /// <summary>
    /// Retrieves Fund balances over an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<FundsInAccountingPeriodRangeModel?> GetFundsAsync(FundsInAccountingPeriodRangeQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        List<AccountingPeriod>? periods = await GetPeriodsAsync(request.Range, cancellationToken);
        if (periods == null)
        {
            return null;
        }

        List<AccountingPeriodBalanceHistory> histories = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .Include(history => history.AccountingPeriod)
            .Include(history => history.FundBalances).ThenInclude(history => history.Fund)
            .Where(history => periods.Select(period => period.Id).Contains(history.AccountingPeriod.Id)).ToListAsync(cancellationToken);
        if (histories.Count != periods.Count)
        {
            return null;
        }

        IQueryable<Fund> funds = ApplyFilter(databaseContext.Funds.AsNoTracking(), request.Filter);
        List<Fund> matching = await funds.ToListAsync(cancellationToken);
        var matchingIds = matching.Select(fund => fund.Id).ToHashSet();
        AccountingPeriodBalanceHistory first = histories.Single(history => history.AccountingPeriod.Id == periods[0].Id);
        AccountingPeriodBalanceHistory last = histories.Single(history => history.AccountingPeriod.Id == periods[^1].Id);
        var rows = matching.Select(fund => new FundWithBalanceRangeModel { Id = fund.Id.Value, Name = fund.Name, Description = fund.Description, StartingBalance = first.FundBalances.SingleOrDefault(balance => balance.Fund.Id == fund.Id)?.OpeningBalance ?? fund.OnboardedBalance ?? 0, EndingBalance = last.FundBalances.SingleOrDefault(balance => balance.Fund.Id == fund.Id)?.ClosingBalance ?? fund.OnboardedBalance ?? 0 }).ToList();
        rows = Sort(rows, request.Sort).ToList();
        (IncomeAmountModel income, decimal spending) = await GetTotalsAsync(periods.Select(period => period.Id.Value).ToList(), cancellationToken);
        return new FundsInAccountingPeriodRangeModel
        {
            Funds = new CollectionModel<FundWithBalanceRangeModel> { Items = rows.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(), TotalCount = rows.Count },
            AvailableFundNames = await databaseContext.Funds.AsNoTracking().OrderBy(fund => fund.Name).Select(fund => fund.Name).ToListAsync(cancellationToken),
            TotalIncome = income,
            TotalSpending = spending,
            AccountingPeriods = periods.Select(period =>
            {
                AccountingPeriodBalanceHistory history = histories.Single(item => item.AccountingPeriod.Id == period.Id);
                return new FundBalanceSummaryByPeriodModel { AccountingPeriod = ToModel(period), OpeningBalance = SummarizeFunds(history.FundBalances.Where(item => matchingIds.Contains(item.Fund.Id)), true), ClosingBalance = SummarizeFunds(history.FundBalances.Where(item => matchingIds.Contains(item.Fund.Id)), false) };
            }).ToList(),
        };
    }
    /// <summary>
    /// Retrieves Account balances over a date range.
    /// </summary>
    public async Task<AccountsInDateRangeModel> GetAccountsAsync(AccountsInDateRangeQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Account> accounts = ApplyFilter(databaseContext.Accounts.AsNoTracking(), request.Filter);
        List<AccountWithBalanceRangeModel> rows = await accounts.Select(account => new AccountWithBalanceRangeModel
        {
            Id = account.Id.Value,
            Name = account.Name,
            Type = (AccountTypeModel)account.Type,
            StartingBalance = databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == account.Id && history.Date < request.Range.Start)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? account.OnboardedBalance ?? 0,
            EndingBalance = databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == account.Id && history.Date <= request.Range.End)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? account.OnboardedBalance ?? 0,
        }).ToListAsync(cancellationToken);
        rows = Sort(rows, request.Sort).ToList();
        List<AccountBalanceHistory> histories = await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => history.Date <= request.Range.End)
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        List<Account> matchingAccounts = await accounts.ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountBalanceSummaryByDateModel> dates = GetDates(request.Range.Start, request.Range.End)
            .Select(date =>
            {
                var balances = matchingAccounts.Select(account =>
                {
                    decimal balance = account.DateOpened is DateOnly opened && date < opened
                        ? 0
                        : histories.LastOrDefault(history => history.Account.Id == account.Id && history.Date <= date)?.PostedBalance
                            ?? account.OnboardedBalance ?? 0;
                    return (account.Type, Balance: account.Type.IsDebt() ? -balance : balance);
                }).ToList();
                return new AccountBalanceSummaryByDateModel
                {
                    Date = date,
                    TotalBalance = balances.Sum(item => item.Balance),
                    TotalTrackedBalance = balances.Where(item => item.Type.IsTracked()).Sum(item => item.Balance),
                    TotalUntrackedBalance = balances.Where(item => !item.Type.IsTracked()).Sum(item => item.Balance),
                    BalanceByAccountType = balances.GroupBy(item => item.Type).OrderBy(group => group.Key)
                        .Select(group => new AccountTypeBalanceModel
                        {
                            AccountType = (AccountTypeModel)group.Key,
                            TotalBalance = group.Sum(item => item.Balance),
                        }).ToList(),
                };
            }).ToList();
        (IncomeAmountModel income, decimal spending) = await GetTotalsAsync(request.Range.Start, request.Range.End, cancellationToken);
        return new AccountsInDateRangeModel
        {
            Accounts = new CollectionModel<AccountWithBalanceRangeModel> { Items = rows.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(), TotalCount = rows.Count },
            AvailableAccountNames = await databaseContext.Accounts.AsNoTracking().OrderBy(account => account.Name).Select(account => account.Name).ToListAsync(cancellationToken),
            TotalIncome = income,
            TotalSpending = spending,
            Dates = dates,
        };
    }

    /// <summary>
    /// Retrieves Fund balances over a date range.
    /// </summary>
    public async Task<FundsInDateRangeModel> GetFundsAsync(FundsInDateRangeQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Fund> funds = ApplyFilter(databaseContext.Funds.AsNoTracking(), request.Filter);
        List<FundWithBalanceRangeModel> rows = await funds.Select(fund => new FundWithBalanceRangeModel
        {
            Id = fund.Id.Value,
            Name = fund.Name,
            Description = fund.Description,
            StartingBalance = databaseContext.FundBalanceHistories.Where(history => history.FundId == fund.Id && history.Date < request.Range.Start)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0,
            EndingBalance = databaseContext.FundBalanceHistories.Where(history => history.FundId == fund.Id && history.Date <= request.Range.End)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0,
        }).ToListAsync(cancellationToken);
        rows = Sort(rows, request.Sort).ToList();
        List<FundBalanceHistory> histories = await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => history.Date <= request.Range.End)
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        List<Fund> matchingFunds = await funds.ToListAsync(cancellationToken);
        IReadOnlyCollection<FundBalanceSummaryByDateModel> dates = GetDates(request.Range.Start, request.Range.End)
            .Select(date =>
            {
                var balances = matchingFunds.Select(fund => new
                {
                    fund.Id,
                    Balance = histories.LastOrDefault(history => history.FundId == fund.Id && history.Date <= date)?.PostedBalance
                        ?? fund.OnboardedBalance ?? 0,
                }).ToList();
                return new FundBalanceSummaryByDateModel
                {
                    Date = date,
                    TotalBalance = balances.Sum(item => item.Balance),
                    TotalAssignedBalance = balances.Where(item => item.Id != Fund.UnassignedFundId).Sum(item => item.Balance),
                    TotalUnassignedBalance = balances.Where(item => item.Id == Fund.UnassignedFundId).Sum(item => item.Balance),
                };
            }).ToList();
        (IncomeAmountModel income, decimal spending) = await GetTotalsAsync(request.Range.Start, request.Range.End, cancellationToken);
        return new FundsInDateRangeModel
        {
            Funds = new CollectionModel<FundWithBalanceRangeModel> { Items = rows.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(), TotalCount = rows.Count },
            AvailableFundNames = await databaseContext.Funds.AsNoTracking().OrderBy(fund => fund.Name).Select(fund => fund.Name).ToListAsync(cancellationToken),
            TotalIncome = income,
            TotalSpending = spending,
            Dates = dates,
        };
    }

    /// <summary>
    /// Retrieves the total income and spending over a specified date range.
    /// </summary>
    private async Task<(IncomeAmountModel, decimal)> GetTotalsAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        IQueryable<IncomeTransaction> incomeQuery = databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>().Where(transaction => transaction.Date >= start && transaction.Date <= end);
        decimal total = await incomeQuery.SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        decimal tracked = await incomeQuery.SumAsync(transaction => (decimal?)transaction.TrackedAmount, cancellationToken) ?? 0;
        decimal spending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>().Where(transaction => transaction.Date >= start && transaction.Date <= end)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        return (new IncomeAmountModel { Total = total, Tracked = tracked, Untracked = total - tracked }, spending);
    }

    /// <summary>
    /// Retrieves the total income and spending over the specified accounting period range.
    /// </summary>
    private async Task<(IncomeAmountModel, decimal)> GetTotalsAsync(IReadOnlyCollection<Guid> periodIds, CancellationToken cancellationToken)
    {
        var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
        IQueryable<IncomeTransaction> incomeQuery = databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId));
        decimal total = await incomeQuery.SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        decimal tracked = await incomeQuery.SumAsync(transaction => (decimal?)transaction.TrackedAmount, cancellationToken) ?? 0;
        decimal spending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)).SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        return (new IncomeAmountModel { Total = total, Tracked = tracked, Untracked = total - tracked }, spending);
    }

    /// <summary>
    /// Gets the Accounting Periods over the provided range
    /// </summary>
    private async Task<List<AccountingPeriod>?> GetPeriodsAsync(AccountingPeriodRangeModel range, CancellationToken cancellationToken)
    {
        List<AccountingPeriod> endpoints = await databaseContext.AccountingPeriods.AsNoTracking().Where(period => period.Id.Value == range.Start || period.Id.Value == range.End).ToListAsync(cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == range.Start);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == range.End);
        if (start == null || end == null)
        {
            return null;
        }

        int startIndex = (start.Year * 12) + start.Month;
        int endIndex = (end.Year * 12) + end.Month;
        if (startIndex > endIndex)
        {
            return null;
        }

        List<AccountingPeriod> periods = await databaseContext.AccountingPeriods.AsNoTracking().Where(period => (period.Year * 12) + period.Month >= startIndex && (period.Year * 12) + period.Month <= endIndex).OrderBy(period => period.Year).ThenBy(period => period.Month).ToListAsync(cancellationToken);
        return periods.Count == endIndex - startIndex + 1 ? periods : null;
    }

    /// <summary>
    /// Converts the provided accounting period to an Accounting Period Model
    /// </summary>
    private static AccountingPeriodModel ToModel(AccountingPeriod period) => new() { Id = period.Id.Value, Year = period.Year, Month = period.Month, Name = period.Name, IsOpen = period.IsOpen };

    /// <summary>
    /// Summarizes the provided account balance histories
    /// </summary>
    private static AccountBalanceSummaryModel SummarizeAccounts(IEnumerable<AccountingPeriodAccountBalanceHistory> balances, bool opening)
    {
        var values = balances.Select(balance => (balance.Account.Type, Amount: (balance.Account.Type.IsDebt() ? -1 : 1) * (opening ? balance.OpeningBalance : balance.ClosingBalance))).ToList();
        return new AccountBalanceSummaryModel { TotalBalance = values.Sum(item => item.Amount), TotalTrackedBalance = values.Where(item => item.Type.IsTracked()).Sum(item => item.Amount), TotalUntrackedBalance = values.Where(item => !item.Type.IsTracked()).Sum(item => item.Amount), BalanceByAccountType = values.GroupBy(item => item.Type).OrderBy(group => group.Key).Select(group => new AccountTypeBalanceModel { AccountType = (AccountTypeModel)group.Key, TotalBalance = group.Sum(item => item.Amount) }).ToList() };
    }

    /// <summary>
    /// Summarizes the provided fund balance histories
    /// </summary>
    private static FundBalanceSummaryModel SummarizeFunds(IEnumerable<AccountingPeriodFundBalanceHistory> balances, bool opening)
    {
        var values = balances.Select(balance => (balance.Fund.Id, Amount: opening ? balance.OpeningBalance : balance.ClosingBalance)).ToList();
        return new FundBalanceSummaryModel { TotalBalance = values.Sum(item => item.Amount), TotalAssignedBalance = values.Where(item => item.Id != Fund.UnassignedFundId).Sum(item => item.Amount), TotalUnassignedBalance = values.Where(item => item.Id == Fund.UnassignedFundId).Sum(item => item.Amount) };
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<Account> ApplyFilter(IQueryable<Account> query, AccountFilterModel? filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.NameSearch))
        {
            query = query.Where(account => account.Name.Contains(filter.NameSearch));
        }
        if (filter?.Names is { Count: > 0 } names)
        {
            query = query.Where(account => names.Contains(account.Name));
        }
        if (filter?.Types is { Count: > 0 } types)
        {
            var values = types.Select(type => (AccountType)type).ToList();
            query = query.Where(account => values.Contains(account.Type));
        }
        return query;
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<Fund> ApplyFilter(IQueryable<Fund> query, FundFilterModel? filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.NameSearch))
        {
            query = query.Where(fund => fund.Name.Contains(filter.NameSearch));
        }

        if (filter?.Names is { Count: > 0 } names)
        {
            query = query.Where(fund => names.Contains(fund.Name));
        }

        return query;
    }

    /// <summary>
    /// Sorts the provided account rows based on the specified sort model.
    /// </summary>
    private static IEnumerable<AccountWithBalanceRangeModel> Sort(IEnumerable<AccountWithBalanceRangeModel> rows, AccountWithBalanceRangeSortModel? sort) => sort switch
    {
        AccountWithBalanceRangeSortModel.NameDescending => rows.OrderByDescending(row => row.Name),
        AccountWithBalanceRangeSortModel.Type => rows.OrderBy(row => row.Type).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.TypeDescending => rows.OrderByDescending(row => row.Type).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.StartingBalance => rows.OrderBy(row => row.StartingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.StartingBalanceDescending => rows.OrderByDescending(row => row.StartingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.EndingBalance => rows.OrderBy(row => row.EndingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.EndingBalanceDescending => rows.OrderByDescending(row => row.EndingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.NetChange => rows.OrderBy(row => row.EndingBalance - row.StartingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.NetChangeDescending => rows.OrderByDescending(row => row.EndingBalance - row.StartingBalance).ThenBy(row => row.Name),
        AccountWithBalanceRangeSortModel.Name => rows.OrderBy(row => row.Name),
        _ => rows.OrderBy(row => row.Name),
    };

    /// <summary>
    /// Sorts the provided fund rows based on the specified sort model.
    /// </summary>
    private static IEnumerable<FundWithBalanceRangeModel> Sort(IEnumerable<FundWithBalanceRangeModel> rows, FundWithBalanceRangeSortModel? sort) => sort switch
    {
        FundWithBalanceRangeSortModel.NameDescending => rows.OrderByDescending(row => row.Name),
        FundWithBalanceRangeSortModel.StartingBalance => rows.OrderBy(row => row.StartingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.StartingBalanceDescending => rows.OrderByDescending(row => row.StartingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.EndingBalance => rows.OrderBy(row => row.EndingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.EndingBalanceDescending => rows.OrderByDescending(row => row.EndingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.NetChange => rows.OrderBy(row => row.EndingBalance - row.StartingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.NetChangeDescending => rows.OrderByDescending(row => row.EndingBalance - row.StartingBalance).ThenBy(row => row.Name),
        FundWithBalanceRangeSortModel.Name => rows.OrderBy(row => row.Name),
        _ => rows.OrderBy(row => row.Name),
    };

    /// <summary>
    /// Gets each date in the range from start to end
    /// </summary>
    private static IEnumerable<DateOnly> GetDates(DateOnly start, DateOnly end)
    {
        for (DateOnly date = start; date <= end;)
        {
            yield return date;
            if (date == end)
            {
                yield break;
            }
            date = date.AddDays(1);
        }
    }
}