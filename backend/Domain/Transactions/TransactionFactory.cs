using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Factory for building a <see cref="Transaction"/>
/// </summary>
public class TransactionFactory(TransactionBalanceEventFactory transactionBalanceEventFactory)
{
    /// <summary>
    /// Create a new Transaction
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for the Transaction</param>
    /// <param name="date">Date for the Transaction</param>
    /// <param name="debitAccountId">Debit Account ID for the Transaction</param>
    /// <param name="creditAccountId">Credit Account ID for the Transaction</param>
    /// <param name="fundAmounts">Fund Amounts for the Transaction</param>
    /// <returns>The newly created Transaction</returns>
    public Transaction Create(
        AccountingPeriodId accountingPeriodId,
        DateOnly date,
        AccountId? debitAccountId,
        AccountId? creditAccountId,
        ICollection<FundAmount> fundAmounts)
    {
        if (!ValidateFundAmounts(fundAmounts.ToList(), out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateAccounts(debitAccountId, creditAccountId, out exception))
        {
            throw exception;
        }
        var transaction = new Transaction(accountingPeriodId, date, fundAmounts);
        if (debitAccountId != null)
        {
            transaction.AddBalanceEvent(transactionBalanceEventFactory.Create(new CreateTransactionBalanceEventRequest
            {
                AccountingPeriodId = accountingPeriodId,
                EventDate = date,
                AccountId = debitAccountId,
                Transaction = transaction,
                EventType = TransactionBalanceEventType.Added,
                AccountType = TransactionAccountType.Debit
            }));
        }
        if (creditAccountId != null)
        {
            TransactionBalanceEvent balanceEvent = transactionBalanceEventFactory.Create(new CreateTransactionBalanceEventRequest
            {
                AccountingPeriodId = accountingPeriodId,
                EventDate = date,
                AccountId = creditAccountId,
                Transaction = transaction,
                EventType = TransactionBalanceEventType.Added,
                AccountType = TransactionAccountType.Credit
            });
            if (debitAccountId != null)
            {
                balanceEvent.EventSequence += 1;
            }
            transaction.AddBalanceEvent(balanceEvent);
        }
        return transaction;
    }

    /// <summary>
    /// Validates the Fund Amounts for this Transaction
    /// </summary>
    /// <param name="fundAmounts">Fund Amounts for the Transaction</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Fund Amounts for this Transaction are valid, false otherwise</returns>
    private static bool ValidateFundAmounts(List<FundAmount> fundAmounts, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (fundAmounts.Count == 0)
        {
            // No Fund Amounts were provided
            exception = new InvalidOperationException();
        }
        if (fundAmounts.Any(entry => entry.Amount <= 0))
        {
            // A blank Fund Amount was provided
            exception ??= new InvalidOperationException();
        }
        if (fundAmounts.GroupBy(entry => entry.FundId).Any(group => group.Count() > 1))
        {
            // Multiple Fund Amounts from the same Fund were provided
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates the Accounts for this Transaction
    /// </summary>
    /// <param name="debitAccountId">Debit Account ID for the Transaction</param>
    /// <param name="creditAccountId">Credit Account ID for the Transaction</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Accounts for this Transaction are valid, false otherwise</returns>
    private static bool ValidateAccounts(
        AccountId? debitAccountId,
        AccountId? creditAccountId,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (debitAccountId == creditAccountId)
        {
            // The same Account was provided for both the Debit and Credit Account (or both were null)
            exception = new InvalidOperationException();
        }
        return exception == null;
    }
}