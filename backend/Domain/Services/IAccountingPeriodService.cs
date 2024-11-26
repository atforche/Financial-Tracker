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
    void ClosePeriod(AccountingPeriod accountingPeriod);
}