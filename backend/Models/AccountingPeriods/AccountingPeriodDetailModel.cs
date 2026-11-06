using Models.AccountGoals;
using Models.Accounts;
using Models.FundGoals;
using Models.Funds;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing everything shown for a single Accounting Period.
/// </summary>
public class AccountingPeriodDetailModel
{
    /// <summary>
    /// The Accounting Period with its balance and totals.
    /// </summary>
    public required AccountingPeriodWithBalanceModel AccountingPeriod { get; init; }

    /// <summary>
    /// The Accounting Period immediately before this one, or null when this is the first.
    /// </summary>
    public required AccountingPeriodModel? PreviousPeriod { get; init; }

    /// <summary>
    /// The Accounting Period immediately after this one, or null when this is the latest.
    /// </summary>
    public required AccountingPeriodModel? NextPeriod { get; init; }

    /// <summary>
    /// Posted spending, net of refunds, during the Accounting Period.
    /// </summary>
    public required decimal TotalSpending { get; init; }

    /// <summary>
    /// Account balances at the start of the Accounting Period.
    /// </summary>
    public required AccountBalanceSummaryModel AccountOpeningBalance { get; init; }

    /// <summary>
    /// Account balances at the end of the Accounting Period.
    /// </summary>
    public required AccountBalanceSummaryModel AccountClosingBalance { get; init; }

    /// <summary>
    /// Fund balances at the start of the Accounting Period.
    /// </summary>
    public required FundBalanceSummaryModel FundOpeningBalance { get; init; }

    /// <summary>
    /// Fund balances at the end of the Accounting Period.
    /// </summary>
    public required FundBalanceSummaryModel FundClosingBalance { get; init; }

    /// <summary>
    /// The Accounts in the Accounting Period with their balances and goal progress.
    /// </summary>
    public required IReadOnlyCollection<AccountingPeriodAccountModel> Accounts { get; init; }

    /// <summary>
    /// The Funds in the Accounting Period with their balances and goal progress.
    /// </summary>
    public required IReadOnlyCollection<AccountingPeriodFundModel> Funds { get; init; }
}

/// <summary>
/// Model representing an Account's balances and goal progress in an Accounting Period.
/// </summary>
public class AccountingPeriodAccountModel
{
    /// <summary>
    /// The Account.
    /// </summary>
    public required AccountModel Account { get; init; }

    /// <summary>
    /// The Account's balance at the start of the Accounting Period.
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// The Account's balance at the end of the Accounting Period.
    /// </summary>
    public required decimal ClosingBalance { get; init; }

    /// <summary>
    /// The Account's goal progress, or null when it has no goal in the Accounting Period.
    /// </summary>
    public AccountGoalProgressModel? GoalProgress { get; init; }
}

/// <summary>
/// Model representing a Fund's balances and goal progress in an Accounting Period.
/// </summary>
public class AccountingPeriodFundModel
{
    /// <summary>
    /// The Fund.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// The Fund's balance at the start of the Accounting Period.
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// The Fund's balance at the end of the Accounting Period.
    /// </summary>
    public required decimal ClosingBalance { get; init; }

    /// <summary>
    /// The Fund's goal progress, or null when it has no goal in the Accounting Period.
    /// </summary>
    public FundGoalProgressModel? GoalProgress { get; init; }
}
