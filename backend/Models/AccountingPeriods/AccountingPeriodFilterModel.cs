namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the filters for retrieving Accounting Periods.
/// </summary>
public class AccountingPeriodFilterModel
{
    /// <summary>
    /// Years to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Years { get; init; }

    /// <summary>
    /// Months to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Months { get; init; }

    /// <summary>
    /// Whether to include only open or only closed Accounting Periods
    /// </summary>
    public bool? IsOpen { get; init; }
}
