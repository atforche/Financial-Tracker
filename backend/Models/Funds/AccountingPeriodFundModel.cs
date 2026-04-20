namespace Models.Funds;

/// <summary>
/// Model representing a Fund in the context of a specific Accounting Period
/// </summary>
public class AccountingPeriodFundModel
{
    /// <summary>
    /// ID for the Fund
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether the Fund is a system-defined fund
    /// </summary>
    public required bool IsSystemFund { get; init; }

    /// <summary>
    /// Opening balance for the Fund
    /// </summary>
    public required FundBalanceModel OpeningBalance { get; init; }

    /// <summary>
    /// Amount assigned to the Fund for the Accounting Period
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Amount spent from the Fund during the Accounting Period
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Closing balance for the Fund
    /// </summary>
    public required FundBalanceModel ClosingBalance { get; init; }

    /// <summary>
    /// Goal type for the Fund during the Accounting Period
    /// </summary>
    public required FundGoalTypeModel? GoalType { get; init; }

    /// <summary>
    /// Goal amount for the Fund during the Accounting Period
    /// </summary>
    public required decimal? GoalAmount { get; init; }

    /// <summary>
    /// Pending amount assigned for the Fund during the Accounting Period
    /// </summary>
    public required decimal? PendingAmountAssigned { get; init; }

    /// <summary>
    /// Remaining amount to assign for the Fund during the Accounting Period
    /// </summary>
    public required decimal? RemainingAmountToAssign { get; init; }

    /// <summary>
    /// Whether the assignment goal has been met for the Fund during the Accounting Period
    /// </summary>
    public required bool? IsAssignmentGoalMet { get; init; }

    /// <summary>
    /// Pending amount spent for the Fund during the Accounting Period
    /// </summary>
    public required decimal? PendingAmountSpent { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Fund during the Accounting Period
    /// </summary>
    public required decimal? RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Whether the spending goal has been met for the Fund during the Accounting Period
    /// </summary>
    public required bool? IsSpendingGoalMet { get; init; }
}