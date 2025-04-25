using System.Collections;

namespace Tests.GetAccountBalanceByDateTests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period Overlap scenarios for getting an Account Balance by Date
/// </summary>
public sealed class AccountingPeriodOverlapScenarios : IEnumerable<TheoryDataRow<AccountingPeriodType, DateOnly>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountingPeriodType, DateOnly>> GetEnumerator()
    {
        foreach (AccountingPeriodType accountingPeriodType in Enum.GetValues<AccountingPeriodType>())
        {
            yield return new TheoryDataRow<AccountingPeriodType, DateOnly>(accountingPeriodType, new DateOnly(2025, 1, 5));
            yield return new TheoryDataRow<AccountingPeriodType, DateOnly>(accountingPeriodType, new DateOnly(2025, 1, 15));
            yield return new TheoryDataRow<AccountingPeriodType, DateOnly>(accountingPeriodType, new DateOnly(2025, 1, 25));
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different Accounting Period types for getting an Account Balance by Date
/// </summary>
public enum AccountingPeriodType
{
    /// <summary>
    /// Past Accounting Period
    /// </summary>
    Past,

    /// <summary>
    /// Future Accounting Period
    /// </summary>
    Future,
}