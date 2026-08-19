using Models.BalanceEvents;

namespace Models.Accounts;

/// <summary>
/// Model representing a balance event for an account.
/// </summary>
public class AccountBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Account for the balance event.
    /// </summary>
    public required AccountModel Account { get; init; }

    /// <summary>
    /// Source of the transaction associated with the balance event.
    /// </summary>
    public required AccountBalanceEventPartyModel Source { get; init; }

    /// <summary>
    /// Destinations of the transaction associated with the balance event.
    /// </summary>
    public required IReadOnlyList<AccountBalanceEventPartyModel> Destinations { get; init; }

    /// <summary>
    /// Account balance prior to the balance event.
    /// </summary>
    public required AccountBalanceModel PreviousBalance { get; init; }

    /// <summary>
    /// Account balance after the balance event.
    /// </summary>
    public required AccountBalanceModel NewBalance { get; init; }
}
