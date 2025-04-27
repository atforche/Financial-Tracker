using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups;

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
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("OtherTest");
        fundRepository.Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
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