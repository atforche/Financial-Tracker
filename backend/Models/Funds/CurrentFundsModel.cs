namespace Models.Funds;

/// <summary>
/// Model representing the current Funds page response.
/// </summary>
public class CurrentFundsModel
{
    /// <summary>
    /// Current aggregate summary for all Funds.
    /// </summary>
    public required FundSummaryModel Summary { get; init; }

    /// <summary>
    /// Current snapshot for each Fund.
    /// </summary>
    public required IReadOnlyCollection<CurrentFundModel> Funds { get; init; }
}