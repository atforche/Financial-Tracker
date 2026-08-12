using Models.Income;

namespace Models.AccountingPeriods;

/// <summary>
/// Expected income from a named source during an Accounting Period.
/// </summary>
public sealed class ExpectedIncomeSourceModel
{
    /// <summary>
    /// Identifier for the source.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Source name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Economic composition expected for each payment.
    /// </summary>
    public required IncomeBreakdownModel Income { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }

    /// <summary>
    /// Net amount expected for one payment.
    /// </summary>
    public required decimal TrackedAmount { get; init; }

    /// <summary>
    /// Untracked income expected for one payment.
    /// </summary>
    public required decimal UntrackedAmount { get; init; }

    /// <summary>
    /// Total expected amount for this source.
    /// </summary>
    public required decimal ExpectedAmount { get; init; }
}