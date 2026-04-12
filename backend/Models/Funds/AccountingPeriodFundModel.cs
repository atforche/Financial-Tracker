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
    /// Whether the goal has been met for the Fund during the Accounting Period
    /// </summary>
    public required bool? IsGoalMet { get; init; }
}