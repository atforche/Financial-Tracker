using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups;

/// <summary>
/// Setup class for an Account test case
/// </summary>
internal sealed class AccountSetup : TestCaseSetup
{
    /// <summary>
    /// Fund for this Account Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Accounting Period for this Account Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for this Account Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountType">Account Type for this Account Setup</param>
    public AccountSetup(AccountType accountType)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);
        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        Account = GetService<IAccountService>().CreateNewAccount("Test", accountType,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 2500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
    }

    /// <summary>
    /// Gets the collection of Account scenarios
    /// </summary>
    public static IEnumerable<TheoryDataRow<AccountType>> GetCollection() => Enum.GetValues<AccountType>()
        .Select(accountType => new TheoryDataRow<AccountType>(accountType));
}