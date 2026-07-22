using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;

namespace Domain.Funds.Queries;

/// <summary>
/// Service for querying Funds and their Balances.
/// </summary>
public sealed class FundQueryService(
    IFundRepository fundRepository,
    IFundQueryRepository fundQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository)
{
    /// <summary>
    /// Retrieves the Fund with the specified ID, or null when it does not exist.
    /// </summary>
    public Fund? GetById(Guid fundId)
    {
        if (fundRepository.TryGetById(fundId, out Fund? fund))
        {
            return fund;
        }
        return null;
    }

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
        IReadOnlyCollection<AccountingPeriod> endpoints = await accountingPeriodQueryRepository.GetEndpointsAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == query.StartId);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == query.EndId);
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
            return new FundAccountingPeriodRangeQueryResult(null, failure);
        }

        int startIndex = GetChronologicalIndex(start!);
        int endIndex = GetChronologicalIndex(end!);
        if (startIndex > endIndex)
        {
            return new FundAccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.Reversed);
        }

        IReadOnlyCollection<FundPeriodBalanceFacts> histories = await fundQueryRepository.GetPeriodBalanceFactsAsync(
            startIndex,
            endIndex,
            cancellationToken);
        IReadOnlyCollection<FundPeriodBalanceFacts> orderedHistories = histories
            .OrderBy(history => history.AccountingPeriod.Year)
            .ThenBy(history => history.AccountingPeriod.Month)
            .ToList();
        IReadOnlyCollection<int> indexes = orderedHistories.Select(history => GetChronologicalIndex(history.AccountingPeriod)).ToList();
        if (!indexes.SequenceEqual(Enumerable.Range(startIndex, endIndex - startIndex + 1)))
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
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> recognizedIncome = incomeFacts
            .Where(fact => !fact.HasInternalSource || fact.PostedDate != null)
            .ToList();
        decimal totalIncome = recognizedIncome.Sum(fact => fact.Amount);
        decimal trackedIncome = recognizedIncome.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount);
        decimal totalSpending = spendingFacts.Where(fact => fact.PostedDate != null).Sum(fact => fact.Amount);
        IReadOnlyCollection<FundPeriodBalanceSummary> summaries = orderedHistories.Select(history => new FundPeriodBalanceSummary(
            history.AccountingPeriod,
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Fund.Id)), true),
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Fund.Id)), false))).ToList();
        var range = new FundAccountingPeriodRange(
            new QueryPage<FundRangeBalance>(items, balances.Count),
            await fundQueryRepository.GetAllNamesAsync(cancellationToken),
            totalIncome,
            trackedIncome,
            totalSpending,
            summaries);
        return new FundAccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Gets the chronological index of an Accounting Period, used for sorting and range calculations.
    /// </summary>
    private static int GetChronologicalIndex(AccountingPeriod period) => (period.Year * 12) + period.Month;

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