namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create an Accounting Period
/// </summary>
public class CreateAccountingPeriodModel
{
    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Year"/>
    public required int Year { get; set; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Month"/>
    public required int Month { get; set; }
}