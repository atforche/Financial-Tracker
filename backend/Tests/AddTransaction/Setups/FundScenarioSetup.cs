using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Tests.AddTransaction.Scenarios;
using Tests.Setups;

namespace Tests.AddTransaction.Setups;

/// <summary>
/// Setup class for a <see cref="FundScenarios"/> for adding a Transaction
/// </summary>
internal sealed class FundScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for this Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Funds for this Setup
    /// </summary>
    public List<Fund> Funds { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    public FundScenarioSetup(FundScenario scenario)
    {
        Funds = GetFundsForScenario(scenario);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
            Funds.Count == 0
                ? []
                : [
                    new FundAmount
                    {
                        FundId = Funds.First().Id,
                        Amount = 1500.00m
                    }
                  ]);
        GetService<IAccountRepository>().Add(Account);
    }

    /// <summary>
    /// Creates the needed Funds for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    private List<Fund> GetFundsForScenario(FundScenario scenario)
    {
        if (scenario == FundScenario.One)
        {
            Fund fund = GetService<AddFundAction>().Run("Test");
            GetService<IFundRepository>().Add(fund);
            return [fund];
        }
        if (scenario == FundScenario.Multiple)
        {
            Fund fund = GetService<AddFundAction>().Run("Test");
            GetService<IFundRepository>().Add(fund);
            Fund otherFund = GetService<AddFundAction>().Run("OtherTest");
            GetService<IFundRepository>().Add(otherFund);
            return [fund, otherFund];
        }
        if (scenario == FundScenario.Duplicate)
        {
            Fund fund = GetService<AddFundAction>().Run("Test");
            GetService<IFundRepository>().Add(fund);
            return [fund, fund];
        }
        return [];
    }
}