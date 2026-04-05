using Domain.Funds;

namespace Domain.Budgets;

/// <summary>
/// Record representing a request to create a <see cref="Budget"/>
/// </summary>
public record CreateBudgetRequest
{
    /// <summary>
    /// Name for the Budget
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Budget
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Type of the Budget
    /// </summary>
    public required BudgetType Type { get; init; }

    /// <summary>
    /// Fund ID linked to the Budget
    /// </summary>
    public required FundId FundId { get; init; }
}
