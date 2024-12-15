using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Accounting Periods
/// </summary>
public interface IAccountingPeriodService
{
    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    /// <returns>The newly created Accounting Period</returns>
    AccountingPeriod CreateNewAccountingPeriod(int year, int month);

    /// <summary>
    /// Adds a Transaction to the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to add a Transaction to</param>
    /// <param name="transactionDate">Transaction Date for this Transaction</param>
    /// <param name="debitAccount">Debit Account for this Transaction</param>
    /// <param name="creditAccount">Credit Account for this Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <returns>The newly created Transaction</returns>
    Transaction AddTransaction(
        AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries);

    /// <summary>
    /// Posts the provided Transaction
    /// </summary>
    /// <param name="transaction">Transaction to be posted</param>
    /// <param name="account">Account that this Tranaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    void PostTransaction(
        Transaction transaction,
        Account account,
        DateOnly postedStatementDate);

    /// <summary>
    /// Adds a Fund Conversion to the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to add a Fund Conversion to</param>
    /// <param name="eventDate">Date for this Fund Conversion</param>
    /// <param name="account">Account for this Fund Conversion</param>
    /// <param name="fromFund">From Fund for this Fund Conversion</param>
    /// <param name="toFund">To Fund for this Fund Conversion</param>
    /// <param name="amount">Amount for this Fund Conversion</param>
    /// <returns>The newly created Fund Conversion</returns>
    FundConversion AddFundConversion(
        AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount);

    /// <summary>
    /// Closes out the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to be closed</param>
    void ClosePeriod(AccountingPeriod accountingPeriod);
}