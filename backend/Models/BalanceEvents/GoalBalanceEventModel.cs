using Models.Funds;
using Models.Goals;

namespace Models.BalanceEvents;

/// <summary>
/// Model representing a balance event for a Goal.
/// </summary>
public class GoalBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Fund affected by the balance event.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Goal balance prior to the balance event.
    /// </summary>
    public required GoalBalanceModel PreviousBalance { get; init; }

    /// <summary>
    /// Goal balance after the balance event.
    /// </summary>
    public required GoalBalanceModel NewBalance { get; init; }
}