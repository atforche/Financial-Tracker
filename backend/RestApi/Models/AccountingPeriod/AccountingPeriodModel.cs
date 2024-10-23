namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing an Accounting Period
/// </summary>
public class AccountingPeriodModel
{
    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Id"/>
    public required Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Year"/>
    public required int Year { get; init; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.Month"/>
    public required int Month { get; init; }

    /// <inheritdoc cref="Domain.Entities.AccountingPeriod.IsOpen"/>
    public required bool IsOpen { get; init; }

    /// <summary>
    /// Converts the Accounting Period domain entity into an Accounting Period REST model
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period domain entity to be converted</param>
    /// <returns>The converted Accounting Period REST model</returns>
    internal static AccountingPeriodModel ConvertEntityToModel(Domain.Entities.AccountingPeriod accountingPeriod) =>
        new AccountingPeriodModel
        {
            Id = accountingPeriod.Id,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
        };
}