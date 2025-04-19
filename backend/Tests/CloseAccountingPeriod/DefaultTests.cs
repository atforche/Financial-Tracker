using Domain.Services;
using Tests.Setups;
using Tests.Validators;

namespace Tests.CloseAccountingPeriod;

/// <summary>
/// Test class that tests closing an Accounting Period with a default scenario
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
        setup.GetService<IAccountingPeriodService>().ClosePeriod(setup.AccountingPeriod);
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod,
            new AccountingPeriodState
            {
                Year = setup.AccountingPeriod.Year,
                Month = setup.AccountingPeriod.Month,
                IsOpen = false,
                AccountBalanceCheckpoints =
                [
                    new AccountBalanceCheckpointState
                    {
                        AccountName = setup.Account.Name,
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
                            }
                        ]
                    }
                ],
                Transactions = []
            });
    }
}