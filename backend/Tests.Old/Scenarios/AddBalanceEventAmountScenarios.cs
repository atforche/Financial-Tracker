using System.Collections;

namespace Tests.Old.Scenarios;

/// <summary>
/// Collection class that contains all the unique Amount scenarios for adding a Balance Event
/// </summary>
public sealed class AddBalanceEventAmountScenarios : IEnumerable<TheoryDataRow<decimal>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<decimal>> GetEnumerator()
    {
        List<decimal> amounts = [100.00m, 0.00m, -100.00m];
        return amounts.Select(amount => new TheoryDataRow<decimal>(amount)).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}