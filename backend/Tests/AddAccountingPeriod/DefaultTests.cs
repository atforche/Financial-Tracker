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
                Year = setup.AccountingPeriod.Year,
                Month = setup.AccountingPeriod.Month,
                IsOpen = true,
                Transactions = []
            });
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints,
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = setup.Account.Name,
                    AccountingPeriodYear = setup.AccountingPeriod.Year,
                    AccountingPeriodMonth = setup.AccountingPeriod.Month,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = setup.Fund.Name,
                            Amount = 1500.00m
                        },
                        new FundAmountState
                        {
                            FundName = setup.OtherFund.Name,
                            Amount = 1500.00m
                        },
                    ]
                }
            ]);
    }
}