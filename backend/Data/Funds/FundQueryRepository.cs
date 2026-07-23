using Domain;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Funds;
using Domain.Funds.Queries;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Funds;

/// <summary>
/// Entity Framework implementation of Fund read operations.
/// </summary>
public sealed class FundQueryRepository(DatabaseContext databaseContext) : IFundQueryRepository
{
    /// <inheritdoc/>
    public async Task<QueryPage<Fund>> GetAsync(FundQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Fund> funds = ApplyFilter(databaseContext.Funds.AsNoTracking(), query.Filter);
        funds = query.Sort switch
        {
            FundSort.Name => funds.OrderBy(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSort.NameDescending => funds.OrderByDescending(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSort.Description => funds.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name).ThenBy(fund => fund.Id),
            FundSort.DescriptionDescending => funds.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name).ThenBy(fund => fund.Id),
            _ => funds.OrderBy(fund => fund.Name).ThenBy(fund => fund.Id),
        };
        int totalCount = await funds.CountAsync(cancellationToken);
        IReadOnlyCollection<Fund> items = await funds.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<Fund>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<QueryPage<FundBalance>> GetBalancesAsync(
        FundBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Fund> funds = ApplyFilter(databaseContext.Funds.AsNoTracking(), query.Filter);
        IQueryable<FundBalanceRow> balances = funds.Select(fund => new FundBalanceRow
        {
            Fund = fund,
            CurrentBalance = databaseContext.FundBalanceHistories.Where(history => history.Fund.Id == fund.Id)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                .Select(history => new PersistedFundBalance
                {
                    PostedBalance = history.PostedBalance,
                    PendingDebitAmount = history.PendingDebitAmount,
                    PendingCreditAmount = history.PendingCreditAmount,
                }).FirstOrDefault() ?? new PersistedFundBalance
                {
                    PostedBalance = fund.OnboardedBalance ?? 0,
                    PendingDebitAmount = 0,
                    PendingCreditAmount = 0,
                },
        });
        balances = query.Sort switch
        {
            FundBalanceSort.Name => balances.OrderBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            FundBalanceSort.NameDescending => balances.OrderByDescending(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            FundBalanceSort.Description => balances.OrderBy(item => item.Fund.Description).ThenBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            FundBalanceSort.DescriptionDescending => balances.OrderByDescending(item => item.Fund.Description).ThenBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            FundBalanceSort.PostedBalance => balances.OrderBy(item => item.CurrentBalance.PostedBalance).ThenBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            FundBalanceSort.PostedBalanceDescending => balances.OrderByDescending(item => item.CurrentBalance.PostedBalance).ThenBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
            _ => balances.OrderBy(item => item.Fund.Name).ThenBy(item => item.Fund.Id),
        };
        int totalCount = await balances.CountAsync(cancellationToken);
        List<FundBalanceRow> rows = await balances.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        IReadOnlyCollection<FundBalance> items = rows.Select(row => new FundBalance(
            row.Fund,
            row.CurrentBalance.PostedBalance,
            row.CurrentBalance.PendingDebitAmount,
            row.CurrentBalance.PendingCreditAmount)).ToList();
        return new QueryPage<FundBalance>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Fund>> GetRangeFundsAsync(
        FundFilter filter,
        CancellationToken cancellationToken = default) =>
        await ApplyFilter(databaseContext.Funds.AsNoTracking(), filter).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<string>> GetAllNamesAsync(CancellationToken cancellationToken = default) =>
        await databaseContext.Funds.AsNoTracking().OrderBy(fund => fund.Name)
            .Select(fund => fund.Name).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default)
    {
        List<AccountingPeriodBalanceHistory> histories = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .Include(history => history.AccountingPeriod)
            .Include(history => history.FundBalances).ThenInclude(balance => balance.Fund)
            .Where(history => ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) >= startIndex
                && ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) <= endIndex)
            .ToListAsync(cancellationToken);
        return histories.Select(history => new FundPeriodBalanceFacts(
            history.AccountingPeriod,
            history.FundBalances.Select(balance => new FundPeriodBalanceFact(
                balance.Fund,
                balance.OpeningBalance,
                balance.ClosingBalance)).ToList())).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundRangeBalance>> GetDateRangeBalancesAsync(
        FundFilter filter,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        List<FundDateRangeBalanceRow> rows = await ApplyFilter(databaseContext.Funds.AsNoTracking(), filter)
            .Select(fund => new FundDateRangeBalanceRow
            {
                Fund = fund,
                StartingBalance = databaseContext.FundBalanceHistories
                    .Where(history => history.Fund.Id == fund.Id && history.Date < startDate)
                    .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                    .Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0,
                EndingBalance = databaseContext.FundBalanceHistories
                    .Where(history => history.Fund.Id == fund.Id && history.Date <= endDate)
                    .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                    .Select(history => (decimal?)history.PostedBalance).FirstOrDefault() ?? fund.OnboardedBalance ?? 0,
            }).ToListAsync(cancellationToken);
        return rows.Select(row => new FundRangeBalance(row.Fund, row.StartingBalance, row.EndingBalance)).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FundDateBalanceFact>> GetDateBalanceFactsAsync(
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => history.Date <= endDate)
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .Select(history => new FundDateBalanceFact(history.Fund.Id, history.Date, history.Sequence, history.PostedBalance))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetDateRangeIncomeFactsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new FinancialRangeIncomeFact(
                destination.Amount,
                destination.Account.Type,
                transaction.Source.Account != null,
                destination.PostedDate))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetDateRangeSpendingFactsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Select(transaction => new FinancialRangeSpendingFact(transaction.Amount, transaction.Source.PostedDate))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Applies the given filter to the given query.
    /// </summary>
    private static IQueryable<Fund> ApplyFilter(IQueryable<Fund> query, FundFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.NameSearch))
        {
            query = query.Where(fund => fund.Name.Contains(filter.NameSearch));
        }
        if (filter.Names.Count > 0)
        {
            query = query.Where(fund => filter.Names.Contains(fund.Name));
        }
        return query;
    }

    /// <summary>
    /// Represents a row of Fund and its current balance facts.
    /// </summary>
    private sealed class FundBalanceRow
    {
        public required Fund Fund { get; init; }
        public required PersistedFundBalance CurrentBalance { get; init; }
    }

    /// <summary>
    /// Represents a row of Fund and its starting and ending balances over a date range.
    /// </summary>
    private sealed class FundDateRangeBalanceRow
    {
        public required Fund Fund { get; init; }
        public decimal StartingBalance { get; init; }
        public decimal EndingBalance { get; init; }
    }

    /// <summary>
    /// Represents the persisted balance facts of a Fund.
    /// </summary>
    private sealed class PersistedFundBalance
    {
        public decimal PostedBalance { get; init; }
        public decimal PendingDebitAmount { get; init; }
        public decimal PendingCreditAmount { get; init; }
    }
}