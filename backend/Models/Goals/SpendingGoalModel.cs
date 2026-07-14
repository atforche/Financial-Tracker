using Models.AccountingPeriods;
using Models.Funds;

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
    /// Fund for the Spending Goal.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Accounting Period for the Spending Goal, when one has been created.
    /// </summary>
    public required AccountingPeriodModel? AccountingPeriod { get; init; }

    /// <summary>
    /// Type of the Spending Goal.
    /// </summary>
    public required SpendingGoalTypeModel Type { get; init; }

    /// <summary>
    /// Total amount to spend for the Spending Goal
    /// </summary>
    public required decimal TotalAmountToSpend { get; init; }

    /// <summary>
    /// Total amount spent for the Spending Goal
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }

    /// <summary>
    /// Total amount spent including pending assigned amounts for the Spending Goal
    /// </summary>
    public required decimal TotalAmountSpentIncludingPending { get; init; }

    /// <summary>
    /// Remaining amount to spend for the Spending Goal
    /// </summary>
    public required decimal RemainingAmountToSpend { get; init; }

    /// <summary>
    /// Remaining amount to spend including pending assigned amounts for the Spending Goal
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