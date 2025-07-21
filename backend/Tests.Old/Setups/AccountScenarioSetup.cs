using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;

namespace Tests.Old.Setups;

/// <summary>
/// Setup class for an <see cref="AccountScenarios"/>
/// </summary>
internal sealed class AccountScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for this Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for this Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountType">Account Type for this Setup</param>
    public AccountScenarioSetup(AccountType accountType)
    {
        Fund = GetService<FundFactory>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("OtherTest", "");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", accountType, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
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