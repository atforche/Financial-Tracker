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
        IReadOnlyCollection<AccountingPeriodId> endpointIds = [new(startId), new(endId)];
        IReadOnlyCollection<AccountingPeriod> endpoints = await accountingPeriodQueryRepository.GetByIdsAsync(
            endpointIds,
            cancellationToken);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeResolver.ResolveEndpoints(
            endpoints,
            startId,
            endId,
            out AccountingPeriod? start,
            out AccountingPeriod? end);
        if (failure != AccountingPeriodRangeQueryFailure.None)
        {
            return new AccountingPeriodRangeResolution(null, failure);
        }

        IReadOnlyCollection<AccountingPeriod> accountingPeriods = await accountingPeriodQueryRepository.GetRangeAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start!),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end!),
            cancellationToken);
        return AccountingPeriodRangeResolver.IsContiguous(accountingPeriods, start!, end!)
            ? new AccountingPeriodRangeResolution(accountingPeriods, AccountingPeriodRangeQueryFailure.None)
            : new AccountingPeriodRangeResolution(null, AccountingPeriodRangeQueryFailure.NotContiguous);
    }
}