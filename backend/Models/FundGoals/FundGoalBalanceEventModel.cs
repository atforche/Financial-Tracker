using Models.BalanceEvents;
using Models.Funds;

namespace Models.FundGoals;

/// <summary>
/// Balance event showing a Transaction's effect on Fund Goal totals.
/// </summary>
public sealed class FundGoalBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Fund whose fundGoal totals were affected.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Source associated with the balance event's Transaction.
    /// </summary>
    public required FundGoalBalanceEventPartyModel Source { get; init; }

    /// <summary>
    /// Destinations associated with the balance event's Transaction.
    /// </summary>
    public required IReadOnlyList<FundGoalBalanceEventPartyModel> Destinations { get; init; }

    /// <summary>
    /// Totals immediately before the Transaction.
    /// </summary>
    public required FundGoalTotalsModel PreviousTotals { get; init; }

    /// <summary>
    /// Totals immediately after the Transaction.
    /// </summary>
    public required FundGoalTotalsModel NewTotals { get; init; }
}