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
    DateOnly Date,
    BalanceEventType Type,
    decimal Amount,
    Fund Fund,
    FundBalance PreviousBalance,
    FundBalance NewBalance);