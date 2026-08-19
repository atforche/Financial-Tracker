namespace Models.Funds;

/// <summary>
/// Model representing a summary of fund balances for a specific date.
/// </summary>
public class FundBalanceSummaryByDateModel : FundBalanceSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }
}
