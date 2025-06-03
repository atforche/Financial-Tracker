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
        transaction.AddBalanceEvent(transactionBalanceEventFactory.Create(new CreateTransactionBalanceEventRequest
        {
            AccountingPeriodId = transaction.AccountingPeriodId,
            EventDate = date,
            AccountId = transaction.GetAccountId(accountType) ?? throw new InvalidOperationException(),
            Transaction = transaction,
            EventType = TransactionBalanceEventType.Posted,
            AccountType = accountType
        }));
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

        if (transaction.GetAccountId(accountType) == null)
        {
            exception = new InvalidOperationException();
        }
        return exception == null;
    }
}