using Domain.AccountingPeriods.Queries;

namespace Domain.Locations.Queries;

/// <summary>
/// Reads Locations and summarizes where money went over a range.
/// </summary>
public sealed class LocationQueryService(
    ILocationQueryRepository locationQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Gets a page of Locations.
    /// </summary>
    public Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default) =>
        locationQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Gets the Locations' activity over a date range.
    /// </summary>
    public Task<LocationRange> GetDateRangeAsync(
        DateOnly start,
        DateOnly end,
        LocationRangeQuery query,
        CancellationToken cancellationToken = default) =>
        BuildRangeAsync(new LocationFlowScope(start, end, null, null), query, cancellationToken);

    /// <summary>
    /// Gets the Locations' activity over an Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<LocationRange>> GetAccountingPeriodRangeAsync(
        Guid startId,
        Guid endId,
        LocationRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(startId, endId, cancellationToken);
        return resolution.AccountingPeriods == null
            ? new AccountingPeriodRangeQueryResult<LocationRange>(null, resolution.Failure)
            : new AccountingPeriodRangeQueryResult<LocationRange>(
                await BuildRangeAsync(
                    new LocationFlowScope(null, null, resolution.AccountingPeriods.Select(period => period.Id).ToList(), null),
                    query,
                    cancellationToken),
                AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Gets one Location's activity over a date range, by month, and how it was paid for. Returns null when the
    /// Location does not exist.
    /// </summary>
    public async Task<LocationDetailRange?> GetDetailDateRangeAsync(
        Guid locationId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        Location? location = await GetLocationAsync(locationId, cancellationToken);
        if (location == null)
        {
            return null;
        }
        IEnumerable<(int Year, int Month)> months = Enumerable
            .Range(0, ((end.Year - start.Year) * 12) + end.Month - start.Month + 1)
            .Select(start.AddMonths)
            .Select(date => (date.Year, date.Month));
        return await BuildDetailAsync(
            location,
            new LocationFlowScope(start, end, null, location.Id),
            months,
            flow => (flow.Date.Year, flow.Date.Month),
            cancellationToken);
    }

    /// <summary>
    /// Gets one Location's activity over an Accounting Period range, by period, and how it was paid for. Returns null
    /// when the Location does not exist.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<LocationDetailRange>?> GetDetailAccountingPeriodRangeAsync(
        Guid locationId,
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        Location? location = await GetLocationAsync(locationId, cancellationToken);
        if (location == null)
        {
            return null;
        }
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(startId, endId, cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<LocationDetailRange>(null, resolution.Failure);
        }
        var periodMonths = resolution.AccountingPeriods.ToDictionary(period => period.Id, period => (period.Year, period.Month));
        return new AccountingPeriodRangeQueryResult<LocationDetailRange>(
            await BuildDetailAsync(
                location,
                new LocationFlowScope(null, null, periodMonths.Keys.ToList(), location.Id),
                periodMonths.Values,
                flow => periodMonths[flow.AccountingPeriodId],
                cancellationToken),
            AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Gets a Location by ID, or null when it does not exist.
    /// </summary>
    private async Task<Location?> GetLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        (await locationQueryRepository.GetAsync(
            new LocationQuery(new LocationFilter(null, [locationId]), 0, 1),
            cancellationToken)).Items.SingleOrDefault();

    /// <summary>
    /// Summarizes a Location's flows over a range.
    /// </summary>
    private static LocationRangeSummary Summarize(Location location, IEnumerable<LocationFlow> flows)
    {
        var all = flows.ToList();
        return new LocationRangeSummary(
            location,
            all.Sum(flow => flow.Spent),
            all.Sum(flow => flow.Received),
            all.Select(flow => flow.TransactionId).Distinct().Count(),
            all.Where(flow => flow.Spent != 0).Select(flow => flow.TransactionId).Distinct().Count(),
            all.Count == 0 ? null : all.Max(flow => flow.Date));
    }

    /// <summary>
    /// Builds the matching Locations' activity, sorted and paged, with totals across all of them.
    /// </summary>
    private async Task<LocationRange> BuildRangeAsync(
        LocationFlowScope scope,
        LocationRangeQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Location> locations = (await locationQueryRepository.GetAsync(
            new LocationQuery(new LocationFilter(query.NameSearch, []), 0, null),
            cancellationToken)).Items;
        ILookup<LocationId, LocationFlow> flowsByLocation = (await locationQueryRepository.GetFlowsAsync(scope, cancellationToken))
            .ToLookup(flow => flow.LocationId);
        var summaries = locations
            .Select(location => Summarize(location, flowsByLocation[location.Id]))
            .ToList();
        IReadOnlyCollection<LocationRangeSummary> items = Sort(summaries, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        return new LocationRange(
            new QueryPage<LocationRangeSummary>(items, summaries.Count),
            summaries.Sum(summary => summary.Spent),
            summaries.Sum(summary => summary.Received),
            summaries.Count(summary => summary.TransactionCount > 0),
            summaries.Select(summary => summary.Spent).OrderDescending().Take(5).Sum());
    }

    /// <summary>
    /// Builds one Location's activity by month and how it was paid for.
    /// </summary>
    private async Task<LocationDetailRange> BuildDetailAsync(
        Location location,
        LocationFlowScope scope,
        IEnumerable<(int Year, int Month)> months,
        Func<LocationFlow, (int Year, int Month)> getMonth,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<LocationFlow> flows = await locationQueryRepository.GetFlowsAsync(scope, cancellationToken);
        ILookup<(int Year, int Month), LocationFlow> flowsByMonth = flows.ToLookup(getMonth);
        IReadOnlyCollection<LocationPayment> payments = await locationQueryRepository.GetPaymentsAsync(scope, cancellationToken);
        return new LocationDetailRange(
            Summarize(location, flows),
            months
                .Select(month => new LocationMonth(
                    month.Year,
                    month.Month,
                    flowsByMonth[month].Sum(flow => flow.Spent),
                    flowsByMonth[month].Sum(flow => flow.Received)))
                .ToList(),
            payments
                .Where(payment => payment.Account != null)
                .GroupBy(payment => payment.Account!.Id)
                .Select(group => new LocationPaymentShare(group.Key.Value, group.First().Account!.Name, group.Sum(payment => payment.Amount)))
                .OrderByDescending(share => share.Amount)
                .ToList(),
            payments
                .SelectMany(payment => payment.Funds)
                .GroupBy(share => share.Fund.Id)
                .Select(group => new LocationPaymentShare(group.Key.Value, group.First().Fund.Name, group.Sum(share => share.Amount)))
                .OrderByDescending(share => share.Amount)
                .ToList());
    }

    /// <summary>
    /// Orders Location summaries, breaking ties by name.
    /// </summary>
    private static IEnumerable<LocationRangeSummary> Sort(IEnumerable<LocationRangeSummary> summaries, LocationRangeSort sort) =>
        sort switch
        {
            LocationRangeSort.Name => summaries.OrderBy(summary => summary.Location.Name),
            LocationRangeSort.NameDescending => summaries.OrderByDescending(summary => summary.Location.Name),
            LocationRangeSort.Spent => summaries.OrderBy(summary => summary.Spent).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.Received => summaries.OrderBy(summary => summary.Received).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.ReceivedDescending => summaries.OrderByDescending(summary => summary.Received).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.TransactionCount => summaries.OrderBy(summary => summary.TransactionCount).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.TransactionCountDescending => summaries.OrderByDescending(summary => summary.TransactionCount).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.LastActivity => summaries.OrderBy(summary => summary.LastActivity).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.LastActivityDescending => summaries.OrderByDescending(summary => summary.LastActivity).ThenBy(summary => summary.Location.Name),
            LocationRangeSort.SpentDescending or _ => summaries.OrderByDescending(summary => summary.Spent).ThenBy(summary => summary.Location.Name),
        };
}
