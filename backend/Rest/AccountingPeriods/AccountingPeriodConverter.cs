using Domain.AccountingPeriods;
using Models.AccountingPeriods;
using Rest.Income;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Periods to Accounting Period Models
/// </summary>
public sealed class AccountingPeriodConverter
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
    /// Converts an expected income source to its API model.
    /// </summary>
    public ExpectedIncomeSourceModel ToModel(ExpectedIncomeSource source) => new()
    {
        Id = source.Id.Value,
        Name = source.Name,
        Income = IncomeBreakdownConverter.ToModel(source.Income),
        ExpectedDates = source.ExpectedDates.Select(date => date.Date).ToList(),
        TrackedAmount = source.TrackedAmount,
        UntrackedAmount = source.UntrackedAmount,
        ExpectedAmount = source.ExpectedAmount,
    };
}