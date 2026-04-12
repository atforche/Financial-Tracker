namespace Models.Funds;

/// <summary>
/// Model representing a request to create a Fund
/// </summary>
public class CreateFundModel
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
    /// Accounting Period that the Fund is being added to
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Optional goal type to create for the Fund in the selected Accounting Period
    /// </summary>
    public FundGoalTypeModel? GoalType { get; init; }

    /// <summary>
    /// Optional goal amount to create for the Fund in the selected Accounting Period
    /// </summary>
    public decimal? GoalAmount { get; init; }
}