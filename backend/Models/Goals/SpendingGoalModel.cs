namespace Models.Goals;

/// <summary>
/// Model representing a Spending Goal.
/// </summary>
public class SpendingGoalModel
{
    /// <summary>
    /// ID of the Spending Goal.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Fund ID for the Spending Goal
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund name for the Spending Goal
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Accounting Period ID for the Spending Goal
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Accounting Period name for the Spending Goal
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Type of the Spending Goal.
    /// </summary>
    public required SpendingGoalTypeModel Type { get; init; }

    /// <summary>
    /// Total amount to spend for the Spending Goal
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Spending Goal
    /// </summary>
    public required decimal RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Spending Goal including pending assigned amounts
    /// </summary>
    public required decimal RemainingAmountToSpendIncludingPending { get; init; }

    /// <summary>
    /// Whether the Spending Goal has been met
    /// </summary>
    public required bool IsGoalMet { get; init; }

    /// <summary>
    /// Whether the Spending Goal has been met including pending assigned amounts
    /// </summary>
    public required bool IsGoalMetIncludingPending { get; init; }
}