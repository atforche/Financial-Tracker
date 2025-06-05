using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Mocks;
using Tests.Setups;
using Tests.Validators;

namespace Tests.GetAccountBalanceByAccountingPeriodTests;

/// <summary>
/// Test class that tests getting the Account Balance by Accounting Period with a default scenario
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
        Transaction transaction = setup.GetService<TransactionFactory>().Create(setup.AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            setup.Account.Id,
            null,
            [
                new FundAmount
                {
                    FundId = setup.Fund.Id,
                    Amount = 250.00m
                }
            ]);
        setup.GetService<ITransactionRepository>().Add(transaction);
        setup.GetService<TestUnitOfWork>().SaveChanges();

        new AccountBalanceByAccountingPeriodValidator().Validate(
            setup.GetService<AccountBalanceService>().GetAccountBalanceByAccountingPeriod(setup.Account.Id, setup.AccountingPeriod.Id),
            new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                StartingFundBalances = [],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = 1500.00m,
                    },
                    new FundAmountState
                    {
                        FundId = setup.OtherFund.Id,
                        Amount = 1500.00m,
                    }
                ],
                EndingPendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundId = setup.Fund.Id,
                        Amount = -250.00m,
                    }
                ]
            });
    }
}