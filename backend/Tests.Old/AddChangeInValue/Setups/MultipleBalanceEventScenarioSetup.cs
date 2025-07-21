using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;
using Domain.Funds;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;
using Tests.Old.Setups;

namespace Tests.Old.AddChangeInValue.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventMultipleBalanceEventScenarios"/> for adding a Change In Value
/// </summary>
internal sealed class MultipleBalanceEventScenarioSetup : ScenarioSetup
{
    private readonly AccountingPeriod _futureAccountingPeriod;

    /// <summary>
    /// Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for the Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public MultipleBalanceEventScenarioSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        Fund = GetService<FundFactory>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("Test2", "");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        _futureAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(_futureAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    FundId = OtherFund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        DoScenarioSpecificSetup(scenario);
    }

    /// <summary>
    /// Performs scenario specific setup for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    private void DoScenarioSpecificSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFundBalanceNegative)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 10),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = -1250.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceToZero)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 10),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = -2500.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 10),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = -2750.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 20),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = -2750.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative)
        {
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 10),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = -2750.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
            GetService<IChangeInValueRepository>().Add(GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = _futureAccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 10),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 2750.00m
                }
            }));
            GetService<TestUnitOfWork>().SaveChanges();
        }
    }
}