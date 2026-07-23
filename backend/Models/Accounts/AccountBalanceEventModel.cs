using Models.BalanceEvents;

namespace Models.Accounts;

/// <summary>
/// Model representing a balance event for an account.
/// </summary>
public class AccountBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Transaction date for the balance event.
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Transaction sequence for the balance event.
    /// </summary>
    public required int TransactionSequence { get; init; }

    /// <summary>
    /// Sequence within the event date, or null when the event is unposted.
    /// </summary>
    public int? EventDateSequence { get; init; }

    /// <summary>
    /// Account for the balance event.
    /// </summary>
    public required AccountModel Account { get; init; }

    /// <summary>
    /// Account balance prior to the balance event.
    /// </summary>
    public required AccountBalanceModel PreviousBalance { get; init; }

    /// <summary>
    /// Account balance after the balance event.
    /// </summary>
    public required AccountBalanceModel NewBalance { get; init; }
}