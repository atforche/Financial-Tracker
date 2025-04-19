using System.Collections;

namespace Tests.CloseAccountingPeriod.Scenarios;

/// <summary>
/// Collection class that contains all the unique Balance Event scenarios for closing an Accounting Period
/// </summary>
public sealed class BalanceEventScenarios : IEnumerable<TheoryDataRow<BalanceEventScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<BalanceEventScenario>> GetEnumerator() => Enum.GetValues<BalanceEventScenario>()
        .Select(value => new TheoryDataRow<BalanceEventScenario>(value))
        .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario to validate</param>
    /// <returns>True if the provided scenario is valid, false otherwise</returns>
    public static bool IsValid(BalanceEventScenario scenario)
    {
        List<BalanceEventScenario> invalidScenarios = [BalanceEventScenario.UnpostedTransaction];
        return !invalidScenarios.Contains(scenario);
    }
}

/// <summary>
/// Enum representing the different Balance Event scenarios for closing an Accounting Period
/// </summary>
public enum BalanceEventScenario
{
    /// <summary>
    /// Scenario with an unposted Transaction in the Accounting Period
    /// </summary>
    UnpostedTransaction,

    /// <summary>
    /// Scenario with a posted Transaction in the Accounting Period
    /// </summary>
    PostedTransaction,

    /// <summary>
    /// Scenario with a Change in Value in the Accounting Period
    /// </summary>
    ChangeInValue,

    /// <summary>
    /// Scenario with a Fund Conversion in the Accounting Period
    /// </summary>
    FundConversion,
}