using Domain;
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
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> transactions = ApplyScope(scope);
        LocationId? locationId = scope.LocationId;
        var lines = new List<(TransactionId TransactionId, LocationId? LocationId, DateOnly Date, Domain.AccountingPeriods.AccountingPeriodId PeriodId, decimal Received, decimal Spent)>();

        // Each transaction type keeps its Locations on different sides, so each is read separately
        lines.AddRange((await transactions.OfType<IncomeTransaction>()
            .Select(transaction => new { TransactionId = transaction.Id, LocationId = EF.Property<LocationId?>(transaction.Source, "LocationId"), transaction.Date, transaction.AccountingPeriodId, transaction.Amount })
            .Where(line => line.LocationId != null && (locationId == null || line.LocationId == locationId))
            .ToListAsync(cancellationToken))
            .Select(line => (line.TransactionId, line.LocationId, line.Date, line.AccountingPeriodId, line.Amount, 0m)));
        lines.AddRange((await transactions.OfType<SpendingTransaction>()
            .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new { TransactionId = transaction.Id, LocationId = EF.Property<LocationId?>(destination, "LocationId"), transaction.Date, transaction.AccountingPeriodId, destination.Amount })
            .Where(line => line.LocationId != null && (locationId == null || line.LocationId == locationId))
            .ToListAsync(cancellationToken))
            .Select(line => (line.TransactionId, line.LocationId, line.Date, line.AccountingPeriodId, 0m, line.Amount)));
        lines.AddRange((await transactions.OfType<RefundTransaction>()
            .SelectMany(transaction => transaction.Sources, (transaction, source) => new { TransactionId = transaction.Id, LocationId = EF.Property<LocationId?>(source, "LocationId"), transaction.Date, transaction.AccountingPeriodId, source.Amount })
            .Where(line => line.LocationId != null && (locationId == null || line.LocationId == locationId))
            .ToListAsync(cancellationToken))
            .Select(line => (line.TransactionId, line.LocationId, line.Date, line.AccountingPeriodId, line.Amount, 0m)));
        lines.AddRange((await transactions.OfType<AccountTransaction>()
            .Select(transaction => new { TransactionId = transaction.Id, LocationId = EF.Property<LocationId?>(transaction.Source, "LocationId"), transaction.Date, transaction.AccountingPeriodId, transaction.Amount })
            .Where(line => line.LocationId != null && (locationId == null || line.LocationId == locationId))
            .ToListAsync(cancellationToken))
            .Select(line => (line.TransactionId, line.LocationId, line.Date, line.AccountingPeriodId, line.Amount, 0m)));
        lines.AddRange((await transactions.OfType<AccountTransaction>()
            .SelectMany(transaction => transaction.Destinations, (transaction, destination) => new { TransactionId = transaction.Id, LocationId = EF.Property<LocationId?>(destination, "LocationId"), transaction.Date, transaction.AccountingPeriodId, destination.Amount })
            .Where(line => line.LocationId != null && (locationId == null || line.LocationId == locationId))
            .ToListAsync(cancellationToken))
            .Select(line => (line.TransactionId, line.LocationId, line.Date, line.AccountingPeriodId, 0m, line.Amount)));
        return lines
            .Select(line => new LocationFlow(line.TransactionId, line.LocationId!, line.Date, line.PeriodId, line.Received, line.Spent))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<LocationPayment>> GetPaymentsAsync(
        LocationFlowScope scope,
        CancellationToken cancellationToken = default)
    {
        LocationId locationId = scope.LocationId ?? throw new ArgumentException("Payments are read for one Location.", nameof(scope));
        IQueryable<Transaction> transactions = ApplyScope(scope).AsSplitQuery();
        List<SpendingTransaction> spending = await transactions.OfType<SpendingTransaction>()
            .Where(transaction => transaction.Destinations.Any(destination => EF.Property<LocationId?>(destination, "LocationId") == locationId))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransfers = await transactions.OfType<AccountTransaction>()
            .Where(transaction => transaction.Destinations.Any(destination => EF.Property<LocationId?>(destination, "LocationId") == locationId))
            .ToListAsync(cancellationToken);
        var fundIds = spending
            .SelectMany(transaction => transaction.Destinations)
            .SelectMany(destination => destination.FundAssignments)
            .Select(assignment => assignment.FundId)
            .ToHashSet();
        Dictionary<FundId, Fund> funds = await databaseContext.Funds.AsNoTracking()
            .Where(fund => fundIds.Contains(fund.Id))
            .ToDictionaryAsync(fund => fund.Id, cancellationToken);
        return
        [
            .. spending.SelectMany(transaction => transaction.Destinations
                .Where(destination => destination.Location?.Id == locationId)
                .Select(destination => new LocationPayment(
                    transaction.Source.Account,
                    destination.FundAssignments.Select(assignment => (funds[assignment.FundId], assignment.Amount)).ToList(),
                    destination.Amount))),
            .. accountTransfers.SelectMany(transaction => transaction.Destinations
                .Where(destination => destination.Location?.Id == locationId)
                .Select(destination => new LocationPayment(transaction.Source.Account, [], destination.Amount))),
        ];
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
}
