using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Represents an interpreted Fund Goal balance event.
/// </summary>
public sealed record FundGoalBalanceEvent(
    AccountingPeriod AccountingPeriod,
    TransactionId TransactionId,
    string Description,
    DateOnly TransactionDate,
    int TransactionSequence,
    DateOnly? EventDate,
    int? EventDateSequence,
    BalanceEventType Type,
    decimal Amount,
    Fund Fund,
    FundGoalBalanceEventParty Source,
    IReadOnlyList<FundGoalBalanceEventParty> Destinations,
    FundGoalTotals PreviousTotals,
    FundGoalTotals NewTotals)
{
    /// <summary>
    /// Gets whether the event has posted.
    /// </summary>
    public bool IsPosted => EventDate.HasValue;
}
