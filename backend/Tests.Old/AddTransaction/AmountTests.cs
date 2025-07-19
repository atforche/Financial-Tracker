using Domain.Funds;
using Domain.Transactions;
using Tests.Old.Scenarios;
using Tests.Old.Setups;
using Tests.Old.Validators;

namespace Tests.Old.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different <see cref="AddBalanceEventAmountScenarios"/>
/// </summary>
public class AmountTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [ClassData(typeof(AddBalanceEventAmountScenarios))]
    public void RunTest(decimal amount)
    {
        var setup = new AddBalanceEventAmountScenarioSetup();
        if (!IsValid(amount))
        {
            Assert.Throws<InvalidOperationException>(() => AddTransaction(setup, amount));
            return;
        }
        new TransactionValidator().Validate(AddTransaction(setup, amount), GetExpectedState(setup, amount));
    }

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    private static bool IsValid(decimal amount) => amount > 0;

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The Transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.Account.Id,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 100.00m,
                },
                new FundAmount
                {
                    FundId = setup.OtherFund.Id,
                    Amount = amount,
                }
            ],
            null,
            null);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Setup for this test case</param>
    /// <param name="amount">Amount for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AddBalanceEventAmountScenarioSetup setup, decimal amount) =>
        new()
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            Date = new DateOnly(2025, 1, 15),
            DebitAccountId = setup.Account.Id,
            DebitFundAmounts =
            [
                new FundAmountState
                {
                    FundId = setup.Fund.Id,
                    Amount = 100.00m,
                },
                new FundAmountState
                {
                    FundId = setup.OtherFund.Id,
                    Amount = amount,
                },
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountingPeriodId = setup.AccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    Parts =
                    [
                        TransactionBalanceEventPartType.AddedDebit
                    ]
                }
            ]
        };
}