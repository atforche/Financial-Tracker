using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups.Transaction;

/// <summary>
/// Setup class for a Fund test case
/// </summary>
internal sealed class FundSetup : TestCaseSetup
{
    /// <summary>
    /// Accounting Period for this Fund Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for this Fund Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Funds for this Fund Setup
    /// </summary>
    public List<Fund> Funds { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Fund Setup</param>
    public FundSetup(FundScenario scenario)
    {
        Funds = GetFundsForScenario(scenario);

        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            Funds.Count == 0
                ? []
                : [
                    new FundAmount
                    {
                        Fund = Funds.First(),
                        Amount = 1500.00m
                    }
                  ]);
        GetService<IAccountRepository>().Add(Account);
    }

    /// <summary>
    /// Gets the collection of Fund scenarios
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<TheoryDataRow<FundScenario>> GetCollection() => Enum.GetValues<FundScenario>()
        .Select(scenario => new TheoryDataRow<FundScenario>(scenario));

    /// <summary>
    /// Creates the needed Funds for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this Fund Setup</param>
    private List<Fund> GetFundsForScenario(FundScenario scenario)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        if (scenario == FundScenario.One)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            return [fund];
        }
        if (scenario == FundScenario.Multiple)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            Fund otherFund = fundService.CreateNewFund("OtherTest");
            fundRepository.Add(otherFund);
            return [fund, otherFund];
        }
        if (scenario == FundScenario.Duplicate)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            return [fund, fund];
        }
        return [];
    }
}

/// <summary>
/// Enum representing the different Fund Scenarios for a Transaction
/// </summary>
public enum FundScenario
{
    /// <summary>
    /// No Fund amounts under the Transaction
    /// </summary>
    None,

    /// <summary>
    /// A single Fund amount under the Transaction
    /// </summary>
    One,

    /// <summary>
    /// Multiple Fund amounts under the Transaction
    /// </summary>
    Multiple,

    /// <summary>
    /// Duplicate Fund amounts under the Transaction
    /// </summary>
    Duplicate
}