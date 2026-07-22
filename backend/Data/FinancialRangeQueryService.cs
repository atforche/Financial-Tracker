using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Accounts;
using Models.Funds;

namespace Data;

/// <summary>
/// Read-only range queries shared by Account and Fund workspaces.
/// </summary>
public sealed class FinancialRangeQueryService(DatabaseContext databaseContext)
{
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
            StartingBalance = databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fund.Id && history.Date < request.Range.Start)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0,
            EndingBalance = databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fund.Id && history.Date <= request.Range.End)
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
                    Balance = histories.LastOrDefault(history => history.Fund.Id == fund.Id && history.Date <= date)?.PostedBalance
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
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => transaction.Date >= start && transaction.Date <= end)
            .ToListAsync(cancellationToken);
        var incomeDestinations = incomeTransactions.SelectMany(transaction => transaction.Destinations
            .Where(destination => transaction.Source.Account == null || destination.PostedDate != null))
            .ToList();
        decimal total = incomeDestinations.Sum(destination => destination.Amount);
        decimal tracked = incomeDestinations.Where(destination => destination.Account.Type.IsTracked()).Sum(destination => destination.Amount);
        decimal spending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => transaction.Date >= start && transaction.Date <= end)
            .Where(transaction => transaction.Source.PostedDate != null)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        return (new IncomeAmountModel { Total = total, Tracked = tracked, Untracked = total - tracked }, spending);
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