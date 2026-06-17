namespace Models.Funds;

/// <summary>
/// Model representing the current Funds page response.
/// </summary>
public class CurrentFundsModel
{
    /// <summary>
    /// Available Fund Names for the current snapshot filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Current aggregate summary for the matching Funds.
    /// </summary>
    public required FundSummaryModel Summary { get; init; }

    /// <summary>
    /// Current snapshot for each Fund.
    /// </summary>
    public required IReadOnlyCollection<CurrentFundModel> Funds { get; init; }
}