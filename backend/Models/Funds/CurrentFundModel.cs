namespace Models.Funds;

/// <summary>
/// Model representing a Fund on the current Funds page.
/// </summary>
public class CurrentFundModel
{
    /// <summary>
    /// ID for the Fund.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Fund.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Current balance for the Fund.
    /// </summary>
    public required FundBalanceModel CurrentBalance { get; init; }

    /// <summary>
    /// Effective date for the most recent balance event affecting the Fund.
    /// </summary>
    public required DateOnly? LastBalanceEventDate { get; init; }

    /// <summary>
    /// Most recent balance events affecting the Fund.
    /// </summary>
    public required IReadOnlyCollection<CurrentFundBalanceEventModel> RecentBalanceEvents { get; init; }
}