using Models.Income;

namespace Models.AccountingPeriods;

/// <summary>
/// Request model for an expected income source.
/// </summary>
public sealed class ExpectedIncomeSourceRequestModel
{
    /// <summary>
    /// Source name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Economic composition expected for each payment.
    /// </summary>
    public required IncomeBreakdownRequestModel Income { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }
}