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
    /// Type for the Fund
    /// </summary>
    public required FundTypeModel Type { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Accounting Period that the Fund is being added to
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }
}