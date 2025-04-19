using Domain.Aggregates.AccountingPeriods;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.AddAccountingPeriod.Setups;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different Year and Month scenarios
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
        if (!YearAndMonthScenarios.IsValid(year, month))
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
}