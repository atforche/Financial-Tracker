using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;
using Domain.Locations.Queries;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Locations;

/// <summary>
/// EF Core query repository for Locations.
/// </summary>
public sealed class LocationQueryRepository(DatabaseContext databaseContext) : ILocationQueryRepository
{
    /// <inheritdoc/>
    public async Task<QueryPage<Location>> GetAsync(LocationQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Location> locations = databaseContext.Locations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Filter.NameSearch))
        {
            string pattern = LikeSearchPattern.Contains(query.Filter.NameSearch);
            locations = locations.Where(location => EF.Functions.Like(location.Name, pattern, LikeSearchPattern.EscapeCharacter));
        }
        if (query.Filter.Ids.Count > 0)
        {
            var ids = query.Filter.Ids.Select(id => new LocationId(id)).ToList();
            locations = locations.Where(location => ids.Contains(location.Id));
        }
        locations = locations.OrderBy(location => location.Name).ThenBy(location => location.Id);
        int totalCount = await locations.CountAsync(cancellationToken);
        locations = locations.Skip(query.Offset);
        if (query.Limit != null)
        {
            locations = locations.Take(query.Limit.Value);
        }
        return new QueryPage<Location>(await locations.ToListAsync(cancellationToken), totalCount);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<LocationFlow>> GetFlowsAsync(
        LocationFlowScope scope,
        CancellationToken cancellationToken = default) =>
        (await GetFlowLines(scope, null).ToListAsync(cancellationToken))
            .Select(line => new LocationFlow(line.TransactionId, line.LocationId!, line.Date, line.AccountingPeriodId, line.Received, line.Spent))
            .ToList();

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<LocationTotals>> GetTotalsAsync(
        LocationFlowScope scope,
        IReadOnlyCollection<LocationId>? locationIds,
        CancellationToken cancellationToken = default) =>
        await GetFlowLines(scope, locationIds)
            .GroupBy(line => line.LocationId!)
            .Select(group => new LocationTotals(
                group.Key,
                group.Sum(line => line.Spent),
                group.Sum(line => line.Received),
                group.Select(line => line.TransactionId).Distinct().Count(),
                group.Where(line => line.Spent != 0).Select(line => line.TransactionId).Distinct().Count(),
                group.Max(line => line.Date)))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<LocationPaymentShares> GetPaymentSharesAsync(
        LocationFlowScope scope,
        CancellationToken cancellationToken = default)
    {
        LocationId locationId = scope.LocationId ?? throw new ArgumentException("Payments are read for one Location.", nameof(scope));
        IQueryable<Transaction> transactions = ApplyScope(scope);
        var accountAmounts = await transactions.OfType<SpendingTransaction>()
            .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new
            {
                LocationId = EF.Property<LocationId?>(destination, "LocationId"),
                AccountId = (AccountId?)EF.Property<AccountId>(transaction.Source, "AccountId"),
                destination.Amount,
            })
            .Concat(transactions.OfType<AccountTransaction>()
                .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new
                {
                    LocationId = EF.Property<LocationId?>(destination, "LocationId"),
                    AccountId = EF.Property<AccountId?>(transaction.Source, "AccountId"),
                    destination.Amount,
                }))
            .Where(line => line.LocationId == locationId && line.AccountId != null)
            .GroupBy(line => line.AccountId!)
            .Select(group => new { AccountId = group.Key, Amount = group.Sum(line => line.Amount) })
            .ToListAsync(cancellationToken);
        var fundAmounts = await transactions.OfType<SpendingTransaction>()
            .SelectMany(transaction => transaction.Destinations)
            .Where(destination => EF.Property<LocationId?>(destination, "LocationId") == locationId)
            .SelectMany(destination => destination.FundAssignments)
            .GroupBy(assignment => assignment.FundId)
            .Select(group => new { FundId = group.Key, Amount = group.Sum(assignment => assignment.Amount) })
            .ToListAsync(cancellationToken);
        var accountIds = accountAmounts.Select(item => item.AccountId).ToList();
        Dictionary<AccountId, string> accountNames = await databaseContext.Accounts.AsNoTracking()
            .Where(account => accountIds.Contains(account.Id))
            .ToDictionaryAsync(account => account.Id, account => account.Name, cancellationToken);
        var fundIds = fundAmounts.Select(item => item.FundId).ToList();
        Dictionary<FundId, string> fundNames = await databaseContext.Funds.AsNoTracking()
            .Where(fund => fundIds.Contains(fund.Id))
            .ToDictionaryAsync(fund => fund.Id, fund => fund.Name, cancellationToken);
        return new LocationPaymentShares(
            accountAmounts
                .Select(item => new LocationPaymentShare(item.AccountId.Value, accountNames[item.AccountId], item.Amount))
                .OrderByDescending(share => share.Amount)
                .ToList(),
            fundAmounts
                .Select(item => new LocationPaymentShare(item.FundId.Value, fundNames[item.FundId], item.Amount))
                .OrderByDescending(share => share.Amount)
                .ToList());
    }

    /// <summary>
    /// Builds one query over every transaction line that names a Location, limited to one Location, a set of them, or
    /// neither. Each transaction type keeps its Locations on different sides, so each side is read and then combined.
    /// </summary>
    private IQueryable<FlowLine> GetFlowLines(LocationFlowScope scope, IReadOnlyCollection<LocationId>? locationIds)
    {
        IQueryable<Transaction> transactions = ApplyScope(scope);
        IQueryable<FlowLine> lines = transactions.OfType<IncomeTransaction>()
            .Select(transaction => new FlowLine
            {
                TransactionId = transaction.Id,
                LocationId = EF.Property<LocationId?>(transaction.Source, "LocationId"),
                Date = transaction.Date,
                AccountingPeriodId = transaction.AccountingPeriodId,
                Received = transaction.Amount,
                Spent = 0m,
            })
            .Concat(transactions.OfType<SpendingTransaction>()
                .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new FlowLine
                {
                    TransactionId = transaction.Id,
                    LocationId = EF.Property<LocationId?>(destination, "LocationId"),
                    Date = transaction.Date,
                    AccountingPeriodId = transaction.AccountingPeriodId,
                    Received = 0m,
                    Spent = destination.Amount,
                }))
            .Concat(transactions.OfType<RefundTransaction>()
                .SelectMany(transaction => transaction.Sources, (transaction, source) => new FlowLine
                {
                    TransactionId = transaction.Id,
                    LocationId = EF.Property<LocationId?>(source, "LocationId"),
                    Date = transaction.Date,
                    AccountingPeriodId = transaction.AccountingPeriodId,
                    Received = source.Amount,
                    Spent = 0m,
                }))
            .Concat(transactions.OfType<AccountTransaction>()
                .Select(transaction => new FlowLine
                {
                    TransactionId = transaction.Id,
                    LocationId = EF.Property<LocationId?>(transaction.Source, "LocationId"),
                    Date = transaction.Date,
                    AccountingPeriodId = transaction.AccountingPeriodId,
                    Received = transaction.Amount,
                    Spent = 0m,
                }))
            .Concat(transactions.OfType<AccountTransaction>()
                .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new FlowLine
                {
                    TransactionId = transaction.Id,
                    LocationId = EF.Property<LocationId?>(destination, "LocationId"),
                    Date = transaction.Date,
                    AccountingPeriodId = transaction.AccountingPeriodId,
                    Received = 0m,
                    Spent = destination.Amount,
                }))
            .Where(line => line.LocationId != null);
        if (scope.LocationId is { } locationId)
        {
            lines = lines.Where(line => line.LocationId == locationId);
        }
        if (locationIds != null)
        {
            lines = lines.Where(line => locationIds.Contains(line.LocationId));
        }
        return lines;
    }

    /// <summary>
    /// Limits transactions to a date range or a set of Accounting Periods.
    /// </summary>
    private IQueryable<Transaction> ApplyScope(LocationFlowScope scope)
    {
        IQueryable<Transaction> transactions = databaseContext.Transactions.AsNoTracking();
        if (scope.Start is DateOnly start)
        {
            transactions = transactions.Where(transaction => transaction.Date >= start);
        }
        if (scope.End is DateOnly end)
        {
            transactions = transactions.Where(transaction => transaction.Date <= end);
        }
        if (scope.AccountingPeriodIds is { } accountingPeriodIds)
        {
            transactions = transactions.Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId));
        }
        return transactions;
    }

    /// <summary>
    /// One transaction line that names a Location, with the money it received or spent there.
    /// </summary>
    private sealed class FlowLine
    {
        public required TransactionId TransactionId { get; init; }

        public required LocationId? LocationId { get; init; }

        public required DateOnly Date { get; init; }

        public required AccountingPeriodId AccountingPeriodId { get; init; }

        public required decimal Received { get; init; }

        public required decimal Spent { get; init; }
    }
}
