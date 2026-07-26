using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.BalanceEvents;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Domain.Funds.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// A Transaction and the Domain context required for pure response conversion.
/// </summary>
public sealed class TransactionDetails(
    TransactionDetailsFacts facts,
    IReadOnlyCollection<AccountBalanceEvent> accountEvents,
    IReadOnlyCollection<FundBalanceEvent> fundEvents,
    IReadOnlyCollection<FundGoalBalanceEvent> fundGoalEvents)
{
    /// <summary>
    /// Gets the Transaction.
    /// </summary>
    public Transaction Transaction => facts.Transaction;

    /// <summary>
    /// Gets the Transaction's Accounting Period.
    /// </summary>
    public AccountingPeriod AccountingPeriod => facts.AccountingPeriod;

    /// <summary>
    /// Gets a resolved Account balance event for the Transaction.
    /// </summary>
    public AccountBalanceEvent GetAccountEvent(
        Account account,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type)
        => accountEvents.First(balanceEvent => balanceEvent.Account.Id == account.Id
            && balanceEvent.EventDate == postedDate
            && balanceEvent.Amount == amount
            && balanceEvent.Type == type);

    /// <summary>
    /// Gets a resolved Fund balance event for the Transaction.
    /// </summary>
    public FundBalanceEvent GetFundEvent(FundAmount amount, BalanceEventType type)
        => fundEvents.First(balanceEvent => balanceEvent.Fund.Id == amount.FundId
            && balanceEvent.Amount == amount.Amount
            && balanceEvent.Type == type);

    /// <summary>
    /// Gets a resolved Fund Goal balance event for the Transaction.
    /// </summary>
    public FundGoalBalanceEvent GetFundGoalEvent(
        FundAmount amount,
        DateOnly? postedDate,
        BalanceEventType type)
        => fundGoalEvents.First(balanceEvent => balanceEvent.Fund.Id == amount.FundId
            && balanceEvent.EventDate == postedDate
            && balanceEvent.Amount == amount.Amount
            && balanceEvent.Type == type);
}