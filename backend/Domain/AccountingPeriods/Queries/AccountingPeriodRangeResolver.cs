namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Resolves and validates Accounting Period ranges.
/// </summary>
public static class AccountingPeriodRangeResolver
{
    /// <summary>
    /// Resolves the requested endpoints and validates their chronological order.
    /// </summary>
    public static AccountingPeriodRangeQueryFailure ResolveEndpoints(
        IReadOnlyCollection<AccountingPeriod> endpoints,
        Guid startId,
        Guid endId,
        out AccountingPeriod? start,
        out AccountingPeriod? end)
    {
        start = endpoints.SingleOrDefault(period => period.Id.Value == startId);
        end = endpoints.SingleOrDefault(period => period.Id.Value == endId);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeQueryFailure.None;
        if (start == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.StartNotFound;
        }
        if (end == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.EndNotFound;
        }
        if (failure == AccountingPeriodRangeQueryFailure.None && GetChronologicalIndex(start!) > GetChronologicalIndex(end!))
        {
            failure = AccountingPeriodRangeQueryFailure.Reversed;
        }
        return failure;
    }

    /// <summary>
    /// Determines whether the provided Accounting Periods form the requested contiguous range.
    /// </summary>
    public static bool IsContiguous(
        IEnumerable<AccountingPeriod> periods,
        AccountingPeriod start,
        AccountingPeriod end) => periods
        .Select(GetChronologicalIndex)
        .Order()
        .SequenceEqual(Enumerable.Range(
            GetChronologicalIndex(start),
            GetChronologicalIndex(end) - GetChronologicalIndex(start) + 1));

    /// <summary>
    /// Calculates the chronological index of an Accounting Period.
    /// </summary>
    public static int GetChronologicalIndex(AccountingPeriod period) => (period.Year * 12) + period.Month;
}
