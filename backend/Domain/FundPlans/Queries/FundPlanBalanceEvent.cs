using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Represents an interpreted Fund Plan balance event.
/// </summary>
public sealed record FundPlanBalanceEvent(
    AccountingPeriod AccountingPeriod,
    TransactionId TransactionId,
    DateOnly TransactionDate,
    int TransactionSequence,
    DateOnly? EventDate,
    int? EventDateSequence,
    BalanceEventType Type,
    decimal Amount,
    Fund Fund,
    FundPlanTotals PreviousTotals,
    FundPlanTotals NewTotals)
{
    /// <summary>
    /// Gets whether the event has posted.
    /// </summary>
    public bool IsPosted => EventDate.HasValue;
}