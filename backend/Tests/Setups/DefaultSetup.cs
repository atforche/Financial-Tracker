using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups;

/// <summary>
/// Default Setup for a test case
/// </summary>
internal sealed class DefaultSetup : TestCaseSetup
{
    /// <summary>
    /// Fund for the Default Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Accounting Period for the Default Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for the Default Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public DefaultSetup()
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);
        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 2500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
    }
}