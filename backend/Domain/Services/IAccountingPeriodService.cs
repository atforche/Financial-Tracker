using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
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
    /// Closes out the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to be closed</param>
    /// <param name="nextAccountingPeriod">The next Accounting Period after the one being closed</param>
    void ClosePeriod(AccountingPeriod accountingPeriod, AccountingPeriod? nextAccountingPeriod);

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="accountingPeriod">Parent Accounting Period for this Transaction</param>
    /// <param name="transactionDate">Date for the Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for the Transaction</param>
    /// <param name="debitAccountDetail">Debit Account Details for the Transaction</param>
    /// <param name="creditAccountDetail">Credit Account Details for the Transaction</param>
    /// <returns>The newly created Transaction</returns>
    Transaction CreateNewTransaction(AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        IEnumerable<FundAmount> accountingEntries,
        TransactionAccountDetail? debitAccountDetail,
        TransactionAccountDetail? creditAccountDetail);
}

/// <summary>
/// Record representing the details about an Account involved in a Transaction
/// </summary>
public record TransactionAccountDetail
{
    /// <summary>
    /// Account that this Transaction affects
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Posted Statement Date of this Transaction in this Account
    /// </summary>
    public DateOnly? PostedStatementDate { get; init; }
}