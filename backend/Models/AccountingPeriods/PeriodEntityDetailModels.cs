using Models.AccountGoals;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundGoals;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing everything shown for one Account or Fund in one Accounting Period
/// </summary>
public abstract class PeriodEntityDetailModel
{
    /// <summary>
    /// Accounting Period being shown
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Accounting Period immediately before this one, when one exists
    /// </summary>
    public required AccountingPeriodModel? PreviousPeriod { get; init; }

    /// <summary>
    /// Accounting Period immediately after this one, when one exists
    /// </summary>
    public required AccountingPeriodModel? NextPeriod { get; init; }

    /// <summary>
    /// Total posted inflow during the Accounting Period
    /// </summary>
    public required decimal TotalInflow { get; init; }

    /// <summary>
    /// Total posted outflow during the Accounting Period
    /// </summary>
    public required decimal TotalOutflow { get; init; }

    /// <summary>
    /// Daily posted balances during the Accounting Period
    /// </summary>
    public required PeriodBalanceDateRangeModel DailyBalances { get; init; }
}

/// <summary>
/// Model representing everything shown for one Account in one Accounting Period
/// </summary>
public sealed class PeriodAccountDetailModel : PeriodEntityDetailModel
{
    /// <summary>
    /// Account's balances and goal progress in the Accounting Period
    /// </summary>
    public required AccountingPeriodAccountModel Account { get; init; }

    /// <summary>
    /// Account Goal for the Accounting Period, when one exists
    /// </summary>
    public required AccountGoalModel? Goal { get; init; }

    /// <summary>
    /// Account's balance events in the Accounting Period, newest first
    /// </summary>
    public required IReadOnlyCollection<AccountBalanceEventModel> Events { get; init; }
}

/// <summary>
/// Model representing everything shown for one Fund in one Accounting Period
/// </summary>
public sealed class PeriodFundDetailModel : PeriodEntityDetailModel
{
    /// <summary>
    /// Fund's balances and goal progress in the Accounting Period
    /// </summary>
    public required AccountingPeriodFundModel Fund { get; init; }

    /// <summary>
    /// Fund Goal for the Accounting Period, when one exists
    /// </summary>
    public required FundGoalModel? Goal { get; init; }

    /// <summary>
    /// Fund's balance events in the Accounting Period, newest first
    /// </summary>
    public required IReadOnlyCollection<FundGoalBalanceEventModel> Events { get; init; }
}
