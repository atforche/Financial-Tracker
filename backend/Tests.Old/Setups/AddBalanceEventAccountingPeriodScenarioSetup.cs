using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;

namespace Tests.Old.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventAccountingPeriodScenarios"/> for adding a Balance Event
/// </summary>
internal sealed class AddBalanceEventAccountingPeriodScenarioSetup : ScenarioSetup
{
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
    public AddBalanceEventAccountingPeriodScenarioSetup(AddBalanceEventAccountingPeriodScenario scenario)
    {
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("Test2");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod nextAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(nextAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test",
            AccountType.Standard,
            scenario == AddBalanceEventAccountingPeriodScenario.PriorToAccountBeingAdded ? nextAccountingPeriod.Id : AccountingPeriod.Id,
            AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        if (scenario == AddBalanceEventAccountingPeriodScenario.Closed)
        {
            GetService<CloseAccountingPeriodAction>().Run(AccountingPeriod);
            GetService<TestUnitOfWork>().SaveChanges();
        }
    }
}