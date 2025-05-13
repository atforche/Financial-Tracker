using System.Collections;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period scenarios for adding a Balance Event
/// </summary>
public sealed class AddBalanceEventAccountingPeriodScenarios : IEnumerable<TheoryDataRow<bool>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<bool>> GetEnumerator()
    {
        List<bool> isAccountingPeriodOpen = [true, false];
        return isAccountingPeriodOpen.Select(isOpen => new TheoryDataRow<bool>(isOpen)).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="isOpen">True if the Accounting Period should be left open, false otherwise</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(bool isOpen) => isOpen;
}