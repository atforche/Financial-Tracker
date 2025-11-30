using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Domain.Transactions;

/// <summary>
/// Action class that posts a Transaction
/// </summary>
public class PostTransactionAction(TransactionBalanceEventFactory transactionBalanceEventFactory)
{
    /// <summary>
    /// Runs this Action
    /// </summary>
    /// <param name="transaction">Transaction to post</param>
    /// <param name="accountId">Account that this Transaction should be posted in</param>
    /// <param name="date">Posting Date for the Transaction in the provided Account</param>
    public void Run(Transaction transaction, AccountId accountId, DateOnly date)
    {
        if (!ValidatePostingDate(transaction, date, out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateAccount(transaction, accountId, out exception))
        {
            throw exception;
        }
        TransactionBalanceEventPartType partType = accountId == transaction.DebitAccountId
            ? TransactionBalanceEventPartType.PostedDebit
            : TransactionBalanceEventPartType.PostedCredit;
        TransactionBalanceEvent? existingBalanceEvent = transaction.TransactionBalanceEvents.SingleOrDefault(balanceEvent => balanceEvent.EventDate == date);
        if (existingBalanceEvent != null)
        {
            existingBalanceEvent.AddPart(new TransactionBalanceEventPart(existingBalanceEvent, partType));
        }
        else
        {
            if (!transactionBalanceEventFactory.TryCreate(new CreateTransactionBalanceEventRequest
            {
                AccountingPeriodId = transaction.AccountingPeriodId,
                EventDate = date,
                Transaction = transaction,
                Parts = [partType]
            }, out TransactionBalanceEvent? createdBalanceEvent, out IEnumerable<Exception> exceptions))
            {
                throw exceptions.First();
            }
            transaction.AddBalanceEvent(createdBalanceEvent);
        }
    }

    /// <summary>
    /// Validates the posting date for this Transaction
    /// </summary>
    /// <param name="transaction">Transaction to post</param>
    /// <param name="date">Posting date</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the posting date for this Transaction is valid, false otherwise</returns>
    private static bool ValidatePostingDate(Transaction transaction, DateOnly date, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (transaction.Date > date)
        {
            exception = new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates the posting Account for this Transaction
    /// </summary>
    /// <param name="transaction">Transaction to post</param>
    /// <param name="accountId">Account that this Transaction should be posted in</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the posting Account for this Transaction is valid, false otherwise</returns>
    private static bool ValidateAccount(Transaction transaction, AccountId accountId, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (accountId != transaction.DebitAccountId && accountId != transaction.CreditAccountId)
        {
            exception = new InvalidOperationException();
        }
        return exception == null;
    }
}