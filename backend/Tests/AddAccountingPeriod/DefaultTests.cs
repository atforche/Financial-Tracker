using Domain.AccountingPeriods;
using Tests.Builders;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding a basic Accounting Period
/// </summary>
public class DefaultTests : TestClass
{
    /// <summary>
    /// Runs the default test for adding an Accounting Period
    /// </summary>
    [Fact]
    public void RunTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        new AccountingPeriodValidator().Validate(accountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 1,
                IsOpen = true
            });
    }
}