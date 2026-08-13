using Domain.AccountingPeriods;
using Models;
using Models.AccountingPeriods;
using Models.Transactions.Types;

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
        IncomeLines = source.IncomeLines.Select(line => new IncomeLineModel
        {
            Description = line.Description,
            Amount = line.Amount,
        }).ToList(),
        IncomeDeductions = source.IncomeDeductions.Select(deduction => new IncomeDeductionModel
        {
            Description = deduction.Description,
            Amount = deduction.Amount,
        }).ToList(),
        ExpectedDates = source.ExpectedDates.Select(date => date.Date).ToList(),
        NetAmount = ToIncomeAmountModel(source.NetAmount, source.TrackedAmount),
        ExpectedAmount = ToIncomeAmountModel(source.ExpectedAmount, source.ExpectedTrackedAmount),
        UntrackedTransfers = source.UntrackedTransfers.Select(transfer => new ExpectedUntrackedIncomeTransferModel
        {
            Description = transfer.Description,
            Amount = transfer.Amount,
        }).ToList(),
    };

    private static IncomeAmountModel ToIncomeAmountModel(decimal total, decimal tracked) => new()
    {
        Total = total,
        Tracked = tracked,
        Untracked = total - tracked,
    };
}