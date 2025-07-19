using System.Collections;

namespace Tests.Old.Scenarios;

/// <summary>
/// Collection class that contains all the unique Date Range scenarios for getting an Account Balance
/// </summary>
public sealed class GetAccountBalanceDateRangeScenarios : IEnumerable<TheoryDataRow<GetAccountBalanceDateRangeScenario>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<GetAccountBalanceDateRangeScenario>> GetEnumerator() => Enum.GetValues<GetAccountBalanceDateRangeScenario>()
        .Select(value => new TheoryDataRow<GetAccountBalanceDateRangeScenario>(value))
        .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different <see cref="GetAccountBalanceDateRangeScenarios"/>
/// </summary>
public enum GetAccountBalanceDateRangeScenario
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