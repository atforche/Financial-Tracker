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
    /// <param name="debitFundAmounts">Debit Fund Amounts for the Transaction</param>
    /// <param name="creditAccountId">Credit Account ID for the Transaction</param>
    /// <param name="creditFundAmounts">Credit Fund Amounts for the Transaction</param>
    /// <returns>The newly created Transaction</returns>
    public Transaction Create(
        AccountingPeriodId accountingPeriodId,
        DateOnly date,
        AccountId? debitAccountId,
        ICollection<FundAmount>? debitFundAmounts,
        AccountId? creditAccountId,
        ICollection<FundAmount>? creditFundAmounts)
    {
        if (!ValidateIndividualFundAmounts(debitAccountId, debitFundAmounts?.ToList(), out Exception? exception))
        {
            throw exception;
        }
        if (!ValidateIndividualFundAmounts(creditAccountId, creditFundAmounts?.ToList(), out exception))
        {
            throw exception;
        }
        if (!ValidateAllFundAmounts(debitFundAmounts?.ToList(), creditFundAmounts?.ToList(), out exception))
        {
            throw exception;
        }
        if (!ValidateAccounts(debitAccountId, creditAccountId, out exception))
        {
            throw exception;
        }
        var transaction = new Transaction(accountingPeriodId, date, debitAccountId, debitFundAmounts, creditAccountId, creditFundAmounts);

        List<TransactionBalanceEventPartType> balanceEventParts = [];
        if (debitAccountId != null)
        {
            balanceEventParts.Add(TransactionBalanceEventPartType.AddedDebit);
        }
        if (creditAccountId != null)
        {
            balanceEventParts.Add(TransactionBalanceEventPartType.AddedCredit);
        }
        transaction.AddBalanceEvent(transactionBalanceEventFactory.Create(new CreateTransactionBalanceEventRequest
        {
            AccountingPeriodId = transaction.AccountingPeriodId,
            EventDate = transaction.Date,
            Transaction = transaction,
            Parts = balanceEventParts
        }));
        return transaction;
    }

    /// <summary>
    /// Validates the individual Fund Amounts for this Transaction
    /// </summary>
    /// <param name="accountId">Account ID for the Transaction</param>
    /// <param name="fundAmounts">Fund Amounts for the Transaction</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Fund Amounts for this Transaction are valid, false otherwise</returns>
    private static bool ValidateIndividualFundAmounts(
        AccountId? accountId,
        List<FundAmount>? fundAmounts,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (accountId == null && fundAmounts != null)
        {
            // Cannot have Fund Amounts without an Account
            exception = new InvalidOperationException();
        }
        if (accountId != null && fundAmounts == null)
        {
            // Cannot have an Account without Fund Amounts
            exception ??= new InvalidOperationException();
        }
        if (fundAmounts == null)
        {
            // The following checks are only relevant if Fund Amounts are provided
            return exception == null;
        }

        if (fundAmounts.Count == 0)
        {
            // No Fund Amounts were provided
            exception = new InvalidOperationException();
        }
        if (fundAmounts.Any(entry => entry.Amount <= 0))
        {
            // A blank or negative Fund Amount was provided
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
    /// Validates both the Debit and Credit Fund Amounts
    /// </summary>
    /// <param name="debitFundAmounts">Debit Fund Amounts for the Transaction</param>
    /// <param name="creditFundAmounts">Credit Fund Amounts for the Transaction</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if both the Debit and Credit Fund Amounts are valid, false otherwise</returns>
    private static bool ValidateAllFundAmounts(
        List<FundAmount>? debitFundAmounts,
        List<FundAmount>? creditFundAmounts,
        [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (debitFundAmounts == null && creditFundAmounts == null)
        {
            // At least one set of Fund Amounts must be provided
            exception = new InvalidOperationException();
        }
        if (debitFundAmounts == null || creditFundAmounts == null)
        {
            // The following checks are only relevant if both sets of Fund Amounts are provided
            return exception == null;
        }
        if (debitFundAmounts.Sum(fundAmount => fundAmount.Amount) != creditFundAmounts.Sum(fundAmount => fundAmount.Amount))
        {
            // Both sets of Fund Amounts must sum to the same amount
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