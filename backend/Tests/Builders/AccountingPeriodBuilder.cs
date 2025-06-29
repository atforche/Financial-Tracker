using Domain.AccountingPeriods;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs an Accounting Period
/// </summary>
public sealed class AccountingPeriodBuilder(
    AccountingPeriodFactory accountingPeriodFactory,
    IAccountingPeriodRepository accountingPeriodRepository,
    TestUnitOfWork testUnitOfWork)
{
    private int _year = 2025;
    private int _month = 1;

    /// <summary>
    /// Builds the specified Accounting Period
    /// </summary>
    /// <returns>The newly constructed Accounting Period</returns>
    public AccountingPeriod Build()
    {
        AccountingPeriod accountingPeriod = accountingPeriodFactory.Create(_year, _month);
        accountingPeriodRepository.Add(accountingPeriod);
        testUnitOfWork.SaveChanges();
        return accountingPeriod;
    }

    /// <summary>
    /// Sets the Year for this Accounting Period Builder
    /// </summary>
    public AccountingPeriodBuilder WithYear(int year)
    {
        _year = year;
        return this;
    }

    /// <summary>
    /// Sets the Month for this Accounting Period Builder
    /// </summary>
    public AccountingPeriodBuilder WithMonth(int month)
    {
        _month = month;
        return this;
    }
}