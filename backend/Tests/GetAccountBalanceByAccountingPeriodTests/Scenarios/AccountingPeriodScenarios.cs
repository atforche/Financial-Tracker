using System.Collections;

namespace Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period scenarios for getting an Account Balance by Accounting Period
/// </summary>
public sealed class AccountingPeriodScenarios : IEnumerable<TheoryDataRow<AccountingPeriodScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountingPeriodScenario>> GetEnumerator() => Enum.GetValues<AccountingPeriodScenario>()
        .Select(value => new TheoryDataRow<AccountingPeriodScenario>(value))
        .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario to validate</param>
    /// <returns>True if the provided scenario is valid, false otherwise</returns>
    public static bool IsValid(AccountingPeriodScenario scenario)
    {
        List<AccountingPeriodScenario> invalidScenarios = [AccountingPeriodScenario.PeriodBeforeAccountWasAdded];
        return !invalidScenarios.Contains(scenario);
    }
}

/// <summary>
/// Enum representing the different Accounting Period scenarios for getting an Account Balance by Accounting Period
/// </summary>
public enum AccountingPeriodScenario
{
    /// <summary>
    /// Scenario where the Account hasn't been added until a later Accounting Period
    /// </summary>
    PeriodBeforeAccountWasAdded,

    /// <summary>
    /// Scenario where the Account was adding during this Accounting Period
    /// </summary>
    PeriodAccountWasAdded,

    /// <summary>
    /// Scenario where the Account was added in a past Accounting Period
    /// </summary>
    PeriodAfterAccountWasAdded,
}