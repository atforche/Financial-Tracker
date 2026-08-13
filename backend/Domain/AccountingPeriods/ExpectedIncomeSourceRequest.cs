using Domain.Transactions.Income;

namespace Domain.AccountingPeriods;

/// <summary>
/// Configures an expected income source for an Accounting Period.
/// </summary>
public sealed record ExpectedIncomeSourceRequest
{
    /// <summary>
    /// Source name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Income lines expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<IncomeLine> IncomeLines { get; init; }

    /// <summary>
    /// Deductions expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeduction> IncomeDeductions { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }
}