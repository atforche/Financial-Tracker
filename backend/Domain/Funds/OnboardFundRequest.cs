using Domain.Goals;

namespace Domain.Funds;

/// <summary>
/// Record representing a request to onboard a <see cref="Fund"/>
/// </summary>
public record OnboardFundRequest
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
    /// Onboarded Balance for the Fund
    /// </summary>
    public required decimal OnboardedBalance { get; init; }

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