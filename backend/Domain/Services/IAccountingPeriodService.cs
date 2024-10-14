using Domain.Entities;

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
    /// <param name="newAccountingPeriod">The newly created Accounting Period</param>
    /// <param name="newAccountStartingBalances">The newly created Account Starting Balances for this Accounting Period</param>
    void CreateNewAccountingPeriod(int year, int month,
        out AccountingPeriod newAccountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances);

    /// <summary>
    /// Closes out the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to be closed</param>
    /// <param name="newAccountStartingBalances">The newly created Account Starting Balances for the closed Accounting Period</param>
    void ClosePeriod(
        AccountingPeriod accountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances);
}