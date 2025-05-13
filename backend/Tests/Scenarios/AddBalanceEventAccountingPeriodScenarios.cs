using System.Collections;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period scenarios for adding a Balance Event
/// </summary>
public sealed class AddBalanceEventAccountingPeriodScenarios : IEnumerable<TheoryDataRow<AddBalanceEventAccountingPeriodScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AddBalanceEventAccountingPeriodScenario>> GetEnumerator() =>
        Enum.GetValues<AddBalanceEventAccountingPeriodScenario>()
            .Select(value => new TheoryDataRow<AddBalanceEventAccountingPeriodScenario>(value))
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(AddBalanceEventAccountingPeriodScenario scenario) =>
        scenario == AddBalanceEventAccountingPeriodScenario.Open;
}

/// <summary>
/// Enum representing the different <see cref="AddBalanceEventAccountingPeriodScenarios"/>
/// </summary>
public enum AddBalanceEventAccountingPeriodScenario
{
    /// <summary>
    /// Add a Balance Event to an open Accounting Period
    /// </summary>
    Open,

    /// <summary>
    /// Add a Balance Event to a closed Accounting Period
    /// </summary>
    Closed,

    /// <summary>
    /// Add a Balance Event to an Accounting Period prior to when the Account was added
    /// </summary>
    PriorToAccountBeingAdded
}