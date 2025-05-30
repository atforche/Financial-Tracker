using Domain.AccountingPeriods;
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
        AccountingPeriod accountingPeriod = new YearAndMonthScenarioSetup(year, month).AccountingPeriod;
        new AccountingPeriodValidator().Validate(accountingPeriod, new AccountingPeriodState
        {
            Key = accountingPeriod.Key,
            IsOpen = true,
            Transactions = [],
            FundConversions = [],
            ChangeInValues = [],
            AccountAddedBalanceEvents = []
        });
    }
}