using Domain.AccountGoals;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.BalanceEvents;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Domain.Funds.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Service for querying one Account or Fund within one Accounting Period.
/// </summary>
public sealed class PeriodEntityQueryService(
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IAccountBalanceEventQueryRepository accountBalanceEventQueryRepository,
    IFundBalanceEventQueryRepository fundBalanceEventQueryRepository,
    IAccountGoalRepository accountGoalRepository,
    IFundGoalRepository fundGoalRepository,
    AccountBalanceEventQueryService accountBalanceEventQueryService,
    FundGoalBalanceEventQueryService fundGoalBalanceEventQueryService)
{
    /// <summary>
    /// Retrieves an Account's detail in an Accounting Period, or null when either is missing or they aren't related.
    /// </summary>
    public async Task<PeriodAccountDetail?> GetAccountAsync(
        Guid accountingPeriodId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodId periodId = new(accountingPeriodId);
        AccountingPeriod? period = await accountingPeriodQueryRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null)
        {
            return null;
        }
        AccountId id = new(accountId);
        AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(periodId);
        AccountingPeriodAccountBalanceHistory? balance = history.AccountBalances.SingleOrDefault(item => item.Account.Id == id);
        if (balance == null)
        {
            return null;
        }
        AccountGoal? goal = accountGoalRepository.GetByAccountAndAccountingPeriod(id, periodId);
        AccountGoalProgress? progress = goal == null
            ? null
            : AccountGoalService.GetProgresses([goal], history).GetValueOrDefault(goal.Id);
        (AccountingPeriod? previous, AccountingPeriod? next) = await accountingPeriodQueryRepository.GetAdjacentAsync(period, cancellationToken);
        IReadOnlyCollection<AccountBalanceHistory> dailyHistories = await accountBalanceEventQueryRepository.GetPeriodHistoriesAsync(id, periodId, cancellationToken);
        QueryPage<AccountBalanceEvent> events = await accountBalanceEventQueryService.GetAsync(
            new AccountBalanceEventAccountQuery(accountId, null, null, [accountingPeriodId], true, AccountBalanceEventSort.DateDescending, 0, null),
            cancellationToken);
        var totals = BalanceEventTotals.Calculate(events.Items, item => item.IsPosted, item => item.Type, item => item.Amount);
        return new PeriodAccountDetail(
            period,
            previous,
            next,
            new AccountingPeriodAccount(balance.Account, balance.OpeningBalance, balance.ClosingBalance, progress),
            goal,
            totals.TotalInflow,
            totals.TotalOutflow,
            PeriodBalanceDateSeries.Build(period, balance.OpeningBalance,
                dailyHistories.Select(item => (item.Date, item.Sequence, item.AccountingPeriodBalanceChange))),
            events.Items);
    }

    /// <summary>
    /// Retrieves a Fund's detail in an Accounting Period, or null when either is missing or they aren't related.
    /// </summary>
    public async Task<PeriodFundDetail?> GetFundAsync(
        Guid accountingPeriodId,
        Guid fundId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodId periodId = new(accountingPeriodId);
        AccountingPeriod? period = await accountingPeriodQueryRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null)
        {
            return null;
        }
        FundId id = new(fundId);
        AccountingPeriodBalanceHistory history = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(periodId);
        AccountingPeriodFundBalanceHistory? balance = history.FundBalances.SingleOrDefault(item => item.Fund.Id == id);
        if (balance == null)
        {
            return null;
        }
        FundGoal? goal = fundGoalRepository.GetByFundAndAccountingPeriod(id, periodId);
        FundGoalProgress? progress = goal == null
            ? null
            : FundGoalService.GetProgresses([goal], history).GetValueOrDefault(goal.Id);
        (AccountingPeriod? previous, AccountingPeriod? next) = await accountingPeriodQueryRepository.GetAdjacentAsync(period, cancellationToken);
        IReadOnlyCollection<FundBalanceHistory> dailyHistories = await fundBalanceEventQueryRepository.GetPeriodHistoriesAsync(id, periodId, cancellationToken);
        AccountingPeriodRangeQueryResult<QueryPage<FundGoalBalanceEvent>> events = await fundGoalBalanceEventQueryService.GetAsync(
            new FundGoalBalanceEventAccountingPeriodRangeQuery(
                accountingPeriodId, accountingPeriodId, new FundGoalBalanceEventFilter([fundId]), FundGoalBalanceEventSort.DateDescending, 0, null),
            cancellationToken);
        IReadOnlyCollection<FundGoalBalanceEvent> items = events.Range?.Items ?? [];
        var totals = BalanceEventTotals.Calculate(items, item => item.IsPosted, item => item.Type, item => item.Amount);
        return new PeriodFundDetail(
            period,
            previous,
            next,
            new AccountingPeriodFund(balance.Fund, balance.OpeningBalance, balance.ClosingBalance, progress),
            goal,
            totals.TotalInflow,
            totals.TotalOutflow,
            PeriodBalanceDateSeries.Build(period, balance.OpeningBalance,
                dailyHistories.Select(item => (item.Date, item.Sequence, item.AccountingPeriodBalanceChange))),
            items);
    }
}
