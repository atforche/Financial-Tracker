using Domain.AccountingPeriods;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a request to create an <see cref="AccountingPeriod"/>
/// </summary>
public class CreateAccountingPeriodModel
{
    /// <inheritdoc cref="AccountingPeriod.Year"/>
    public required int Year { get; init; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    public required int Month { get; init; }
}