using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.AccountingPeriods;
using Models.Transactions.Types;

namespace Data.AccountingPeriods;

/// <summary>
/// Read-only queries for Accounting Period API models.
/// </summary>
public sealed class AccountingPeriodQueryService(DatabaseContext databaseContext, TransactionQueryService transactionQueryService)
{
    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID.
    /// </summary>
    private Task<AccountingPeriodWithBalanceModel?> GetByIdAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default) =>
        (from history in databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
         where history.AccountingPeriod.Id == new AccountingPeriodId(accountingPeriodId)
         select new AccountingPeriodWithBalanceModel
         {
             Id = history.AccountingPeriod.Id.Value,
             Name = history.AccountingPeriod.Name,
             Year = history.AccountingPeriod.Year,
             Month = history.AccountingPeriod.Month,
             IsOpen = history.AccountingPeriod.IsOpen,
             OpeningBalance = history.OpeningBalance,
             ClosingBalance = history.ClosingBalance,
         }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period with its matching Transactions and totals.
    /// </summary>
    public async Task<AccountingPeriodWithTransactionsModel?> GetWithTransactionsAsync(
        Guid accountingPeriodId,
        AccountingPeriodWithTransactionsQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodWithBalanceModel? period = await GetByIdAsync(accountingPeriodId, cancellationToken);
        if (period == null)
        {
            return null;
        }
        var periodId = new AccountingPeriodId(accountingPeriodId);
        CollectionModel<TransactionModel> transactions = await transactionQueryService.GetForAccountingPeriodAsync(accountingPeriodId, request, cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId)
            .ToListAsync(cancellationToken);
        var incomeDestinations = incomeTransactions.SelectMany(transaction => transaction.Destinations
            .Where(destination => transaction.Source.Account == null || destination.PostedDate != null))
            .ToList();
        decimal totalIncome = incomeDestinations.Sum(destination => destination.Amount);
        decimal trackedIncome = incomeDestinations.Where(destination => destination.Account.Type.IsTracked()).Sum(destination => destination.Amount);
        decimal totalSpending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId)
            .Where(transaction => transaction.Source.PostedDate != null)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        return new AccountingPeriodWithTransactionsModel
        {
            Id = period.Id,
            Name = period.Name,
            Year = period.Year,
            Month = period.Month,
            IsOpen = period.IsOpen,
            OpeningBalance = period.OpeningBalance,
            ClosingBalance = period.ClosingBalance,
            Transactions = transactions,
            TotalIncome = new IncomeAmountModel { Total = totalIncome, Tracked = trackedIncome, Untracked = totalIncome - trackedIncome },
            TotalSpending = totalSpending,
        };
    }

    /// <summary>
    /// Retrieves a contiguous range of Accounting Periods and aggregate totals.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult> GetRangeAsync(
        AccountingPeriodsInRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        List<AccountingPeriod> endpoints = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => period.Id == new AccountingPeriodId(request.Range.Start) || period.Id == new AccountingPeriodId(request.Range.End))
            .ToListAsync(cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == request.Range.Start);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == request.Range.End);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeQueryFailure.None;
        if (start == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.StartNotFound;
        }
        if (end == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.EndNotFound;
        }
        if (failure != AccountingPeriodRangeQueryFailure.None)
        {
            return new AccountingPeriodRangeQueryResult(null, failure);
        }
        int startIndex = (start!.Year * 12) + start.Month;
        int endIndex = (end!.Year * 12) + end.Month;
        if (startIndex > endIndex)
        {
            return new AccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.Reversed);
        }

        List<AccountingPeriodWithBalanceModel> periods = await (from history in databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
                                                                let period = history.AccountingPeriod
                                                                let index = (period.Year * 12) + period.Month
                                                                where index >= startIndex && index <= endIndex
                                                                orderby period.Year, period.Month
                                                                select new AccountingPeriodWithBalanceModel
                                                                {
                                                                    Id = period.Id.Value,
                                                                    Name = period.Name,
                                                                    Year = period.Year,
                                                                    Month = period.Month,
                                                                    IsOpen = period.IsOpen,
                                                                    OpeningBalance = history.OpeningBalance,
                                                                    ClosingBalance = history.ClosingBalance,
                                                                }).ToListAsync(cancellationToken);
        if (periods.Count != endIndex - startIndex + 1)
        {
            return new AccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }
        var ids = periods.Select(period => new AccountingPeriodId(period.Id)).ToList();
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId))
            .ToListAsync(cancellationToken);
        var incomeDestinations = incomeTransactions.SelectMany(transaction => transaction.Destinations
            .Where(destination => transaction.Source.Account == null || destination.PostedDate != null))
            .ToList();
        decimal income = incomeDestinations.Sum(destination => destination.Amount);
        decimal tracked = incomeDestinations.Where(destination => destination.Account.Type.IsTracked()).Sum(destination => destination.Amount);
        decimal spending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId))
            .Where(transaction => transaction.Source.PostedDate != null)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        IEnumerable<AccountingPeriodWithBalanceModel> sorted = request.Sort switch
        {
            AccountingPeriodWithBalanceSortModel.Date => periods.OrderBy(period => period.Year).ThenBy(period => period.Month).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.DateDescending => periods.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.OpeningBalance => periods.OrderBy(period => period.OpeningBalance).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.OpeningBalanceDescending => periods.OrderByDescending(period => period.OpeningBalance).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.ClosingBalance => periods.OrderBy(period => period.ClosingBalance).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.ClosingBalanceDescending => periods.OrderByDescending(period => period.ClosingBalance).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.IsOpen => periods.OrderBy(period => period.IsOpen).ThenBy(period => period.Id),
            AccountingPeriodWithBalanceSortModel.IsOpenDescending => periods.OrderByDescending(period => period.IsOpen).ThenBy(period => period.Id),
            _ => periods.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
        };
        return new AccountingPeriodRangeQueryResult(new AccountingPeriodsInRangeModel
        {
            AccountingPeriods = new CollectionModel<AccountingPeriodWithBalanceModel>
            {
                Items = sorted.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
                TotalCount = periods.Count,
            },
            TotalIncome = new IncomeAmountModel { Total = income, Tracked = tracked, Untracked = income - tracked },
            TotalSpending = spending,
        }, AccountingPeriodRangeQueryFailure.None);
    }
}