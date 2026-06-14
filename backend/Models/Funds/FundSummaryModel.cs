namespace Models.Funds;

/// <summary>
/// Model representing summary balances for Funds.
/// </summary>
public class FundSummaryModel
{
    /// <summary>
    /// Sum of the posted balances for all Funds.
    /// </summary>
    public required decimal TotalTrackedBalance { get; init; }

    /// <summary>
    /// Sum of the posted balances for all Funds except the unassigned Fund.
    /// </summary>
    public required decimal TotalAssignedBalance { get; init; }

    /// <summary>
    /// Posted balance of the unassigned Fund.
    /// </summary>
    public required decimal TotalUnassignedBalance { get; init; }
}