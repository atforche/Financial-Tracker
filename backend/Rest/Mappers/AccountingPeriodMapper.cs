using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Data.AccountingPeriods;
using Domain.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Periods to Accounting Period Models
/// </summary>
public sealed class AccountingPeriodMapper(AccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Maps the provided Accounting Period to an Accounting Period Model
    /// </summary>
    public static AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Name = accountingPeriod.PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen
    };

    /// <summary>
    /// Attempts to map the provided ID to an Accounting Period
    /// </summary>
    public bool TryToDomain(Guid accountingPeriodId, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod) =>
        accountingPeriodRepository.TryGetById(accountingPeriodId, out accountingPeriod);
}