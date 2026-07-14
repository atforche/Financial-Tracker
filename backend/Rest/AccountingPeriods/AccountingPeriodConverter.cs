using System.Diagnostics.CodeAnalysis;
using Data.AccountingPeriods;
using Domain.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Periods to Accounting Period Models
/// </summary>
public sealed class AccountingPeriodConverter(
    AccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Converts the provided Accounting Period to an Accounting Period Model
    /// </summary>
    public AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Name = accountingPeriod.Name,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen,
    };

    /// <summary>
    /// Attempts to convert the provided ID to an Accounting Period
    /// </summary>
    public bool TryToDomain(Guid accountingPeriodId, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod) =>
        accountingPeriodRepository.TryGetById(accountingPeriodId, out accountingPeriod);
}