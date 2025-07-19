using System.Collections;

namespace Tests.Old.GetAccountBalanceByAccountingPeriodTests.Scenarios;

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
}

/// <summary>
/// Enum representing the different <see cref="AccountingPeriodScenarios"/>
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

    /// <summary>
    /// Scenario where the prior Accounting Period has pending balance changes
    /// </summary>
    PriorPeriodHasPendingBalanceChanges,
}