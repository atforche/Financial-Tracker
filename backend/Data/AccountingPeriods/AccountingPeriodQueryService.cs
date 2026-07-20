using Data.Transactions;
using Domain.AccountingPeriods;
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
    /// Retrieves Accounting Periods matching the provided query.
    /// </summary>
    public async Task<CollectionModel<AccountingPeriodModel>> GetAsync(
        AccountingPeriodQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccountingPeriod> query = databaseContext.AccountingPeriods.AsNoTracking();
        if (request.Filter?.Years is { Count: > 0 } years)
        {
            query = query.Where(period => years.Contains(period.Year));
        }
        if (request.Filter?.Months is { Count: > 0 } months)
        {
            query = query.Where(period => months.Contains(period.Month));
        }

        query = request.Sort switch
        {
            AccountingPeriodSortModel.Date => query.OrderBy(period => period.Year).ThenBy(period => period.Month),
            AccountingPeriodSortModel.DateDescending => query.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodSortModel.IsOpen => query.OrderBy(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodSortModel.IsOpenDescending => query.OrderByDescending(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            _ => query.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month),
        };

        int totalCount = await query.CountAsync(cancellationToken);
        List<AccountingPeriodModel> items = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue)
            .Select(period => new AccountingPeriodModel
            {
                Id = period.Id.Value,
                Name = period.Name,
                Year = period.Year,
                Month = period.Month,
                IsOpen = period.IsOpen,
            }).ToListAsync(cancellationToken);
        return new CollectionModel<AccountingPeriodModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves Accounting Periods and their balances.
    /// </summary>
    public async Task<CollectionModel<AccountingPeriodWithBalanceModel>> GetWithBalancesAsync(
        AccountingPeriodWithBalanceQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccountingPeriodWithBalanceModel> query =
            from history in databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            let period = history.AccountingPeriod
            select new AccountingPeriodWithBalanceModel
            {
                Id = period.Id.Value,
                Name = period.Name,
                Year = period.Year,
                Month = period.Month,
                IsOpen = period.IsOpen,
                OpeningBalance = history.OpeningBalance,
                ClosingBalance = history.ClosingBalance,
            };
        if (request.Filter?.Years is { Count: > 0 } years)
        {
            query = query.Where(period => years.Contains(period.Year));
        }
        if (request.Filter?.Months is { Count: > 0 } months)
        {
            query = query.Where(period => months.Contains(period.Month));
        }
        query = request.Sort switch
        {
            AccountingPeriodWithBalanceSortModel.Date => query.OrderBy(period => period.Year).ThenBy(period => period.Month),
            AccountingPeriodWithBalanceSortModel.DateDescending => query.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodWithBalanceSortModel.IsOpen => query.OrderBy(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodWithBalanceSortModel.IsOpenDescending => query.OrderByDescending(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodWithBalanceSortModel.OpeningBalance => query.OrderBy(period => period.OpeningBalance).ThenBy(period => period.Year).ThenBy(period => period.Month),
            AccountingPeriodWithBalanceSortModel.OpeningBalanceDescending => query.OrderByDescending(period => period.OpeningBalance).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            AccountingPeriodWithBalanceSortModel.ClosingBalance => query.OrderBy(period => period.ClosingBalance).ThenBy(period => period.Year).ThenBy(period => period.Month),
            AccountingPeriodWithBalanceSortModel.ClosingBalanceDescending => query.OrderByDescending(period => period.ClosingBalance).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month),
            _ => query.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month),
        };
        int totalCount = await query.CountAsync(cancellationToken);
        List<AccountingPeriodWithBalanceModel> items = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new CollectionModel<AccountingPeriodWithBalanceModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID.
    /// </summary>
    public Task<AccountingPeriodWithBalanceModel?> GetByIdAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default) =>
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
        IQueryable<IncomeTransaction> incomeQuery = databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId);
        decimal totalIncome = await incomeQuery.SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        decimal trackedIncome = await incomeQuery.SumAsync(transaction => (decimal?)transaction.TrackedAmount, cancellationToken) ?? 0;
        decimal totalSpending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId)
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
        decimal income = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId)).SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        decimal tracked = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId)).SumAsync(transaction => (decimal?)transaction.TrackedAmount, cancellationToken) ?? 0;
        decimal spending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId)).SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
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