using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Transactions;

namespace Domain.Funds.Queries;

/// <summary>
/// Represents an interpreted Fund balance event.
/// </summary>
public sealed record FundBalanceEvent(
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
    FundBalanceEventParty Source,
    IReadOnlyList<FundBalanceEventParty> Destinations,
    FundBalance PreviousBalance,
    FundBalance NewBalance)
{
    /// <summary>
    /// Gets whether the event has posted.
    /// </summary>
    public bool IsPosted => EventDate.HasValue;
}