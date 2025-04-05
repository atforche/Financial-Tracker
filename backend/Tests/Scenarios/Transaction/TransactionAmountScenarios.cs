using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;

namespace Tests.Scenarios.Transaction;

/// <summary>
/// Collection class that contains all the unique Transaction Amount scenarios that should be tested
/// </summary>
public sealed class TransactionAmountScenarios :
    IEnumerable<TheoryDataRow<BalanceEventAmountScenario, TransactionAccountType?>>
{
    /// <summary>
    /// List of Balance Event Amount scenarios that don't need separate tests for the debit and credit Accounts
    /// </summary>
    private static readonly List<BalanceEventAmountScenario> accountIndependentScenarios =
    [
        BalanceEventAmountScenario.Positive,
        BalanceEventAmountScenario.Zero,
        BalanceEventAmountScenario.Negative
    ];

    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<BalanceEventAmountScenario, TransactionAccountType?>> GetEnumerator()
    {
        foreach (BalanceEventAmountScenario scenario in new BalanceEventAmountScenarios().Select(row => row.Data))
        {
            if (accountIndependentScenarios.Contains(scenario))
            {
                yield return new TheoryDataRow<BalanceEventAmountScenario, TransactionAccountType?>(scenario, null);
            }
            else
            {
                foreach (TransactionAccountType accountType in Enum.GetValues<TransactionAccountType>())
                {
                    yield return new TheoryDataRow<BalanceEventAmountScenario, TransactionAccountType?>(scenario, accountType);
                }
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Setup class for a Transaction Amount scenario
/// </summary>
internal sealed class TransactionAmountScenarioSetup : ScenarioSetup
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
    /// Standard Account for this Setup
    /// </summary>
    public Account StandardAccount { get; }

    /// <summary>
    /// Debt Account for this Setup
    /// </summary>
    public Account DebtAccount { get; }

    /// <summary>
    /// Amount for this Setup
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    public TransactionAmountScenarioSetup(BalanceEventAmountScenario scenario)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("Test2");
        fundRepository.Add(OtherFund);

        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        IAccountService accountService = GetService<IAccountService>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        var baseSetup = new BalanceEventAmountScenarioSetup(scenario);
        StandardAccount = accountService.CreateNewAccount("TestOne",
            AccountType.Standard,
            baseSetup.AccountingPeriod.AccountBalanceCheckpoints.First().FundBalances);
        accountRepository.Add(StandardAccount);
        DebtAccount = accountService.CreateNewAccount("TestTwo",
            AccountType.Debt,
            baseSetup.AccountingPeriod.AccountBalanceCheckpoints.First().FundBalances);
        accountRepository.Add(DebtAccount);

        Amount = baseSetup.Amount;
        if (scenario != BalanceEventAmountScenario.Negative)
        {
            Amount = Math.Abs(Amount);
        }
    }
}