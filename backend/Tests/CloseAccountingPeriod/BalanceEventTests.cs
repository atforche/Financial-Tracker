using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Tests.CloseAccountingPeriod.Scenarios;
using Tests.CloseAccountingPeriod.Setups;
using Tests.Validators;

namespace Tests.CloseAccountingPeriod;

/// <summary>
/// Test class that tests closing an Accounting Period with different <see cref="BalanceEventScenarios"/>
/// </summary>
public class BalanceEventTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(BalanceEventScenarios))]
    public void RunTest(BalanceEventScenario scenario)
    {
        var setup = new BalanceEventScenarioSetup(scenario);
        if (!BalanceEventScenarios.IsValid(scenario))
        {
            Assert.Throws<InvalidOperationException>(() => CloseAccountingPeriod(setup));
            return;
        }
        CloseAccountingPeriod(setup);
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod, GetExpectedState(scenario, setup));
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

    /// <summary>
    /// Closes the Accounting Period for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    private static void CloseAccountingPeriod(BalanceEventScenarioSetup setup) =>
        setup.GetService<CloseAccountingPeriodAction>().Run(setup.AccountingPeriod);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(BalanceEventScenario scenario, BalanceEventScenarioSetup setup) =>
        new()
        {
            Year = setup.AccountingPeriod.Year,
            Month = setup.AccountingPeriod.Month,
            IsOpen = false,
            Transactions = GetExpectedTransactionStates(scenario, setup)
        };

    /// <summary>
    /// Gets the expected Transaction states for this test case
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Transaction states for this test case</returns>
    private static List<TransactionState> GetExpectedTransactionStates(BalanceEventScenario scenario, BalanceEventScenarioSetup setup)
    {
        if (scenario is not BalanceEventScenario.UnpostedTransaction and not BalanceEventScenario.PostedTransaction)
        {
            return [];
        }
        List<TransactionState> expectedTransactionStates = [];
        List<TransactionBalanceEventState> expectedTransactionBalanceEvents = [];

        expectedTransactionBalanceEvents.Add(new TransactionBalanceEventState
        {
            AccountName = setup.Account.Name,
            EventDate = new DateOnly(2025, 1, 15),
            EventSequence = 1,
            TransactionEventType = TransactionBalanceEventType.Added,
            TransactionAccountType = TransactionAccountType.Debit
        });
        if (scenario is BalanceEventScenario.PostedTransaction)
        {
            expectedTransactionBalanceEvents.Add(new TransactionBalanceEventState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 2,
                TransactionEventType = TransactionBalanceEventType.Posted,
                TransactionAccountType = TransactionAccountType.Debit
            });
        }
        expectedTransactionStates.Add(new TransactionState
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 250.00m,
                }
            ],
            TransactionBalanceEvents = expectedTransactionBalanceEvents
        });
        return expectedTransactionStates;
    }
}