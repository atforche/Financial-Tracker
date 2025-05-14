using System.Collections;

namespace Tests.PostTransaction.Scenarios;

/// <summary>
/// Collection class that contains all the unique Account Type scenarios for posting a Transaction
/// </summary>
public sealed class AccountTypeScenarios : IEnumerable<TheoryDataRow<AccountTypeScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountTypeScenario>> GetEnumerator() =>
        Enum.GetValues<AccountTypeScenario>()
            .Select(value => new TheoryDataRow<AccountTypeScenario>(value))
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if the provided scenario is valid, false otherwise</returns>
    public static bool IsValid(AccountTypeScenario scenario)
    {
        List<AccountTypeScenario> invalidScenarios =
        [
            AccountTypeScenario.MissingDebit,
            AccountTypeScenario.MissingCredit
        ];
        return !invalidScenarios.Contains(scenario);
    }
}

/// <summary>
/// Enum representing the different <see cref="AccountTypeScenarios"/>
/// </summary>
public enum AccountTypeScenario
{
    /// <summary>
    /// Posts the Transaction in the Debit Account
    /// </summary>
    Debit,

    /// <summary>
    /// Attempts to post a credit-only Transaction in the Debit Account
    /// </summary>
    MissingDebit,

    /// <summary>
    /// Posts the Transaction in the Credit Account
    /// </summary>
    Credit,

    /// <summary>
    /// Attempts to post a debit-only Transaction in the Credit Account
    /// </summary>
    MissingCredit,
}