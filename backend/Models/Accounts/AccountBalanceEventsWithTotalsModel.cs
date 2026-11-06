using Models.BalanceEvents;

namespace Models.Accounts;

/// <summary>
/// Model representing an Account's balance events and their totals
/// </summary>
public sealed class AccountBalanceEventsWithTotalsModel
{
    /// <summary>
    /// Matching balance events
    /// </summary>
    public required IReadOnlyCollection<AccountBalanceEventModel> Events { get; init; }

    /// <summary>
    /// Totals of the matching posted balance events
    /// </summary>
    public required BalanceEventTotalsModel Totals { get; init; }
}
