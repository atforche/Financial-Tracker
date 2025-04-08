using Domain.Aggregates.AccountingPeriods;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different year and month scenarios
/// </summary>
public class YearAndMonthTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(YearAndMonthScenarios))]
    public void RunTest(int year, int month)
    {
        if (ShouldThrowException(year, month))
        {
            Assert.Throws<InvalidOperationException>(() => new YearAndMonthScenarioSetup(year, month));
            return;
        }
        AccountingPeriod accountingPeriod = new YearAndMonthScenarioSetup(year, month).AccountingPeriod;
        new AccountingPeriodValidator().Validate(accountingPeriod, new AccountingPeriodState
        {
            Year = year,
            Month = month,
            IsOpen = true,
            AccountBalanceCheckpoints = [],
            Transactions = []
        });
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="year">Year for this test case</param>
    /// <param name="month">Month for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            return true;
        }
        return year is < 2000 or > 2100;
    }
}