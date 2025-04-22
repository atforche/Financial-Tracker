using System.Collections;

namespace Tests.GetAccountBalanceByAccountingPeriodTests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period Overlap scenarios for getting an
/// Account Balance by Accounting Period
/// </summary>
public sealed class AccountingPeriodOverlapScenarios : IEnumerable<TheoryDataRow<DateOnly>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateOnly>> GetEnumerator() => new Tests.Scenarios.AddBalanceEventDateScenarios()
        .Where(row => Tests.Scenarios.AddBalanceEventDateScenarios.IsValid(row.Data))
        .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}