using Tests.Setups;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding a Accounting Period with a default scenario
/// </summary>
public class DefaultTests
{
    /// <summary>
    /// Runs the test for this test case
    /// </summary>
    [Fact]
    public void RunTest()
    {
        var setup = new DefaultScenarioSetup();
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod,
            new AccountingPeriodState
            {
                Key = setup.AccountingPeriod.Key,
                IsOpen = true,
                Transactions = [],
                FundConversions = [],
                ChangeInValues = [],
                AccountAddedBalanceEvents =
                [
                    new AccountAddedBalanceEventState
                    {
                        AccountingPeriodKey = setup.AccountingPeriod.Key,
                        AccountName = setup.Account.Name,
                        EventDate = setup.AccountingPeriod.PeriodStartDate,
                        EventSequence = 1,
                        FundAmounts =
                        [
                            new FundAmountState
                            {
                                FundName = setup.Fund.Name,
                                Amount = 1500.00m,
                            },
                            new FundAmountState
                            {
                                FundName = setup.OtherFund.Name,
                                Amount = 1500.00m
                            }
                        ]
                    }
                ]
            });
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, []);
    }
}