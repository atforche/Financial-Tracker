using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a Transaction Balance Event Part
/// </summary>
public sealed class TransactionBalanceEventPart : Entity<TransactionBalanceEventPartId>
{
    /// <summary>
    /// Parent Transaction Balance Event for this Transaction Balance Event Part
    /// </summary>
    public TransactionBalanceEvent TransactionBalanceEvent { get; private set; }

    /// <summary>
    /// Type for this Transaction Balance Event Part
    /// </summary>
    public TransactionBalanceEventPartType Type { get; private set; }

    /// <summary>
    /// Gets the Account ID associated with this Transaction Balance Event Part
    /// </summary>
    public AccountId AccountId => Type is TransactionBalanceEventPartType.AddedDebit or TransactionBalanceEventPartType.PostedDebit
        ? TransactionBalanceEvent.Transaction.DebitAccountId ?? throw new InvalidOperationException()
        : TransactionBalanceEvent.Transaction.CreditAccountId ?? throw new InvalidOperationException();

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transactionBalanceEvent">Transaction Balance Event for this Transaction Balance Event Part</param>
    /// <param name="type">Type for this Transaction Balance Event Part</param>
    internal TransactionBalanceEventPart(
        TransactionBalanceEvent transactionBalanceEvent,
        TransactionBalanceEventPartType type)
        : base(new TransactionBalanceEventPartId(Guid.NewGuid()))
    {
        TransactionBalanceEvent = transactionBalanceEvent;
        Type = type;
        Validate(type);
    }

    /// <inheritdoc cref="IBalanceEvent.IsValidToApplyToBalance"/>
    internal bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (AccountId != currentBalance.Account.Id)
        {
            // Balance Event doesn't affect the provided balance
            return true;
        }
        if (Type is TransactionBalanceEventPartType.PostedDebit or TransactionBalanceEventPartType.PostedCredit)
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
        if (Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) -
                TransactionBalanceEvent.Transaction.FundAmounts.Sum(entry => entry.Amount) < 0)
        {
            // Cannot apply this Balance Event if it will take the Accounts overall balance negative
            // For simplicity, count pending balance decreases but don't count pending balance increases.
            exception = new InvalidOperationException();
        }
        return exception == null;
    }

    /// <inheritdoc cref="IBalanceEvent.ApplyEventToBalance"/>
    internal AccountBalance ApplyEventPartToBalance(AccountBalance currentBalance, ApplicationDirection direction)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        foreach (FundAmount fundAmount in DetermineFundAmountsToApply(currentBalance.Account.Type, direction))
        {
            if (Type is TransactionBalanceEventPartType.AddedDebit or TransactionBalanceEventPartType.AddedCredit)
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
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionBalanceEventPart() : base() => TransactionBalanceEvent = null!;

    /// <summary>
    /// Determines the Fund Amounts that should be applied to the Account Balance given the account type and reversal flag
    /// </summary>
    /// <param name="accountType">Account Type that this Transaction Balance Event is being applied to</param>
    /// <param name="direction">Direction in which to apply this Balance Event</param>
    /// <returns>The Fund Amounts that should be applied to the Account Balance</returns>
    private IReadOnlyCollection<FundAmount> DetermineFundAmountsToApply(AccountType accountType, ApplicationDirection direction)
    {
        bool shouldBalanceEventIncreaseAccountBalance =
            (Type is TransactionBalanceEventPartType.AddedDebit or TransactionBalanceEventPartType.PostedDebit && accountType == AccountType.Debt) ||
            (Type is TransactionBalanceEventPartType.AddedCredit or TransactionBalanceEventPartType.PostedCredit && accountType != AccountType.Debt);
        if (direction == ApplicationDirection.Reverse)
        {
            shouldBalanceEventIncreaseAccountBalance = !shouldBalanceEventIncreaseAccountBalance;
        }
        return shouldBalanceEventIncreaseAccountBalance
            ? TransactionBalanceEvent.Transaction.FundAmounts
            : TransactionBalanceEvent.Transaction.FundAmounts.Select(fundAmount => fundAmount.GetWithReversedAmount()).ToList();
    }

    /// <summary>
    /// Validates this Transaction Balance Event Part
    /// </summary>
    /// <param name="type">Type for this Transaction Balance Event Part</param>
    private void Validate(TransactionBalanceEventPartType type)
    {
        if (TransactionBalanceEvent.Transaction.TransactionBalanceEvents.SelectMany(balanceEvent => balanceEvent.Parts).Any(part => part.Type == type))
        {
            // Ensure there isn't a duplicate Transaction Balance Event Part under any balance event under this transaction
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Enum representing the different types of Transaction Balance Event Parts
/// </summary>
public enum TransactionBalanceEventPartType
{
    /// <summary>
    /// Event Part corresponding to a new Transaction being added in the Debit Account
    /// </summary>
    AddedDebit,

    /// <summary>
    /// Event Part corresponding to a new Transaction being added in the Credit Account
    /// </summary>
    AddedCredit,

    /// <summary>
    /// Event Part corresponding to the Transaction being posted in the Debit Account
    /// </summary>
    PostedDebit,

    /// <summary>
    /// Event Part corresponding to the Transaction being posted in the Credit Account
    /// </summary>
    PostedCredit,
}

/// <summary>
/// Value object class representing the ID of an <see cref="TransactionBalanceEventPart"/>
/// </summary>
public record TransactionBalanceEventPartId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Transaction Balance Event Part ID during 
    /// Transaction creation / posting. 
    /// </summary>
    /// <param name="value">Value for this Transaction Balance Event Part ID</param>
    internal TransactionBalanceEventPartId(Guid value)
        : base(value)
    {
    }
}