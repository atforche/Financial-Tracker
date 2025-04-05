using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Setups;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Tests adding a Transaction with different Accounting Period setups
/// </summary>
public class AccountingPeriodTest
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Theory]
    [MemberData(nameof(AccountingPeriodSetup.GetCollection), MemberType = typeof(AccountingPeriodSetup))]
    public void RunTest(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        var setup = new AccountingPeriodSetup(pastPeriodStatus, currentPeriodStatus, futurePeriodStatus);
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
    /// <param name="setup">Accounting Period Setup for this test case</param>
    /// <returns>True if this test case should throw an exception, false otherwise</returns>
    private static bool ShouldThrowException(AccountingPeriodSetup setup) => !setup.CurrentAccountingPeriod.IsOpen;

    /// <summary>
    /// Adds the Transaction for this test case
    /// </summary>
    /// <param name="setup">Accounting Period Setup for this test case</param>
    /// <returns>The transaction that was added for this test case</returns>
    private static Transaction AddTransaction(AccountingPeriodSetup setup) =>
        setup.GetService<IAccountingPeriodService>().AddTransaction(setup.CurrentAccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            null,
            [
                new FundAmount()
                {
                    Fund = setup.Fund,
                    Amount = 25.00m,
                }
            ]);

    /// <summary>
    /// Gets the expected state for this test case
    /// </summary>
    /// <param name="setup">Accounting Period Setup for this test case</param>
    /// <returns>The expected state for this test case</returns>
    private static TransactionState GetExpectedState(AccountingPeriodSetup setup) =>
        new()
        {
            TransactionDate = new DateOnly(2025, 1, 15),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = setup.Fund.Name,
                    Amount = 25.00m,
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountName = setup.Account.Name,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        };
}