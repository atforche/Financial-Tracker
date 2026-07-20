using Models.FundPlans;
using Models.Funds;

namespace Models.BalanceEvents;

/// <summary>
/// Balance event showing a Transaction's effect on Fund Plan totals.
/// </summary>
public sealed class FundPlanBalanceEventModel : BalanceEventModel
{
    /// <summary>
    /// Fund whose plan totals were affected.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Totals immediately before the Transaction.
    /// </summary>
    public required FundPlanTotalsModel PreviousTotals { get; init; }

    /// <summary>
    /// Totals immediately after the Transaction.
    /// </summary>
    public required FundPlanTotalsModel NewTotals { get; init; }
}