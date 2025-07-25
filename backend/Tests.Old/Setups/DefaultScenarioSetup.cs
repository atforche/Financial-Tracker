using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.Mocks;

namespace Tests.Old.Setups;

/// <summary>
/// Setup for a default scenario
/// </summary>
internal sealed class DefaultScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for the Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public DefaultScenarioSetup()
    {
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("OtherTest");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
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
                    Amount = 1500.00m
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();
    }
}