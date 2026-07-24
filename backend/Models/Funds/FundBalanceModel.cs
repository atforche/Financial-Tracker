namespace Models.Funds;

/// <summary>
/// Model representing an Fund Balance
/// </summary>
public class FundBalanceModel
{
    /// <summary>
    /// Posted Balance for the Fund Balance
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Balance including pending Transaction effects.
    /// </summary>
    public required decimal BalanceIncludingPending { get; init; }
}