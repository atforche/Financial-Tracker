using System.Diagnostics.CodeAnalysis;
using Domain.Locations;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Locations;

/// <summary>
/// EF Core repository for Locations.
/// </summary>
public sealed class LocationRepository(DatabaseContext databaseContext) : ILocationRepository
{
    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Location? location)
    {
        LocationId locationId = new(id);
        location = databaseContext.Locations.SingleOrDefault(candidate => candidate.Id == locationId)
            ?? databaseContext.Locations.Local.SingleOrDefault(candidate => candidate.Id == locationId);
        return location != null;
    }

    /// <inheritdoc/>
    public bool TryGetByNormalizedName(string normalizedName, [NotNullWhen(true)] out Location? location)
    {
        location = databaseContext.Locations.SingleOrDefault(candidate => candidate.NormalizedName == normalizedName)
            ?? databaseContext.Locations.Local.SingleOrDefault(candidate => candidate.NormalizedName == normalizedName);
        return location != null;
    }

    /// <inheritdoc/>
    public void Add(Location location) => databaseContext.Locations.Add(location);

    /// <inheritdoc/>
    public void Delete(Location location) => databaseContext.Locations.Remove(location);

    /// <inheritdoc/>
    public bool IsReferenced(LocationId locationId) =>
        databaseContext.Transactions.OfType<IncomeTransaction>().Any(transaction => transaction.Source.Location != null && transaction.Source.Location.Id == locationId)
        || databaseContext.Transactions.OfType<SpendingTransaction>().Any(transaction => transaction.Destinations.Any(destination => destination.Location != null && destination.Location.Id == locationId))
        || databaseContext.Transactions.OfType<AccountTransaction>().Any(transaction =>
            (transaction.Source.Location != null && transaction.Source.Location.Id == locationId)
            || transaction.Destinations.Any(destination => destination.Location != null && destination.Location.Id == locationId));

    /// <inheritdoc/>
    public void Consolidate(Location source, Location target)
    {
        var transactions = databaseContext.Transactions.AsSplitQuery()
            .ToList()
            .Where(transaction => transaction.GetAllAffectedLocationIds().Contains(source.Id))
            .ToList();
        foreach (Domain.Transactions.Transaction transaction in transactions)
        {
            transaction.ReplaceLocation(source, target);
        }
        databaseContext.Locations.Remove(source);
    }
}
