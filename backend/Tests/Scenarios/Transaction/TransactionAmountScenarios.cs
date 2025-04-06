using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;

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
internal sealed class TransactionAmountScenarioSetup : BalanceEventAmountScenarioSetup
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Setup</param>
    /// <param name="transactionAccountType">Transaction Account Type for this Setup</param>
    public TransactionAmountScenarioSetup(BalanceEventAmountScenario scenario, TransactionAccountType? transactionAccountType)
        : base(scenario, transactionAccountType == TransactionAccountType.Credit ? AccountType.Debt : AccountType.Standard)
    {
        if (scenario != BalanceEventAmountScenario.Negative)
        {
            Amount = Math.Abs(Amount);
        }
    }
}