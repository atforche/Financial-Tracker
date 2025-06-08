using System.Diagnostics.CodeAnalysis;

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
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="date">Posting Date for the Transaction in the provided Account</param>
    public void Run(Transaction transaction, TransactionAccountType accountType, DateOnly date)
    {
        if (!ValidatePostingDate(transaction, date, out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateAccount(transaction, accountType, out exception))
        {
            throw exception;
        }
        TransactionBalanceEventPartType partType = accountType == TransactionAccountType.Debit
            ? TransactionBalanceEventPartType.PostedDebit
            : TransactionBalanceEventPartType.PostedCredit;
        TransactionBalanceEvent? existingBalanceEvent = transaction.TransactionBalanceEvents.SingleOrDefault(balanceEvent => balanceEvent.EventDate == date);
        if (existingBalanceEvent != null)
        {
            existingBalanceEvent.AddPart(new TransactionBalanceEventPart(existingBalanceEvent, partType));
        }
        else
        {
            transaction.AddBalanceEvent(transactionBalanceEventFactory.Create(new CreateTransactionBalanceEventRequest
            {
                AccountingPeriodId = transaction.AccountingPeriodId,
                EventDate = date,
                Transaction = transaction,
                Parts = [partType]
            }));
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
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the posting Account for this Transaction is valid, false otherwise</returns>
    private static bool ValidateAccount(Transaction transaction, TransactionAccountType accountType, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (accountType == TransactionAccountType.Debit && transaction.DebitAccountId == null)
        {
            exception = new InvalidOperationException();
        }
        if (accountType == TransactionAccountType.Credit && transaction.CreditAccountId == null)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
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