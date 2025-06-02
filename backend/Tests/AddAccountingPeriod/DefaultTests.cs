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
        using var setup = new DefaultScenarioSetup();
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod,
            new AccountingPeriodState
            {
                Year = setup.AccountingPeriod.Year,
                Month = setup.AccountingPeriod.Month,
                IsOpen = true,
            });
        new AccountValidator().Validate(setup.Account,
            new AccountState
            {
                Name = setup.Account.Name,
                Type = setup.Account.Type,
                AccountAddedBalanceEvent = new AccountAddedBalanceEventState
                {
                    AccountingPeriodId = setup.AccountingPeriod.Id,
                    EventDate = setup.AccountingPeriod.PeriodStartDate,
                    EventSequence = 1,
                    AccountName = setup.Account.Name,
                    FundAmounts =
                    [
                        new FundAmountState
                        {
                            FundId = setup.Fund.Id,
                            Amount = 1500.00m,
                        },
                        new FundAmountState
                        {
                            FundId = setup.OtherFund.Id,
                            Amount = 1500.00m
                        }
                    ]
                },
                AccountBalanceCheckpoints = []
            });
    }
}