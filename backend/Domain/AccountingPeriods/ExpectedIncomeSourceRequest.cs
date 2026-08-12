using Domain.Income;

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
    /// Economic composition expected for each payment.
    /// </summary>
    public required IncomeBreakdown Income { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }
}