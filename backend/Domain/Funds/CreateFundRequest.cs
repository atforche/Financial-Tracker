using Domain.AccountingPeriods;
using Domain.Goals;

namespace Domain.Funds;

/// <summary>
/// Record representing a request to create a <see cref="Fund"/>
/// </summary>
public record CreateFundRequest
{
    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Opening Accounting Period for the Fund
    /// </summary>
    public required AccountingPeriod OpeningAccountingPeriod { get; init; }

    /// <summary>
    /// Assignment goal behavior for the Fund.
    /// </summary>
    public required AssignmentGoalType AssignmentGoalType { get; init; }

    /// <summary>
    /// Assignment goal amount for the Fund.
    /// </summary>
    public required decimal AssignmentGoalAmount { get; init; }

    /// <summary>
    /// Spending goal behavior for the Fund.
    /// </summary>
    public required SpendingGoalType SpendingGoalType { get; init; }
}