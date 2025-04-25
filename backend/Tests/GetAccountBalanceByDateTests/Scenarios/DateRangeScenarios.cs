using System.Collections;

namespace Tests.GetAccountBalanceByDateTests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Date Range scenarios for getting an Account Balance by Date
/// </summary>
public sealed class DateRangeScenarios : IEnumerable<TheoryDataRow<DateRangeScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateRangeScenario>> GetEnumerator() => Enum.GetValues<DateRangeScenario>()
        .Select(value => new TheoryDataRow<DateRangeScenario>(value))
        .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different Date Range scenarios for getting an Account Balance by Date
/// </summary>
public enum DateRangeScenario
{
    /// <summary>
    /// Scenario where the date range starts before all of the Accounting Periods but ends within the first Accounting Period
    /// </summary>
    RangeExtendsIntoFirstAccountingPeriod,

    /// <summary>
    /// Scenario where the date range starts before the Account was added but ends after the Account was added
    /// </summary>
    RangeExtendsAfterAccountWasAdded,

    /// <summary>
    /// Scenario where the date range falls entirely after the Account was added
    /// </summary>
    RangeFallsAfterAccountWasAdded,

    /// <summary>
    /// Scenario where the date range starts in the last Accounting Period and extends after the last Accounting Period
    /// </summary>
    RangeExtendsOutOfLastAccountingPeriod,

    /// <summary>
    /// Scenario where the entire date range falls after all the Accounting Periods
    /// </summary>
    RangeFallsAfterAllAccountingPeriods,
}