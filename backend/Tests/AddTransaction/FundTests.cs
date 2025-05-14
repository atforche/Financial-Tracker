using Domain.AccountingPeriods;
using Domain.Actions;
using Domain.Funds;
using Tests.AddTransaction.Scenarios;
using Tests.AddTransaction.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="FundScenarios"/>
/// </summary>
public class FundTest
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(FundScenarios))]
    public void RunTest(FundScenario scenario)
    {
        var setup = new FundScenarioSetup(scenario);
        if (ShouldThrowException(setup))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup), GetExpectedState(setup));
    }

    /// <summary>
    /// Determines if this test case should throw an exception
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(FundScenarioSetup setup)
    {
        if (setup.Funds.Count == 0)
        {
            return true;
        }
        if (setup.Funds.GroupBy(fund => fund.Id).Any(group => group.Count() > 1))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(FundScenarioSetup setup) =>
        setup.GetService<AddTransactionAction>().Run(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            setup.Funds.Select(fund => new FundAmount
            {
                Fund = fund,
                Amount = 25.00m
            }));

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(FundScenarioSetup setup) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries = setup.Funds.Select(fund => new FundAmountState
            {
                FundName = fund.Name,
                Amount = 25.00m
            }).ToList(),
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountingPeriodKey = setup.AccountingPeriod.Key,
                    AccountName = setup.Account.Name,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        };
}