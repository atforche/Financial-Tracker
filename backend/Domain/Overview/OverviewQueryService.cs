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
    AccountQueryService accountQueryService,
    FundQueryService fundQueryService,
    AccountingPeriodQueryService accountingPeriodQueryService,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundGoalRepository fundGoalRepository,
    IAccountGoalRepository accountGoalRepository)
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
        AccountingPeriod? period = await overviewQueryRepository.GetLatestOpenAccountingPeriodAsync(cancellationToken);
        OverviewPeriod? currentPeriod = null;
        var attention = new List<OverviewAttentionGroup?>();
        OverviewAttentionGroup? incomeNotReceived = null;
        if (period != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(period.Id);
            IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(period.Id);
            IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundGoalProgress = FundGoalService.GetProgresses(fundGoals, balanceHistory);
            IReadOnlyCollection<AccountGoal> accountGoals = accountGoalRepository.GetAllByAccountingPeriod(period.Id);
            IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> accountGoalProgress = AccountGoalService.GetProgresses(accountGoals, balanceHistory);
            AccountingPeriodBalance balance = await accountingPeriodQueryService.EnrichBalanceAsync(period, balanceHistory, fundGoals, cancellationToken);
            currentPeriod = await GetCurrentPeriodAsync(balance, cancellationToken);
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
            incomeNotReceived = GetIncomeNotReceived(balance, asOf);
        }
        attention.Add(GetUnpostedTransactions(await overviewQueryRepository.GetUnpostedEffectsAsync(cancellationToken)));
        attention.Add(incomeNotReceived);
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
        (List<AccountDateBalanceSummary> accountSummaries, BalanceGranularity granularity) =
            await accountQueryService.GetDateSummariesAsync(new AccountFilter(null, [], []), start, end, cancellationToken);
        (List<FundDateBalanceSummary> fundSummaries, _) =
            await fundQueryService.GetDateSummariesAsync(new FundFilter(null, []), start, end, cancellationToken);
        return new OverviewDateRange(
            start,
            accountSummaries[0].Balance,
            accountSummaries[^1].Balance,
            accountSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Balance.TotalBalance)).ToList(),
            fundSummaries[^1].Balance,
            fundSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Balance.TotalBalance)).ToList(),
            granularity);
    }

    /// <summary>
    /// Gets the income, goal contribution, spending, and net worth figures for the current Accounting Period.
    /// </summary>
    private async Task<OverviewPeriod> GetCurrentPeriodAsync(AccountingPeriodBalance balance, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts =
            await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync([balance.AccountingPeriod.Id.Value], cancellationToken);
        return new OverviewPeriod(
            balance.AccountingPeriod,
            balance.AccountingPeriod.ExpectedIncomeSources.Sum(source => source.ExpectedTrackedAmount),
            balance.ActualPlannedTrackedIncome,
            balance.ActualUnplannedTrackedIncome,
            balance.ExpectedGoalContributions,
            balance.ActualGoalContributions,
            balance.ActualUnplannedGoalContributions,
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
            .Select(goal => fundGoalProgress.GetValueOrDefault(goal.Id)?.EndingBalance is { } balance
                ? new OverviewGoalRange(goal.Fund.Name, balance.AmountBelowMinimum, balance.AmountAboveMaximum)
                : null)
            .Concat(accountGoals
                .Select(goal => accountGoalProgress.GetValueOrDefault(goal.Id)?.EndingBalance is { } balance
                    ? new OverviewGoalRange(goal.Account.Name, balance.AmountBelowMinimum, balance.AmountAboveMaximum)
                    : null))
            .OfType<OverviewGoalRange>()
            .ToList();

    /// <summary>
    /// Flags Fund Goals short of their expected contribution.
    /// </summary>
    private static OverviewAttentionGroup? GetGoalsBehind(
        IReadOnlyCollection<FundGoal> fundGoals,
        IReadOnlyDictionary<FundGoalId, FundGoalProgress> fundGoalProgress) =>
        CreateGroup(OverviewAttentionKind.GoalsBehind, fundGoals
            .Select(goal => (goal.Fund.Name, fundGoalProgress.GetValueOrDefault(goal.Id)?.Contribution))
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
    private static OverviewAttentionGroup? GetIncomeNotReceived(AccountingPeriodBalance period, DateOnly asOf)
    {
        var passedPayments = period.AccountingPeriod.ExpectedIncomeSources
            .Where(source => source.TrackedAmount > 0)
            .SelectMany(source => source.ExpectedDates
                .Where(expectedDate => expectedDate.Date < asOf)
                .Select(expectedDate => (expectedDate.Date, Amount: source.TrackedAmount)))
            .OrderBy(payment => payment.Date)
            .ToList();
        decimal expected = passedPayments.Sum(payment => payment.Amount);
        decimal shortfall = expected - period.ActualPlannedTrackedIncome;
        if (shortfall <= 0)
        {
            return null;
        }
        DateOnly behindSince = default;
        decimal runningExpected = 0;
        foreach ((DateOnly date, decimal amount) in passedPayments)
        {
            runningExpected += amount;
            if (runningExpected > period.ActualPlannedTrackedIncome)
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
            new OverviewIncomeShortfall(expected, period.ActualPlannedTrackedIncome));
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
