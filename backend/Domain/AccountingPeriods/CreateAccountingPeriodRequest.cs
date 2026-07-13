namespace Domain.AccountingPeriods;

/// <summary>
/// Record representing a request to create an <see cref="AccountingPeriod"/>
/// </summary>
public record CreateAccountingPeriodRequest
{
    /// <summary>
    /// Year for the Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for the Accounting Period
    /// </summary>
    public required int Month { get; init; }
}