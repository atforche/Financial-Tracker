namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create an Accounting Period
/// </summary>
public class CreateAccountingPeriodModel
{
    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.AccountingPeriod.Year"/>
    public required int Year { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.AccountingPeriod.Month"/>
    public required int Month { get; init; }
}