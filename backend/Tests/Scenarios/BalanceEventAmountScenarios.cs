using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Setups;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Balance Event Amount scenarios that should be tested
/// </summary>
public sealed class BalanceEventAmountScenarios : IEnumerable<TheoryDataRow<BalanceEventAmountScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<BalanceEventAmountScenario>> GetEnumerator() => Enum.GetValues<BalanceEventAmountScenario>()
        .Select(scenario => new TheoryDataRow<BalanceEventAmountScenario>(scenario)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different Balance Event Amount Scenarios
/// </summary>
public enum BalanceEventAmountScenario
{
    /// <summary>
    /// Positive Amount
    /// </summary>
    Positive,

    /// <summary>
    /// Zero Amount
    /// </summary>
    Zero,

    /// <summary>
    /// Negative Amount
    /// </summary>
    Negative,

    /// <summary>
    /// Amount that forces the balance of a Fund within the Account to be negative
    /// </summary>
    ForcesFundBalanceNegative,

    /// <summary>
    /// Amount that forces the balance of the Account to be zero
    /// </summary>
    ForcesAccountBalanceToZero,

    /// <summary>
    /// Amount that forces the balance of the Account to be negative
    /// </summary>
    ForcesAccountBalanceNegative,

    /// <summary>
    /// Amount that forces a future Balance Event to force the Account balance to be negative
    /// </summary>
    ForcesFutureEventToMakeAccountBalanceNegative,

    /// <summary>
    /// Amount that forces the Account Balance at the end of the Accounting Period to be negative
    /// </summary>
    ForcesAccountBalancesAtEndOfPeriodToBeNegative,
}

/// <summary>
/// Setup class for a Balance Event Amount scenario
/// </summary>
internal class BalanceEventAmountScenarioSetup : ScenarioSetup
{
    private readonly AccountingPeriod _futureAccountingPeriod;

    /// <summary>
    /// Fund for this Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for this Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Current Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Amount for this Setup
    /// </summary>
    public decimal Amount { get; protected init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    /// <param name="accountType">Account Type for this Setup</param>
    public BalanceEventAmountScenarioSetup(BalanceEventAmountScenario scenario, AccountType accountType = AccountType.Standard)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("Test2");
        fundRepository.Add(OtherFund);

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        AccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 1);
        accountingPeriodRepository.Add(AccountingPeriod);
        _futureAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 2);
        accountingPeriodRepository.Add(_futureAccountingPeriod);

        Account = GetService<IAccountService>().CreateNewAccount("Test", accountType,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        Amount = DetermineAmountForSetup(scenario);
    }

    /// <summary>
    /// Determines the Balance Event amount for this Setup
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    /// <returns>The Balance Event Amount to use for this setup</returns>
    private decimal DetermineAmountForSetup(BalanceEventAmountScenario scenario)
    {
        if (scenario == BalanceEventAmountScenario.Positive)
        {
            return 1000.00m;
        }
        if (scenario == BalanceEventAmountScenario.Negative)
        {
            return -1000.00m;
        }
        if (scenario == BalanceEventAmountScenario.ForcesFundBalanceNegative)
        {
            return -2000.00m;
        }
        if (scenario == BalanceEventAmountScenario.ForcesAccountBalanceToZero)
        {
            return -3000.00m;
        }
        if (scenario == BalanceEventAmountScenario.ForcesAccountBalanceNegative)
        {
            return -4000.00m;
        }
        if (scenario == BalanceEventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative)
        {
            GetService<IAccountingPeriodService>().AddChangeInValue(_futureAccountingPeriod,
                new DateOnly(2025, 2, 28),
                Account,
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = -2999.99m,
                });
            return -100.00m;
        }
        if (scenario == BalanceEventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative)
        {
            GetService<IAccountingPeriodService>().AddChangeInValue(_futureAccountingPeriod,
                new DateOnly(2025, 1, 1),
                Account,
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 3000.00m,
                });
            return -4000.00m;
        }
        return 0.00m;
    }
}