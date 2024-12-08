using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

/// <summary>
/// Entity class representing a Transaction Balance Event
/// </summary>
/// <remarks>
/// A Transaction will generate balance events for both the credited and debited
/// accounts. For each account, a balance event will be generated when the 
/// Transaction is added and when the Transaction is posted.
/// </remarks>
public sealed class TransactionBalanceEvent : EntityBase, IBalanceEvent
{
    /// <summary>
    /// Parent Transaction for this Transaction Balance Event
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <inheritdoc/>
    public Account Account { get; private set; }

    /// <inheritdoc/>
    public AccountingPeriod AccountingPeriod => Transaction.AccountingPeriod;

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Event Type for this Transaction Balance Event
    /// </summary>
    public TransactionBalanceEventType TransactionEventType { get; private set; }

    /// <summary>
    /// Account Type for this Transaction Balance Event
    /// </summary>
    public TransactionAccountType TransactionAccountType { get; private set; }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance)
    {
        if (Account.Id != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        return TransactionEventType == TransactionBalanceEventType.Added
            ? ApplyTransactionAddedBalanceEvent(currentBalance, false)
            : ApplyTransactionPostedBalanceEvent(currentBalance, false);
    }

    /// <inheritdoc/>
    public AccountBalance ReverseEventFromBalance(AccountBalance currentBalance)
    {
        if (Account.Id != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        return TransactionEventType == TransactionBalanceEventType.Added
            ? ApplyTransactionAddedBalanceEvent(currentBalance, true)
            : ApplyTransactionPostedBalanceEvent(currentBalance, true);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Parent Transaction for this Transaction Balance Event</param>
    /// <param name="account">Account for this Transaction Balance Event</param>
    /// <param name="eventDate">Event Date for this Transaction Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Transaction Balance Event</param>
    /// <param name="transactionEventType">Transaction Balance Event Type for this Transaction Balance Event</param>
    /// <param name="transactionAccountType">Transaction Account Type for this Transaction Balance Event</param>
    internal TransactionBalanceEvent(Transaction transaction,
        Account account,
        DateOnly eventDate,
        int eventSequence,
        TransactionBalanceEventType transactionEventType,
        TransactionAccountType transactionAccountType)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Account = account;
        EventDate = eventDate;
        EventSequence = eventSequence;
        TransactionEventType = transactionEventType;
        TransactionAccountType = transactionAccountType;
        Transaction = transaction;
        Validate();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionBalanceEvent()
        : base(new EntityId(default(long), Guid.NewGuid()))
    {
        Account = null!;
        Transaction = null!;
    }

    /// <summary>
    /// Validates the current Transaction Balance Event
    /// </summary>
    private void Validate()
    {
        if (EventDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        if (EventSequence <= 0)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Applies an Added Transaction Balance Event to the provided balance
    /// </summary>
    /// <param name="currentBalance">Account Balance to apply this event to</param>
    /// <param name="isReverse">True if this Transaction Balance Event is being reversed, false otherwise</param>
    /// <returns>The new Account Balance after this event has been applied</returns>
    private AccountBalance ApplyTransactionAddedBalanceEvent(AccountBalance currentBalance, bool isReverse)
    {
        Dictionary<Fund, decimal> pendingFundBalanceChanges = currentBalance.PendingFundBalanceChanges
            .ToDictionary(fundAmount => fundAmount.Fund, fundAmount => fundAmount.Amount);
        foreach (FundAmount fundAmount in Transaction.AccountingEntries)
        {
            decimal balanceChange = fundAmount.Amount * DetermineBalanceChangeFactor(currentBalance.Account, isReverse);
            if (!pendingFundBalanceChanges.TryAdd(fundAmount.Fund, balanceChange))
            {
                pendingFundBalanceChanges[fundAmount.Fund] =
                    pendingFundBalanceChanges[fundAmount.Fund] + balanceChange;
            }
        }
        return new AccountBalance(currentBalance.Account,
            currentBalance.FundBalances,
            pendingFundBalanceChanges
                .Select(pair => new FundAmount
                {
                    Fund = pair.Key,
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
        // Posting a transaction removes the pending balance change and adds it to the actual balance
        Dictionary<Fund, decimal> fundBalances = currentBalance.FundBalances
            .ToDictionary(fundAmount => fundAmount.Fund, fundAmount => fundAmount.Amount);
        Dictionary<Fund, decimal> pendingFundBalanceChanges = currentBalance.PendingFundBalanceChanges
            .ToDictionary(fundAmount => fundAmount.Fund, fundAmount => fundAmount.Amount);
        foreach (FundAmount fundAmount in Transaction.AccountingEntries)
        {
            decimal balanceChange = fundAmount.Amount * DetermineBalanceChangeFactor(currentBalance.Account, isReverse);
            if (!pendingFundBalanceChanges.TryAdd(fundAmount.Fund, -balanceChange))
            {
                pendingFundBalanceChanges[fundAmount.Fund] =
                    pendingFundBalanceChanges[fundAmount.Fund] - balanceChange;
            }
            if (pendingFundBalanceChanges[fundAmount.Fund] == 0)
            {
                pendingFundBalanceChanges.Remove(fundAmount.Fund);
            }
            if (!fundBalances.TryAdd(fundAmount.Fund, balanceChange))
            {
                fundBalances[fundAmount.Fund] = fundBalances[fundAmount.Fund] + balanceChange;
            }
        }
        return new AccountBalance(currentBalance.Account,
            fundBalances
                .Select(pair => new FundAmount
                {
                    Fund = pair.Key,
                    Amount = pair.Value,
                }),
            pendingFundBalanceChanges
                .Select(pair => new FundAmount
                {
                    Fund = pair.Key,
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
            if (TransactionAccountType == TransactionAccountType.Debit && account.Type == AccountType.Debt)
            {
                return 1;
            }
            if (TransactionAccountType != TransactionAccountType.Debit && account.Type != AccountType.Debt)
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