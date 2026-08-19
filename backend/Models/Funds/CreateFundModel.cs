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
    /// Regular contribution for the Fund Goal.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Minimum funded balance for the Fund Goal.
    /// </summary>
    public decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Maximum funded balance for the Fund Goal.
    /// </summary>
    public decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Target ending balance for the Fund Goal.
    /// </summary>
    public decimal? TargetEndingBalance { get; init; }
}
