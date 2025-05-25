using Domain.Actions;
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
        using var setup = new DefaultScenarioSetup();
        setup.GetService<CloseAccountingPeriodAction>().Run(setup.AccountingPeriod);
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod,
            new AccountingPeriodState
            {
                Year = setup.AccountingPeriod.Year,
                Month = setup.AccountingPeriod.Month,
                IsOpen = false,
                Transactions = [],
                FundConversions = [],
                ChangeInValues = [],
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