using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Transactions;

namespace Domain.Accounts.Queries;

/// <summary>
/// Represents an interpreted Account balance event.
/// </summary>
public sealed record AccountBalanceEvent(
    AccountingPeriod AccountingPeriod,
    TransactionId TransactionId,
    DateOnly? Date,
    BalanceEventType Type,
    decimal Amount,
    Account Account,
    AccountBalance PreviousBalance,
    AccountBalance NewBalance)
{
    /// <summary>
    /// Gets whether the event has posted.
    /// </summary>
    public bool IsPosted => Date.HasValue;
}