using Models.Funds;

namespace Models.BalanceEvents;

/// <summary>
/// Model representing a balance event for a fund.
/// </summary>
public class FundBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Fund affected by the balance event.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Fund balance prior to the balance event.
    /// </summary>
    public required FundBalanceModel PreviousBalance { get; init; }

    /// <summary>
    /// Fund balance after the balance event.
    /// </summary>
    public required FundBalanceModel NewBalance { get; init; }
}