using Domain.AccountGoals;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.Funds.Queries;

namespace Domain.Overview;

/// <summary>
/// Service for querying the balances, current Accounting Period, and items needing attention shown on the overview.
/// </summary>
public sealed class OverviewQueryService(
    IOverviewQueryRepository overviewQueryRepository,
    AccountingPeriodQueryService accountingPeriodQueryService,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    IFundGoalRepository fundGoalRepository,
    FundGoalService fundGoalService,
    IAccountGoalRepository accountGoalRepository,
    AccountGoalService accountGoalService)
{
    /// <summary>
    /// Number of items named in each group needing attention.
    /// </summary>
    private const int TopItemCount = 3;

    /// <summary>
    /// Retrieves the overview as of the provided date, with balance history over the provided range.
    /// </summary>
    public async Task<OverviewSummary> GetAsync(
        DateOnly asOf,
        OverviewHistoryRange historyRange,
        CancellationToken cancellationToken = default)
    {
        DateOnly start = historyRange switch
        {
            OverviewHistoryRange.ThreeMonths => asOf.AddMonths(-3),
            OverviewHistoryRange.OneYear => asOf.AddYears(-1),
            OverviewHistoryRange.FiveYears => asOf.AddYears(-5),
            _ => throw new ArgumentOutOfRangeException(nameof(historyRange)),
        };
        OverviewDateRange history = await GetDateRangeAsync(start, asOf, cancellationToken);
        OverviewPeriod? currentPeriod = await GetCurrentPeriodAsync(cancellationToken);
        var attention = new List<OverviewAttentionGroup?>();
        if (currentPeriod != null)
        {
            AccountingPeriod period = currentPeriod.AccountingPeriod;
            IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(period.Id);
            IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundGoalProgress = fundGoalService.GetProgresses(fundGoals, period);
            IReadOnlyCollection<AccountGoal> accountGoals = accountGoalRepository.GetAllByAccountingPeriod(period.Id);
            IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> accountGoalProgress = accountGoalService.GetProgresses(accountGoals, period);
            List<OverviewGoalRange> goalRanges = GetGoalRanges(fundGoals, fundGoalProgress, accountGoals, accountGoalProgress);
            attention.Add(CreateGroup(OverviewAttentionKind.GoalsBelowMinimum, goalRanges
                .Where(goal => goal.AmountBelowMinimum > 0)
                .Select(goal => new OverviewAttentionItem(goal.Name, goal.AmountBelowMinimum, null))
                .ToList()));
            attention.Add(CreateGroup(OverviewAttentionKind.GoalsAboveMaximum, goalRanges
                .Where(goal => goal.AmountAboveMaximum > 0)
                .Select(goal => new OverviewAttentionItem(goal.Name, goal.AmountAboveMaximum, null))
                .ToList()));
            attention.Add(GetGoalsBehind(fundGoals, fundGoalProgress));
        }
        attention.Add(GetUnpostedTransactions(await overviewQueryRepository.GetUnpostedEffectsAsync(cancellationToken)));
        if (currentPeriod != null)
        {
            attention.Add(GetIncomeNotReceived(currentPeriod, asOf));
        }
        return new OverviewSummary(history, currentPeriod, attention.OfType<OverviewAttentionGroup>().ToList());
    }

    /// <summary>
    /// Retrieves Account and Fund balance summaries over a date range.
    /// </summary>
    private async Task<OverviewDateRange> GetDateRangeAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OverviewAccountOpeningBalance> accounts =
            await overviewQueryRepository.GetAccountOpeningBalancesAsync(start, cancellationToken);
        IReadOnlyCollection<AccountDateBalanceFact> accountHistory =
            await overviewQueryRepository.GetAccountBalanceHistoryAsync(start, end, cancellationToken);
        IReadOnlyCollection<OverviewFundOpeningBalance> funds =
            await overviewQueryRepository.GetFundOpeningBalancesAsync(start, cancellationToken);
        IReadOnlyCollection<FundDateBalanceFact> fundHistory =
            await overviewQueryRepository.GetFundBalanceHistoryAsync(start, end, cancellationToken);

        BalanceGranularity granularity = BalanceSeries.GetGranularity(start, end);
        List<DateOnly> sampleDates = BalanceSeries.GetSampleDates(start, end, granularity);
        List<(DateOnly Date, AccountBalanceSummary Summary)> accountSummaries = BalanceSeries.Summarize(
            sampleDates,
            accounts.ToDictionary(account => account.AccountId, account => account.Balance),
            accountHistory.Select(item => (item.AccountId, item.Date, item.Sequence, item.PostedBalance)),
            (date, currentBalances) => (date, AccountBalanceSummary.Create(accounts.Select(account => (
                account.Type,
                account.DateOpened is DateOnly opened && date < opened ? 0 : currentBalances[account.AccountId])))));
        List<(DateOnly Date, FundBalanceSummary Summary)> fundSummaries = BalanceSeries.Summarize(
            sampleDates,
            funds.ToDictionary(fund => fund.FundId, fund => fund.Balance),
            fundHistory.Select(item => (item.FundId, item.Date, item.Sequence, item.PostedBalance)),
            (date, currentBalances) => (date, FundBalanceSummary.Create(
                funds.Select(fund => (fund.FundId, currentBalances[fund.FundId])))));
        return new OverviewDateRange(
            accountSummaries[0].Summary,
            accountSummaries[^1].Summary,
            accountSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Summary.TotalBalance)).ToList(),
            fundSummaries[0].Summary,
            fundSummaries[^1].Summary,
            fundSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Summary.TotalBalance)).ToList(),
            granularity);
    }

    /// <summary>
    /// Retrieves the figures for the most recent open Accounting Period, or null when none is open.
    /// </summary>
    private async Task<OverviewPeriod?> GetCurrentPeriodAsync(CancellationToken cancellationToken)
    {
        AccountingPeriodId? periodId = await overviewQueryRepository.GetLatestOpenAccountingPeriodIdAsync(cancellationToken);
        AccountingPeriodBalance? balance = periodId == null
            ? null
            : await accountingPeriodQueryService.GetBalanceByIdAsync(periodId.Value, cancellationToken);
        if (balance == null)
        {
            return null;
        }
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts =
            await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync([balance.AccountingPeriod.Id.Value], cancellationToken);
        return new OverviewPeriod(
            balance.AccountingPeriod,
            balance.AccountingPeriod.ExpectedIncomeSources.Sum(source => source.ExpectedTrackedAmount),
            balance.ActualTrackedIncome,
            balance.ExpectedGoalContributions,
            balance.ActualGoalContributions,
            FinancialRangeTotals.Calculate([], spendingFacts).TotalSpending,
            balance.ClosingBalance - balance.OpeningBalance);
    }

    /// <summary>
    /// Gets how far each Fund and Account Goal's balance is below its minimum and above its maximum.
    /// </summary>
    private static List<OverviewGoalRange> GetGoalRanges(
        IReadOnlyCollection<FundGoal> fundGoals,
        IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundGoalProgress,
        IReadOnlyCollection<AccountGoal> accountGoals,
        IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> accountGoalProgress) =>
        fundGoals
            .Where(goal => fundGoalProgress.ContainsKey(goal.Id))
            .Select(goal => new OverviewGoalRange(
                goal.Fund.Name,
                fundGoalProgress[goal.Id].EndingBalance.AmountBelowMinimum,
                fundGoalProgress[goal.Id].EndingBalance.AmountAboveMaximum))
            .Concat(accountGoals
                .Where(goal => accountGoalProgress.ContainsKey(goal.Id))
                .Select(goal => new OverviewGoalRange(
                    goal.Account.Name,
                    accountGoalProgress[goal.Id].EndingBalance.AmountBelowMinimum,
                    accountGoalProgress[goal.Id].EndingBalance.AmountAboveMaximum)))
            .ToList();

    /// <summary>
    /// Flags Fund Goals short of their expected contribution.
    /// </summary>
    private static OverviewAttentionGroup? GetGoalsBehind(
        IReadOnlyCollection<FundGoal> fundGoals,
        IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundGoalProgress) =>
        CreateGroup(OverviewAttentionKind.GoalsBehind, fundGoals
            .Where(goal => fundGoalProgress.ContainsKey(goal.Id))
            .Select(goal => (goal.Fund.Name, fundGoalProgress[goal.Id].Contribution))
            .Where(item => item.Contribution is { IsSatisfied: false })
            .Select(item => new OverviewAttentionItem(item.Name, item.Contribution!.RemainingAmount, null))
            .ToList());

    /// <summary>
    /// Flags every Transaction not yet posted to all of its Accounts, totalled by Account as its effect on net worth.
    /// </summary>
    private static OverviewAttentionGroup? GetUnpostedTransactions(IReadOnlyCollection<OverviewUnpostedEffect> effects)
    {
        if (effects.Count == 0)
        {
            return null;
        }
        var accounts = effects
            .GroupBy(effect => (effect.AccountId, effect.AccountName))
            .Select(group => new OverviewAttentionItem(
                group.Key.AccountName,
                group.Sum(effect => effect.PendingCreditAmount - effect.PendingDebitAmount),
                group.Select(effect => effect.TransactionId).Distinct().Count()))
            .ToList();
        return new OverviewAttentionGroup(
            OverviewAttentionKind.UnpostedTransactions,
            effects.Select(effect => effect.TransactionId).Distinct().Count(),
            accounts.Sum(account => account.Amount),
            accounts.Count,
            GetTopItems(accounts),
            effects.Min(effect => effect.Date));
    }

    /// <summary>
    /// Flags expected tracked income whose dates have passed when less than that much tracked income has been
    /// received. Untracked income often arrives weeks after its expected date, so it is left out. Received income is
    /// not linked to the source it came from, so only the totals are compared and no sources are named. The group's
    /// date is when the expected payments, taken in date order, first added up to more than was received.
    /// </summary>
    private static OverviewAttentionGroup? GetIncomeNotReceived(OverviewPeriod period, DateOnly asOf)
    {
        var passedPayments = period.AccountingPeriod.ExpectedIncomeSources
            .Where(source => source.TrackedAmount > 0)
            .SelectMany(source => source.ExpectedDates
                .Where(expectedDate => expectedDate.Date < asOf)
                .Select(expectedDate => (expectedDate.Date, Amount: source.TrackedAmount)))
            .OrderBy(payment => payment.Date)
            .ToList();
        decimal expected = passedPayments.Sum(payment => payment.Amount);
        decimal shortfall = expected - period.ActualTrackedIncome;
        if (shortfall <= 0)
        {
            return null;
        }
        DateOnly behindSince = default;
        decimal runningExpected = 0;
        foreach ((DateOnly date, decimal amount) in passedPayments)
        {
            runningExpected += amount;
            if (runningExpected > period.ActualTrackedIncome)
            {
                behindSince = date;
                break;
            }
        }
        return new OverviewAttentionGroup(
            OverviewAttentionKind.IncomeNotReceived,
            passedPayments.Count,
            shortfall,
            0,
            [],
            behindSince,
            new OverviewIncomeShortfall(expected, period.ActualTrackedIncome));
    }

    /// <summary>
    /// Creates a group from its items, or null when there are none.
    /// </summary>
    private static OverviewAttentionGroup? CreateGroup(OverviewAttentionKind kind, List<OverviewAttentionItem> items) =>
        items.Count == 0
            ? null
            : new OverviewAttentionGroup(kind, items.Count, items.Sum(item => item.Amount), items.Count, GetTopItems(items), null);

    /// <summary>
    /// Gets the items with the largest amounts.
    /// </summary>
    private static List<OverviewAttentionItem> GetTopItems(IEnumerable<OverviewAttentionItem> items) =>
        items
            .OrderByDescending(item => Math.Abs(item.Amount))
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .Take(TopItemCount)
            .ToList();
}

/// <summary>
/// How far a Fund or Account Goal's balance is below its minimum and above its maximum.
/// </summary>
internal sealed record OverviewGoalRange(string Name, decimal AmountBelowMinimum, decimal AmountAboveMaximum);
