using System.Diagnostics.CodeAnalysis;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Transaction
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
/// <param name="accountBalanceService">Account Balance Service</param>
public class AddTransactionAction(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountBalanceService accountBalanceService)
    : AddBalanceEventAction(accountingPeriodRepository, accountBalanceService)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Transaction</param>
    /// <param name="transactionDate">Transaction Date for the Transaction</param>
    /// <param name="debitAccount">Debit Account for the Transaction</param>
    /// <param name="creditAccount">Credit Account for the Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for the Transaction</param>
    /// <returns>The newly created Transaction</returns>
    public Transaction Run(
        AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries)
    {
        if (!IsValid(accountingPeriod,
                transactionDate,
                debitAccount,
                creditAccount,
                accountingEntries.ToList(),
                out Exception? exception))
        {
            throw exception;
        }
        var transaction = new Transaction(accountingPeriod, transactionDate, debitAccount, creditAccount, accountingEntries);
        foreach (TransactionBalanceEvent balanceEvent in transaction.TransactionBalanceEvents)
        {
            if (!ValidateFutureBalanceEvents(balanceEvent, accountingPeriod, out exception))
            {
                throw exception;
            }
        }
        accountingPeriod.AddTransaction(transaction);
        return transaction;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Transaction</param>
    /// <param name="transactionDate">Transaction Date for the Transaction</param>
    /// <param name="debitAccount">Debit Account for the Transaction</param>
    /// <param name="creditAccount">Credit Account for the Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for the Transaction</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private static bool IsValid(
        AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        List<FundAmount> accountingEntries,
        [NotNullWhen(false)] out Exception? exception)
    {
        if (!IsValid(accountingPeriod, transactionDate, out exception))
        {
            return false;
        }
        if (transactionDate == DateOnly.MinValue)
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that an Accounting Entry was provided
        if (accountingEntries.Count == 0)
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that no blank Accounting Entries were provided
        if (accountingEntries.Any(entry => entry.Amount <= 0))
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that we don't have multiple Accounting Entries for the same Fund
        if (accountingEntries.GroupBy(entry => entry.Fund.Id).Any(group => group.Count() > 1))
        {
            exception ??= new InvalidOperationException();
        }
        // Validate that different debit and credit Accounts were provided (and both weren't null)
        if (debitAccount == creditAccount)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}