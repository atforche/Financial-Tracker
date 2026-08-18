namespace Models.Funds;

/// <summary>
/// Model representing a Fund along with its current balance.
/// </summary>
public class FundWithBalanceModel : FundModel
{
    /// <summary>
    /// Current balance for the Fund along with its details.
    /// </summary>
    public required FundBalanceModel CurrentBalance { get; init; }
}
