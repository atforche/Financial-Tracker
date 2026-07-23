using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying Funds and their Balances.
/// </summary>
public sealed class FundQueryService(
    IFundQueryRepository fundQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves the Fund with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<Fund?> GetByIdAsync(Guid fundId, CancellationToken cancellationToken = default) =>
        fundQueryRepository.GetByIdAsync(new FundId(fundId), cancellationToken);

    /// <summary>
    /// Retrieves the Funds matching the provided query.
    /// </summary>
    public Task<QueryPage<Fund>> GetAsync(FundQuery query, CancellationToken cancellationToken = default) =>
        fundQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Funds and their current balances.
    /// </summary>
    public Task<QueryPage<FundBalance>> GetWithBalancesAsync(
        FundBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        fundQueryRepository.GetBalancesAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Fund balances and financial totals over an Accounting Period range.
    /// </summary>
    public async Task<FundAccountingPeriodRangeQueryResult> GetAccountingPeriodRangeAsync(
        FundAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new FundAccountingPeriodRangeQueryResult(null, resolution.Failure);
        }

        AccountingPeriod start = resolution.AccountingPeriods.First();
        AccountingPeriod end = resolution.AccountingPeriods.Last();
        IReadOnlyCollection<FundPeriodBalanceFacts> histories = await fundQueryRepository.GetPeriodBalanceFactsAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end),
            cancellationToken);
        IReadOnlyCollection<FundPeriodBalanceFacts> orderedHistories = histories
            .OrderBy(history => history.AccountingPeriod.Year)
            .ThenBy(history => history.AccountingPeriod.Month)
            .ToList();
        if (!AccountingPeriodRangeResolver.IsContiguous(orderedHistories.Select(history => history.AccountingPeriod), start, end))
        {
            return new FundAccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }

        IReadOnlyCollection<Fund> funds = await fundQueryRepository.GetRangeFundsAsync(query.Filter, cancellationToken);
        var matchingIds = funds.Select(fund => fund.Id).ToHashSet();
        FundPeriodBalanceFacts first = orderedHistories.First();
        FundPeriodBalanceFacts last = orderedHistories.Last();
        var balances = funds.Select(fund => new FundRangeBalance(
            fund,
            first.Balances.SingleOrDefault(balance => balance.Fund.Id == fund.Id)?.OpeningBalance ?? fund.OnboardedBalance ?? 0,
            last.Balances.SingleOrDefault(balance => balance.Fund.Id == fund.Id)?.ClosingBalance ?? fund.OnboardedBalance ?? 0)).ToList();
        IReadOnlyCollection<FundRangeBalance> items = Sort(balances, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        IReadOnlyCollection<Guid> periodIds = orderedHistories.Select(history => history.AccountingPeriod.Id.Value).ToList();
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(periodIds, cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        IReadOnlyCollection<FundPeriodBalanceSummary> summaries = orderedHistories.Select(history => new FundPeriodBalanceSummary(
            history.AccountingPeriod,
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Fund.Id)), true),
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Fund.Id)), false))).ToList();
        var range = new FundAccountingPeriodRange(
            new QueryPage<FundRangeBalance>(items, balances.Count),
            await fundQueryRepository.GetAllNamesAsync(cancellationToken),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending,
            summaries);
        return new FundAccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Retrieves Fund balances and financial totals over a date range.
    /// </summary>
    public async Task<FundDateRange> GetDateRangeAsync(
        FundDateRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<FundRangeBalance> balances = await fundQueryRepository.GetDateRangeBalancesAsync(
            query.Filter,
            query.Start,
            query.End,
            cancellationToken);
        IReadOnlyCollection<FundDateBalanceFact> history = await fundQueryRepository.GetDateBalanceFactsAsync(query.End, cancellationToken);
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await fundQueryRepository.GetDateRangeIncomeFactsAsync(
            query.Start,
            query.End,
            cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await fundQueryRepository.GetDateRangeSpendingFactsAsync(
            query.Start,
            query.End,
            cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        IReadOnlyCollection<FundDateBalanceSummary> dates = GetDates(query.Start, query.End)
            .Select(date => new FundDateBalanceSummary(date, SummarizeDate(balances, history, date)))
            .ToList();
        IReadOnlyCollection<FundRangeBalance> items = Sort(balances, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        return new FundDateRange(
            new QueryPage<FundRangeBalance>(items, balances.Count),
            await fundQueryRepository.GetAllNamesAsync(cancellationToken),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending,
            dates);
    }

    /// <summary>
    /// Summarizes Fund balances into a FundBalanceSummary.
    /// </summary>
    private static FundBalanceSummary Summarize(IEnumerable<FundPeriodBalanceFact> balances, bool opening)
    {
        var values = balances.Select(balance => (
            balance.Fund.Id,
            Amount: opening ? balance.OpeningBalance : balance.ClosingBalance)).ToList();
        return new FundBalanceSummary(
            values.Sum(item => item.Amount),
            values.Where(item => item.Id != Fund.UnassignedFundId).Sum(item => item.Amount),
            values.Where(item => item.Id == Fund.UnassignedFundId).Sum(item => item.Amount));
    }

    /// <summary>
    /// Summarizes Fund balances into a FundBalanceSummary for a specific date, using the provided history facts.
    /// </summary>
    private static FundBalanceSummary SummarizeDate(
        IEnumerable<FundRangeBalance> balances,
        IReadOnlyCollection<FundDateBalanceFact> history,
        DateOnly date)
    {
        var values = balances.Select(balance => (
            balance.Fund.Id,
            Amount: history.LastOrDefault(item => item.FundId == balance.Fund.Id && item.Date <= date)?.PostedBalance
                ?? balance.Fund.OnboardedBalance ?? 0)).ToList();
        return new FundBalanceSummary(
            values.Sum(item => item.Amount),
            values.Where(item => item.Id != Fund.UnassignedFundId).Sum(item => item.Amount),
            values.Where(item => item.Id == Fund.UnassignedFundId).Sum(item => item.Amount));
    }

    /// <summary>
    /// Gets the dates between the provided start and end dates, inclusive.
    /// </summary>
    private static IEnumerable<DateOnly> GetDates(DateOnly start, DateOnly end)
    {
        for (DateOnly date = start; date <= end; date = date.AddDays(1))
        {
            yield return date;
            if (date == end)
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Sorts Fund balances according to the specified sort order.
    /// </summary>
    private static IOrderedEnumerable<FundRangeBalance> Sort(
        IEnumerable<FundRangeBalance> balances,
        FundRangeSort sort) => sort switch
        {
            FundRangeSort.Name => balances.OrderBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.NameDescending => balances.OrderByDescending(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.StartingBalance => balances.OrderBy(balance => balance.StartingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.StartingBalanceDescending => balances.OrderByDescending(balance => balance.StartingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.EndingBalance => balances.OrderBy(balance => balance.EndingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.EndingBalanceDescending => balances.OrderByDescending(balance => balance.EndingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.NetChange => balances.OrderBy(balance => balance.EndingBalance - balance.StartingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            FundRangeSort.NetChangeDescending => balances.OrderByDescending(balance => balance.EndingBalance - balance.StartingBalance).ThenBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
            _ => balances.OrderBy(balance => balance.Fund.Name).ThenBy(balance => balance.Fund.Id),
        };
}