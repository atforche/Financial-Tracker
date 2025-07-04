using Domain.AccountingPeriods;
using Tests.AddAccountingPeriod.Scenarios;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different <see cref="YearAndMonthScenarios"/>
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
        var setup = new ScenarioSetup();
        if (!YearAndMonthScenarios.IsValid(year, month))
        {
            Assert.Throws<InvalidOperationException>(() => setup.GetService<AccountingPeriodFactory>().Create(year, month));
            return;
        }
        AccountingPeriod accountingPeriod = setup.GetService<AccountingPeriodFactory>().Create(year, month);
        new AccountingPeriodValidator().Validate(accountingPeriod, new AccountingPeriodState
        {
            Year = year,
            Month = month,
            IsOpen = true,
        });
    }
}