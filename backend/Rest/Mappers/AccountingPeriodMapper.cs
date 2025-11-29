using Domain.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Periods to Accounting Period Models
/// </summary>
internal sealed class AccountingPeriodMapper
{
    /// <summary>
    /// Converts the provided Accounting Period into an Accounting Period Model
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to convert into a model</param>
    /// <returns>The Accounting Period Model corresponding to the provided Accounting Period</returns>
    public static AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen
    };
}