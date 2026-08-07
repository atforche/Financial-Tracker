using Models.BalanceEvents;

namespace Models.Funds;

/// <summary>
/// Model representing a balance event for a fund.
/// </summary>
public class FundBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Whether this income assignment is extra funding outside the regular
    /// monthly Fund Goal contribution.
    /// </summary>
    public bool IsExtraContribution { get; init; }

    /// <summary>
    /// Fund affected by the balance event.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Source associated with the balance event's Transaction.
    /// </summary>
    public required FundBalanceEventPartyModel Source { get; init; }

    /// <summary>
    /// Destinations associated with the balance event's Transaction.
    /// </summary>
    public required IReadOnlyList<FundBalanceEventPartyModel> Destinations { get; init; }

    /// <summary>
    /// Fund balance prior to the balance event.
    /// </summary>
    public required FundBalanceModel PreviousBalance { get; init; }

    /// <summary>
    /// Fund balance after the balance event.
    /// </summary>
    public required FundBalanceModel NewBalance { get; init; }
}