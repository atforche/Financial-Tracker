using Domain;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.AccountingPeriods;

/// <summary>
/// Entity Framework implementation of Accounting Period read operations.
/// </summary>
public sealed class AccountingPeriodQueryRepository(DatabaseContext databaseContext) : IAccountingPeriodQueryRepository
{
    /// <inheritdoc/>
    public async Task<QueryPage<AccountingPeriod>> GetAsync(
        AccountingPeriodQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccountingPeriod> periods = ApplyFilter(databaseContext.AccountingPeriods.AsNoTracking(), query.Filter);
        periods = query.Sort switch
        {
            AccountingPeriodSort.Date => periods.OrderBy(period => period.Year).ThenBy(period => period.Month).ThenBy(period => period.Id),
            AccountingPeriodSort.DateDescending => periods.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
            AccountingPeriodSort.IsOpen => periods.OrderBy(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
            AccountingPeriodSort.IsOpenDescending => periods.OrderByDescending(period => period.IsOpen).ThenByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
            _ => periods.OrderByDescending(period => period.Year).ThenByDescending(period => period.Month).ThenBy(period => period.Id),
        };
        int totalCount = await periods.CountAsync(cancellationToken);
        IReadOnlyCollection<AccountingPeriod> items = await periods.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<AccountingPeriod>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<QueryPage<AccountingPeriodBalance>> GetBalancesAsync(
        AccountingPeriodBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccountingPeriodBalanceRow> balances = databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .AsSplitQuery()
            .Include(history => history.AccountingPeriod)
            .ThenInclude(accountingPeriod => accountingPeriod.ExpectedIncomeSources)
            .Select(history => new AccountingPeriodBalanceRow
            {
                AccountingPeriod = history.AccountingPeriod,
                OpeningBalance = history.OpeningBalance,
                ClosingBalance = history.ClosingBalance,
            });
        balances = ApplyFilter(balances, query.Filter);
        balances = query.Sort switch
        {
            AccountingPeriodBalanceSort.Date => balances.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.DateDescending => balances.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.IsOpen => balances.OrderBy(item => item.AccountingPeriod.IsOpen).ThenByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.IsOpenDescending => balances.OrderByDescending(item => item.AccountingPeriod.IsOpen).ThenByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.OpeningBalance => balances.OrderBy(item => item.OpeningBalance).ThenBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.OpeningBalanceDescending => balances.OrderByDescending(item => item.OpeningBalance).ThenByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.ClosingBalance => balances.OrderBy(item => item.ClosingBalance).ThenBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            AccountingPeriodBalanceSort.ClosingBalanceDescending => balances.OrderByDescending(item => item.ClosingBalance).ThenByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
            _ => balances.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.AccountingPeriod.Id),
        };
        int totalCount = await balances.CountAsync(cancellationToken);
        List<AccountingPeriodBalanceRow> rows = await balances.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountingPeriodBalance> items = rows.Select(row => new AccountingPeriodBalance(
            row.AccountingPeriod,
            row.OpeningBalance,
            row.ClosingBalance,
            0,
            0,
            0,
            0)).ToList();
        return new QueryPage<AccountingPeriodBalance>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<AccountingPeriodBalance?> GetBalanceByIdAsync(
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodBalanceRow? row = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking().AsSplitQuery()
            .Include(history => history.AccountingPeriod)
            .ThenInclude(accountingPeriod => accountingPeriod.ExpectedIncomeSources)
            .Where(history => history.AccountingPeriod.Id == accountingPeriodId)
            .Select(history => new AccountingPeriodBalanceRow
            {
                AccountingPeriod = history.AccountingPeriod,
                OpeningBalance = history.OpeningBalance,
                ClosingBalance = history.ClosingBalance,
            }).SingleOrDefaultAsync(cancellationToken);
        return row == null ? null : new AccountingPeriodBalance(row.AccountingPeriod, row.OpeningBalance, row.ClosingBalance, 0, 0, 0, 0);
    }

    /// <inheritdoc/>
    public Task<AccountingPeriod?> GetByIdAsync(
        AccountingPeriodId accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        databaseContext.AccountingPeriods.AsNoTracking()
            .SingleOrDefaultAsync(period => period.Id == accountingPeriodId, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountingPeriod>> GetRangeAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => (period.Year * 12) + period.Month >= startIndex && (period.Year * 12) + period.Month <= endIndex)
            .OrderBy(period => period.Year).ThenBy(period => period.Month)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountingPeriod>> GetByIdsAsync(
        IReadOnlyCollection<AccountingPeriodId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => ids.Contains(period.Id))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountingPeriodBalance>> GetRangeBalancesAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default)
    {
        List<AccountingPeriodBalanceRow> rows = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .AsSplitQuery()
            .Include(history => history.AccountingPeriod)
            .ThenInclude(accountingPeriod => accountingPeriod.ExpectedIncomeSources)
            .Where(history => ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) >= startIndex
                && ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) <= endIndex)
            .OrderBy(history => history.AccountingPeriod.Year)
            .ThenBy(history => history.AccountingPeriod.Month)
            .Select(history => new AccountingPeriodBalanceRow
            {
                AccountingPeriod = history.AccountingPeriod,
                OpeningBalance = history.OpeningBalance,
                ClosingBalance = history.ClosingBalance,
            }).ToListAsync(cancellationToken);
        return rows.Select(row => new AccountingPeriodBalance(
            row.AccountingPeriod,
            row.OpeningBalance,
            row.ClosingBalance,
            0,
            0,
            0,
            0)).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetRangeIncomeFactsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default)
    {
        var ids = accountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
        return await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId))
            .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new FinancialRangeIncomeFact(
                destination.Amount,
                destination.Account.Type,
                destination.PostedDate))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetRangeSpendingFactsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default)
    {
        var ids = accountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
        return await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => ids.Contains(transaction.AccountingPeriodId))
            .Select(transaction => new FinancialRangeSpendingFact(transaction.Amount, transaction.Source.PostedDate))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Applies the provided filter to the given queryable of Accounting Periods.
    /// </summary>
    private static IQueryable<AccountingPeriod> ApplyFilter(
        IQueryable<AccountingPeriod> periods,
        AccountingPeriodFilter filter)
    {
        if (filter.Years.Count > 0)
        {
            periods = periods.Where(period => filter.Years.Contains(period.Year));
        }
        if (filter.Months.Count > 0)
        {
            periods = periods.Where(period => filter.Months.Contains(period.Month));
        }
        return periods;
    }

    /// <summary>
    /// Applies the provided filter to the given queryable of Accounting Period Balances.
    /// </summary>
    private static IQueryable<AccountingPeriodBalanceRow> ApplyFilter(
        IQueryable<AccountingPeriodBalanceRow> balances,
        AccountingPeriodFilter filter)
    {
        if (filter.Years.Count > 0)
        {
            balances = balances.Where(item => filter.Years.Contains(item.AccountingPeriod.Year));
        }
        if (filter.Months.Count > 0)
        {
            balances = balances.Where(item => filter.Months.Contains(item.AccountingPeriod.Month));
        }
        return balances;
    }

    /// <summary>
    /// Represents a row of Accounting Period Balance data retrieved from the database.
    /// </summary>
    private sealed class AccountingPeriodBalanceRow
    {
        public required AccountingPeriod AccountingPeriod { get; init; }
        public decimal OpeningBalance { get; init; }
        public decimal ClosingBalance { get; init; }
    }
}
