using Domain.AccountingPeriods;
using Domain.Transactions;
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
        new AccountingPeriodValidator().Validate(setup.AccountingPeriod, GetExpectedState(setup));
        new ChangeInValueValidator().Validate(setup.ChangeInValue != null ? [setup.ChangeInValue] : [], GetExpectedChangeInValueStates(scenario, setup));
        new FundConversionValidator().Validate(setup.FundConversion != null ? [setup.FundConversion] : [], GetExpectedFundConversionStates(scenario, setup));
        new TransactionValidator().Validate(setup.Transaction != null ? [setup.Transaction] : [], GetExpectedTransactionStates(scenario, setup));
        new AccountBalanceCheckpointValidator().Validate(setup.Account.AccountBalanceCheckpoints, []);
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
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static AccountingPeriodState GetExpectedState(BalanceEventScenarioSetup setup) =>
        new()
        {
            Year = setup.AccountingPeriod.Year,
            Month = setup.AccountingPeriod.Month,
            IsOpen = false,
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
        List<TransactionBalanceEventPartType> expectedTransactionBalanceEventParts = [];
        expectedTransactionBalanceEventParts.Add(TransactionBalanceEventPartType.AddedDebit);
        if (scenario is BalanceEventScenario.PostedTransaction)
        {
            expectedTransactionBalanceEventParts.Add(TransactionBalanceEventPartType.PostedDebit);
        }
        return
        [
            new TransactionState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = setup.Account.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = setup.AccountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts = expectedTransactionBalanceEventParts
                    }
                ]
            }
        ];
    }

    /// <summary>
    /// Gets the expected Fund Conversion states for this test case
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Fund Conversion states for this test case</returns>
    private static List<FundConversionState> GetExpectedFundConversionStates(BalanceEventScenario scenario, BalanceEventScenarioSetup setup)
    {
        if (scenario is not BalanceEventScenario.FundConversion)
        {
            return [];
        }
        return
        [
            new FundConversionState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountId = setup.Account.Id,
                FromFundId = setup.Fund.Id,
                ToFundId = setup.OtherFund.Id,
                Amount = 250.00m
            }
        ];
    }

    /// <summary>
    /// Gets the expected Change In Value states for this test case
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <param name="setup">Setup for this test case</param>
    /// <returns>The expected Change In Value states for this test case</returns>
    private static List<ChangeInValueState> GetExpectedChangeInValueStates(BalanceEventScenario scenario, BalanceEventScenarioSetup setup)
    {
        if (scenario is not BalanceEventScenario.ChangeInValue)
        {
            return [];
        }
        return
        [
            new ChangeInValueState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountId = setup.Account.Id,
                FundAmount = new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m
                }
            }
        ];
    }
}