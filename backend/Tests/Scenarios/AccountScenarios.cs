using System.Collections;
using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Setups;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Account scenarios that should be tested
/// </summary>
public sealed class AccountScenarios : IEnumerable<TheoryDataRow<AccountType>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountType>> GetEnumerator() => Enum.GetValues<AccountType>()
        .Select(accountType => new TheoryDataRow<AccountType>(accountType)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Setup class for an Account scenario
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
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("OtherTest");
        fundRepository.Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        Account = GetService<AddAccountAction>().Run("Test", accountType, AccountingPeriod, AccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = 1500.00m
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
    }
}