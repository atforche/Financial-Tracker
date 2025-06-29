using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Builders;
using Tests.Mocks;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests that adding an Accounting Period creates balance checkpoints as expected
/// </summary>
public class BalanceCheckpointTests : TestClass
{
    /// <summary>
    /// Runs the test for adding an Accounting Period when the previous Accounting Period is still open
    /// </summary>
    [Fact]
    public void RunOpenPreviousPeriodTest()
    {
        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>()
            .WithAddedFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 1500.00m
                    }
                ])
            .Build();

        GetService<AccountingPeriodBuilder>()
            .WithMonth(2)
            .Build();
        new AccountValidator().Validate(account,
            new AccountState
            {
                Name = account.Name,
                Type = AccountType.Standard,
                AccountAddedBalanceEvent = new AccountAddedBalanceEventState
                {
                    AccountingPeriodId = firstAccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 1),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundAmounts =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 1500.00m
                        }
                    ]
                },
                AccountBalanceCheckpoints = []
            });
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period when the previous Accounting Period is closed
    /// </summary>
    [Fact]
    public void RunClosedPreviousPeriodTest()
    {
        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>()
            .WithAddedFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 1500.00m
                    }
                ])
            .Build();
        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod secondAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithMonth(2)
            .Build();
        new AccountValidator().Validate(account,
            new AccountState
            {
                Name = account.Name,
                Type = AccountType.Standard,
                AccountAddedBalanceEvent = new AccountAddedBalanceEventState
                {
                    AccountingPeriodId = firstAccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 1),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundAmounts =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 1500.00m
                        }
                    ]
                },
                AccountBalanceCheckpoints =
                [
                    new AccountBalanceCheckpointState
                    {
                        AccountId = account.Id,
                        AccountingPeriodId = secondAccountingPeriod.Id,
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundId = fund.Id,
                                Amount = 1500.00m
                            }
                        ]
                    }
                ]
            });
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period that will create a balance checkpoint with a balance of zero
    /// </summary>
    [Fact]
    public void RunZeroBalanceTest()
    {
        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Account account = GetService<AccountBuilder>().Build();
        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod secondAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithMonth(2)
            .Build();
        new AccountValidator().Validate(account,
            new AccountState
            {
                Name = account.Name,
                Type = AccountType.Standard,
                AccountAddedBalanceEvent = new AccountAddedBalanceEventState
                {
                    AccountingPeriodId = firstAccountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 1),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundAmounts = []
                },
                AccountBalanceCheckpoints =
                [
                    new AccountBalanceCheckpointState
                    {
                        AccountId = account.Id,
                        AccountingPeriodId = secondAccountingPeriod.Id,
                        FundBalances = []
                    }
                ]
            });
    }
}