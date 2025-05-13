using System.Collections;

namespace Tests.AddTransaction.Scenarios;

/// <summary>
/// Collection class that contains all the unique Fund scenarios for adding a Transaction
/// </summary>
public sealed class FundScenarios : IEnumerable<TheoryDataRow<FundScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<FundScenario>> GetEnumerator() =>
        Enum.GetValues<FundScenario>()
            .Select(scenario => new TheoryDataRow<FundScenario>(scenario))
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different Fund Scenarios
/// </summary>
public enum FundScenario
{
    /// <summary>
    /// No Fund amounts under the Transaction
    /// </summary>
    None,

    /// <summary>
    /// A single Fund amount under the Transaction
    /// </summary>
    One,

    /// <summary>
    /// Multiple Fund amounts under the Transaction
    /// </summary>
    Multiple,

    /// <summary>
    /// Duplicate Fund amounts under the Transaction
    /// </summary>
    Duplicate
}