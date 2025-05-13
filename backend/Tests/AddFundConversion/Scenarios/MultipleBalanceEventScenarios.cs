using System.Collections;
using Tests.Scenarios;

namespace Tests.AddFundConversion.Scenarios;

/// <summary>
/// Collection class that contains all the unique Multiple Balance Event scenarios for adding a Fund Conversion
/// </summary>
public sealed class MultipleBalanceEventScenarios : IEnumerable<TheoryDataRow<AddBalanceEventMultipleBalanceEventScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AddBalanceEventMultipleBalanceEventScenario>> GetEnumerator()
    {
        List<AddBalanceEventMultipleBalanceEventScenario> scenariosToExclude =
        [
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceToZero,
            AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative,
            AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative
        ];
        return new AddBalanceEventMultipleBalanceEventScenarios()
            .Where(scenario => !scenariosToExclude.Contains(scenario))
            .GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        List<AddBalanceEventMultipleBalanceEventScenario> invalidScenarios =
        [
            AddBalanceEventMultipleBalanceEventScenario.ForcesFundBalanceNegative,
        ];
        return !invalidScenarios.Contains(scenario);
    }
}