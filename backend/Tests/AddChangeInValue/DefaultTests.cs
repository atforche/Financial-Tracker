using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddChangeInValue;

/// <summary>
/// Test class that tests adding a Change In Value with a default scenario
/// </summary>
public class DefaultTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Fact]
    public void RunTest()
    {
        var setup = new DefaultScenarioSetup();
        ChangeInValue changeInValue = setup.GetService<AddChangeInValueAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            new FundAmount
            {
                Fund = setup.Fund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountingPeriodKey = setup.AccountingPeriod.Key,
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountingEntry = new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = -100.00m,
                }
            });
    }
}