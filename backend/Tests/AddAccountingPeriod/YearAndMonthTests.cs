using Tests.AddAccountingPeriod.Scenarios;
using Tests.AddAccountingPeriod.Setups;
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
        if (!YearAndMonthScenarios.IsValid(year, month))
        {
            Assert.Throws<InvalidOperationException>(() => new YearAndMonthScenarioSetup(year, month));
            return;
        }
        using var setup = new YearAndMonthScenarioSetup(year, month);
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod, new AccountingPeriodState
        {
            Year = setup.AccountingPeriod.Year,
            Month = setup.AccountingPeriod.Month,
            IsOpen = true,
        });
    }
}