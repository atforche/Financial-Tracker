using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// Account ID for this Transaction Balance Event
    /// </summary>
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
    public IReadOnlyCollection<AccountId> GetAccountIds() => [AccountId];

    /// <inheritdoc/>
    public bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (AccountId != currentBalance.Account.Id)
        {
            // Balance Event doesn't affect the provided balance
            return true;
        }
        if (EventType == TransactionBalanceEventType.Posted)
        {
            // Posted Transaction Balance Events are always valid
            return true;
        }
        if (DetermineFundAmountsToApply(currentBalance.Account.Type, ApplicationDirection.Standard)
                .All(fundAmount => fundAmount.Amount >= 0))
        {
            // Transaction Balance Events that are increasing an Account's balance are always valid
            return true;
        }
        if (Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) - Transaction.FundAmounts.Sum(entry => entry.Amount) < 0)
        {
            // Cannot apply this Balance Event if it will take the Accounts overall balance negative
            // For simplicity, count pending balance decreases but don't count pending balance increases.
            exception = new InvalidOperationException();
        }
        return exception == null;
    }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance, ApplicationDirection direction)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        foreach (FundAmount fundAmount in DetermineFundAmountsToApply(currentBalance.Account.Type, direction))
        {
            if (EventType == TransactionBalanceEventType.Added)
            {
                currentBalance = currentBalance.AddNewPendingAmount(fundAmount);
            }
            else
            {
                currentBalance = currentBalance.AddNewAmount(fundAmount);
                currentBalance = currentBalance.AddNewPendingAmount(fundAmount.GetWithReversedAmount());
            }
        }
        return currentBalance;
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
    /// Determines the Fund Amounts that should be applied to the Account Balance given the account type and reversal flag
    /// </summary>
    /// <param name="accountType">Account Type that this Transaction Balance Event is being applied to</param>
    /// <param name="direction">Direction in which to apply this Balance Event</param>
    /// <returns>The Fund Amounts that should be applied to the Account Balance</returns>
    private IReadOnlyCollection<FundAmount> DetermineFundAmountsToApply(AccountType accountType, ApplicationDirection direction)
    {
        bool shouldBalanceEventIncreaseAccountBalance =
            (AccountType == TransactionAccountType.Debit && accountType == Accounts.AccountType.Debt) ||
            (AccountType == TransactionAccountType.Credit && accountType != Accounts.AccountType.Debt);
        if (direction == ApplicationDirection.Reverse)
        {
            shouldBalanceEventIncreaseAccountBalance = !shouldBalanceEventIncreaseAccountBalance;
        }
        return shouldBalanceEventIncreaseAccountBalance
            ? Transaction.FundAmounts
            : Transaction.FundAmounts.Select(fundAmount => fundAmount.GetWithReversedAmount()).ToList();
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