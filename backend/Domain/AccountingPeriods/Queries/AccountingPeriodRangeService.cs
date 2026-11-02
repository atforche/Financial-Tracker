namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Resolves and validates Accounting Period ranges.
/// </summary>
public sealed class AccountingPeriodRangeService(IAccountingPeriodQueryRepository accountingPeriodQueryRepository)
{
    /// <summary>
    /// Resolves the requested Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeResolution> ResolveAsync(
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        (AccountingPeriod? start, AccountingPeriod? end, AccountingPeriodRangeQueryFailure failure) =
            await ResolveEndpointsAsync(startId, endId, cancellationToken);
        if (start == null || end == null)
        {
            return new AccountingPeriodRangeResolution(null, failure);
        }

        IReadOnlyCollection<AccountingPeriod> accountingPeriods = await accountingPeriodQueryRepository.GetRangeAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end),
            cancellationToken);
        return AccountingPeriodRangeResolver.IsContiguous(accountingPeriods, start, end)
            ? new AccountingPeriodRangeResolution(accountingPeriods, AccountingPeriodRangeQueryFailure.None)
            : new AccountingPeriodRangeResolution(null, AccountingPeriodRangeQueryFailure.NotContiguous);
    }

    /// <summary>
    /// Resolves the first and last Accounting Periods of the requested range without loading the periods between them.
    /// Both are null when the range is invalid.
    /// </summary>
    public async Task<(AccountingPeriod? Start, AccountingPeriod? End, AccountingPeriodRangeQueryFailure Failure)> ResolveEndpointsAsync(
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountingPeriod> endpoints = await accountingPeriodQueryRepository.GetByIdsAsync(
            [new(startId), new(endId)],
            cancellationToken);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeResolver.ResolveEndpoints(
            endpoints,
            startId,
            endId,
            out AccountingPeriod? start,
            out AccountingPeriod? end);
        return failure == AccountingPeriodRangeQueryFailure.None ? (start, end, failure) : (null, null, failure);
    }
}
