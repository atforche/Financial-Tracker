using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;

namespace Domain.Actions;

/// <summary>
/// Action class that posts a Transaction
/// </summary>
/// <param name="balanceEventRepository">Balance Event Repository</param>
/// <param name="balanceEventDateValidator">Balance Event Date Validator</param>
public class PostTransactionAction(IBalanceEventRepository balanceEventRepository, BalanceEventDateValidator balanceEventDateValidator)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="transaction">Transaction to post</param>
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="postingDate">Posting Date for the Transaction in the provided Account</param>
    public void Run(
        Transaction transaction,
        TransactionAccountType accountType,
        DateOnly postingDate)
    {
        if (!IsValid(transaction,
                accountType,
                postingDate,
                out Exception? exception))
        {
            throw exception;
        }
        transaction.Post(accountType,
            postingDate,
            balanceEventRepository.GetHighestEventSequenceOnDate(postingDate) + 1);
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="transaction">Transaction to post</param>
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="postingDate">Posting Date for the Transaction in the provided Account</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(
        Transaction transaction,
        TransactionAccountType accountType,
        DateOnly postingDate,
        [NotNullWhen(false)] out Exception? exception)
    {
        Account? account = transaction.GetAccount(accountType) ?? throw new InvalidOperationException();
        _ = balanceEventDateValidator.Validate(transaction.AccountingPeriod, [account], postingDate, out exception);
        // Validate that this Transaction hasn't already been posted for the provided Account
        if (transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.AccountType == accountType &&
                balanceEvent.EventType == TransactionBalanceEventType.Posted))
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that this posting date falls after the Transaction was added
        if (transaction.TransactionBalanceEvents.Any(balanceEvent =>
                balanceEvent.EventType == TransactionBalanceEventType.Added &&
                balanceEvent.EventDate > postingDate))
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}