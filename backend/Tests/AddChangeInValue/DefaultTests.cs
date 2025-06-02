using Domain.ChangeInValues;
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
        using var setup = new DefaultScenarioSetup();
        ChangeInValue changeInValue = setup.GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            AccountId = setup.Account.Id,
            FundAmount = new FundAmount
            {
                FundId = setup.Fund.Id,
                Amount = -100.00m,
            }
        });
        new ChangeInValueValidator().Validate(changeInValue,
            new ChangeInValueState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountId = setup.Account.Id,
                FundAmount = new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = -100.00m,
                }
            });
    }
}