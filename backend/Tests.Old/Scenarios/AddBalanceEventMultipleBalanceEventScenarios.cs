using System.Collections;

namespace Tests.Old.Scenarios;

/// <summary>
/// Collection class that contains all the unique Multiple Balance Event scenarios for adding a Balance Event
/// </summary>
public sealed class AddBalanceEventMultipleBalanceEventScenarios : IEnumerable<TheoryDataRow<AddBalanceEventMultipleBalanceEventScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AddBalanceEventMultipleBalanceEventScenario>> GetEnumerator() =>
        Enum.GetValues<AddBalanceEventMultipleBalanceEventScenario>()
            .Select(value => new TheoryDataRow<AddBalanceEventMultipleBalanceEventScenario>(value))
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different <see cref="AddBalanceEventMultipleBalanceEventScenarios"/>
/// </summary>
public enum AddBalanceEventMultipleBalanceEventScenario
{
    /// <summary>
    /// Multiple Balance Events on the same day
    /// </summary>
    MultipleEventsSameDay,

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