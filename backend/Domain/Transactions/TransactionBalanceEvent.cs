using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a Transaction Balance Event
/// </summary>
/// <remarks>
/// A Transaction will generate balance events for both the credited and debited
/// accounts. For each account, a balance event will be generated when the 
/// Transaction is added and when the Transaction is posted.
/// </remarks>
public sealed class TransactionBalanceEvent : Entity<TransactionBalanceEventId>, IBalanceEvent
{
    /// <summary>
    /// Parent Transaction for this Transaction Balance Event
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <inheritdoc/>
    public AccountingPeriodId AccountingPeriodId => Transaction.AccountingPeriodId;

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; internal set; }

    /// <inheritdoc/>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Event Type for this Transaction Balance Event
    /// </summary>
    public TransactionBalanceEventType EventType { get; private set; }

    /// <summary>
    /// Account Type for this Transaction Balance Event
    /// </summary>
    public TransactionAccountType AccountType { get; private set; }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance) =>
        EventType == TransactionBalanceEventType.Added
            ? ApplyTransactionAddedBalanceEvent(currentBalance, false)
            : ApplyTransactionPostedBalanceEvent(currentBalance, false);

    /// <inheritdoc/>
    public AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) =>
        EventType == TransactionBalanceEventType.Added
            ? ApplyTransactionAddedBalanceEvent(currentBalance, true)
            : ApplyTransactionPostedBalanceEvent(currentBalance, true);

    /// <inheritdoc/>
    public bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return true;
        }
        // Posted Transaction Balance Events are always valid
        if (EventType == TransactionBalanceEventType.Posted)
        {
            return true;
        }
        // Transaction Balance Events that are increasing an Account's balance are always valid
        if (DetermineBalanceChangeFactor(currentBalance.Account, false) > 0)
        {
            return true;
        }
        // Cannot apply this Balance Event if it will take the Accounts overall balance negative
        // For simplicity, count pending balance decreases but don't count pending balance increases.
        return Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) - Transaction.FundAmounts.Sum(entry => entry.Amount) >= 0;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Parent Transaction for this Transaction Balance Event</param>
    /// <param name="eventDate">Event Date for this Transaction Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Transaction Balance Event</param>
    /// <param name="accountId">Account ID for this Transaction Balance Event</param>
    /// <param name="eventType">Event Type for this Transaction Balance Event</param>
    /// <param name="accountType">Account Type for this Transaction Balance Event</param>
    internal TransactionBalanceEvent(
        Transaction transaction,
        DateOnly eventDate,
        int eventSequence,
        AccountId accountId,
        TransactionBalanceEventType eventType,
        TransactionAccountType accountType)
        : base(new TransactionBalanceEventId(Guid.NewGuid()))
    {
        Transaction = transaction;
        EventDate = eventDate;
        EventSequence = eventSequence;
        AccountId = accountId;
        EventType = eventType;
        AccountType = accountType;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionBalanceEvent() : base()
    {
        Transaction = null!;
        AccountId = null!;
    }

    /// <summary>
    /// Applies an Added Transaction Balance Event to the provided balance
    /// </summary>
    /// <param name="currentBalance">Account Balance to apply this event to</param>
    /// <param name="isReverse">True if this Transaction Balance Event is being reversed, false otherwise</param>
    /// <returns>The new Account Balance after this event has been applied</returns>
    private AccountBalance ApplyTransactionAddedBalanceEvent(AccountBalance currentBalance, bool isReverse)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        var pendingFundBalanceChanges = currentBalance.PendingFundBalanceChanges
            .ToDictionary(fundAmount => fundAmount.FundId, fundAmount => fundAmount.Amount);
        foreach (FundAmount fundAmount in Transaction.FundAmounts)
        {
            decimal balanceChange = fundAmount.Amount * DetermineBalanceChangeFactor(currentBalance.Account, isReverse);
            if (!pendingFundBalanceChanges.TryAdd(fundAmount.FundId, balanceChange))
            {
                pendingFundBalanceChanges[fundAmount.FundId] =
                    pendingFundBalanceChanges[fundAmount.FundId] + balanceChange;
            }
        }
        return new AccountBalance(currentBalance.Account,
            currentBalance.FundBalances,
            pendingFundBalanceChanges
                .Select(pair => new FundAmount
                {
                    FundId = pair.Key,
                    Amount = pair.Value,
                }));
    }

    /// <summary>
    /// Applies a Posted Transaction Balance Event to the provided balance
    /// </summary>
    /// <param name="currentBalance">Account Balance to apply this event to</param>
    /// <param name="isReverse">True if this Transaction Balance Event is being reversed, false otherwise</param>
    /// <returns>The new Account Balance after this event has been applied</returns>
    private AccountBalance ApplyTransactionPostedBalanceEvent(AccountBalance currentBalance, bool isReverse)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        // Posting a transaction removes the pending balance change and adds it to the actual balance
        var fundBalances = currentBalance.FundBalances
            .ToDictionary(fundAmount => fundAmount.FundId, fundAmount => fundAmount.Amount);
        var pendingFundBalanceChanges = currentBalance.PendingFundBalanceChanges
            .ToDictionary(fundAmount => fundAmount.FundId, fundAmount => fundAmount.Amount);
        foreach (FundAmount fundAmount in Transaction.FundAmounts)
        {
            decimal balanceChange = fundAmount.Amount * DetermineBalanceChangeFactor(currentBalance.Account, isReverse);
            if (!pendingFundBalanceChanges.TryAdd(fundAmount.FundId, -balanceChange))
            {
                pendingFundBalanceChanges[fundAmount.FundId] =
                    pendingFundBalanceChanges[fundAmount.FundId] - balanceChange;
            }
            if (pendingFundBalanceChanges[fundAmount.FundId] == 0)
            {
                _ = pendingFundBalanceChanges.Remove(fundAmount.FundId);
            }
            if (!fundBalances.TryAdd(fundAmount.FundId, balanceChange))
            {
                fundBalances[fundAmount.FundId] = fundBalances[fundAmount.FundId] + balanceChange;
            }
        }
        return new AccountBalance(currentBalance.Account,
            fundBalances
                .Select(pair => new FundAmount
                {
                    FundId = pair.Key,
                    Amount = pair.Value,
                }),
            pendingFundBalanceChanges
                .Select(pair => new FundAmount
                {
                    FundId = pair.Key,
                    Amount = pair.Value,
                }));
    }

    /// <summary>
    /// Determines how the balance of an Account will change when a Transaction is applied
    /// </summary>
    /// <param name="account">Account to have a Transaction applied</param>
    /// <param name="isReverse">True if this Transaction Balance Event is being reversed, false otherwise</param>
    /// <returns>An enum representing how the Transaction will change the Account's balance</returns>
    private int DetermineBalanceChangeFactor(Account account, bool isReverse)
    {
        int DetermineBalanceChangeFactor(Account account)
        {
            if (AccountType == TransactionAccountType.Debit && account.Type == Accounts.AccountType.Debt)
            {
                return 1;
            }
            if (AccountType != TransactionAccountType.Debit && account.Type != Accounts.AccountType.Debt)
            {
                return 1;
            }
            return -1;
        }
        return !isReverse ? DetermineBalanceChangeFactor(account) : DetermineBalanceChangeFactor(account) * -1;
    }
}

/// <summary>
/// Enum representing the different types of Transaction Balance Event
/// </summary>
public enum TransactionBalanceEventType
{
    /// <summary>
    /// Event that corresponds to a new Transaction being added
    /// </summary>
    Added,

    /// <summary>
    /// Event that corresponds to an existing Transaction being posted
    /// </summary>
    Posted,
}

/// <summary>
/// Enum representing the different Accounts affected by the Transaction
/// </summary>
public enum TransactionAccountType
{
    /// <summary>
    /// Account that is being debited by the Transaction
    /// </summary>
    Debit,

    /// <summary>
    /// Account that is being credited by the Transaction
    /// </summary>
    Credit,
}

/// <summary>
/// Value object class representing the ID of an <see cref="TransactionBalanceEvent"/>
/// </summary>
public record TransactionBalanceEventId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Transaction Balance Event ID during 
    /// Transaction creation. 
    /// </summary>
    /// <param name="value">Value for this Transaction Balance Event ID</param>
    internal TransactionBalanceEventId(Guid value)
        : base(value)
    {
    }
}