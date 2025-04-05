using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Scenarios.Transaction;

/// <summary>
/// Collection class that contains all the unique Transaction Fund scenarios that should be tested
/// </summary>
public sealed class TransactionFundScenarios : IEnumerable<TheoryDataRow<TransactionFundScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<TransactionFundScenario>> GetEnumerator() =>
        Enum.GetValues<TransactionFundScenario>()
            .Select(scenario => new TheoryDataRow<TransactionFundScenario>(scenario))
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different Transaction Fund Scenarios
/// </summary>
public enum TransactionFundScenario
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

/// <summary>
/// Setup class for a Transaction Fund scenario
/// </summary>
internal sealed class TransactionFundScenarioSetup : ScenarioSetup
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
    public TransactionFundScenarioSetup(TransactionFundScenario scenario)
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
    /// Creates the needed Funds for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    private List<Fund> GetFundsForScenario(TransactionFundScenario scenario)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        if (scenario == TransactionFundScenario.One)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            return [fund];
        }
        if (scenario == TransactionFundScenario.Multiple)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            Fund otherFund = fundService.CreateNewFund("OtherTest");
            fundRepository.Add(otherFund);
            return [fund, otherFund];
        }
        if (scenario == TransactionFundScenario.Duplicate)
        {
            Fund fund = fundService.CreateNewFund("Test");
            fundRepository.Add(fund);
            return [fund, fund];
        }
        return [];
    }
}