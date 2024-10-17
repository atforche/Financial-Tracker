namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing an Accounting Period
/// </summary>
public class AccountingPeriodModel
{
    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Year"/>
    public required int Year { get; set; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Month"/>
    public required int Month { get; set; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.IsOpen"/>
    public required bool IsOpen { get; set; }
}