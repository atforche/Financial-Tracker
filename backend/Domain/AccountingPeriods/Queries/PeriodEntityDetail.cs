using Domain.AccountGoals;
using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.FundGoals.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents everything shown for one Account in one Accounting Period.
/// </summary>
public sealed record PeriodAccountDetail(
    AccountingPeriod AccountingPeriod,
    AccountingPeriod? PreviousPeriod,
    AccountingPeriod? NextPeriod,
    AccountingPeriodAccount Account,
    AccountGoal? Goal,
    decimal TotalInflow,
    decimal TotalOutflow,
    PeriodBalanceDateRange DailyBalances,
    IReadOnlyCollection<AccountBalanceEvent> Events);

/// <summary>
/// Represents everything shown for one Fund in one Accounting Period.
/// </summary>
public sealed record PeriodFundDetail(
    AccountingPeriod AccountingPeriod,
    AccountingPeriod? PreviousPeriod,
    AccountingPeriod? NextPeriod,
    AccountingPeriodFund Fund,
    FundGoal? Goal,
    decimal TotalInflow,
    decimal TotalOutflow,
    PeriodBalanceDateRange DailyBalances,
    IReadOnlyCollection<FundGoalBalanceEvent> Events);
