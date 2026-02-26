using Domain.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Model representing information about an Accounting Period required for sorting
/// </summary>
internal sealed class AccountingPeriodSortModel
{
    /// <summary>
    /// Accounting Period
    /// </summary>
    public required AccountingPeriod AccountingPeriod { get; init; }
}