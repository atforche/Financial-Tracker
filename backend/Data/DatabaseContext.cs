using Data.Models;
using Domain.Entities;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>
/// Main DbContext for this application
/// </summary>
public class DatabaseContext : DbContext
{
    private readonly IMediator _mediator;
    private readonly string _dbPath = Path.Join("/workspaces/Financial-Tracker/backend", "backend.db");

    /// <summary>
    /// Collection of Accounts in the database
    /// </summary>
    public DbSet<AccountData> Accounts { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Periods in the database
    /// </summary>
    public DbSet<AccountingPeriodData> AccountingPeriods { get; set; } = default!;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="mediator">Mediator instance used to publish events</param>
    public DatabaseContext(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Saves the entities associated with this DbContext and fires any domain events associated with those entities
    /// </summary>
    public async Task SaveEntitiesAsync()
    {
        IEnumerable<IDomainEvent> domainEvents = ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<Entity>()
            .SelectMany(entity =>
            {
                List<IDomainEvent> entityEvents = entity.GetDomainEvents().ToList();
                entity.ClearDomainEvents();
                return entityEvents;
            });
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent).ConfigureAwait(false);
        }
        await base.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={_dbPath}");

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountData>().HasKey(account => account.PrimaryKey);
        modelBuilder.Entity<AccountData>().HasIndex(account => account.Id).IsUnique();
        modelBuilder.Entity<AccountData>().HasIndex(account => account.Name).IsUnique();
        modelBuilder.Entity<AccountData>().Property(account => account.Type).HasConversion<string>();

        modelBuilder.Entity<AccountingPeriodData>().HasKey(accountingPeriod => accountingPeriod.PrimaryKey);
        modelBuilder.Entity<AccountingPeriodData>().HasIndex(accountingPeriod => accountingPeriod.Id).IsUnique();
        modelBuilder.Entity<AccountingPeriodData>().HasIndex(accountingPeriod => new { accountingPeriod.Year, accountingPeriod.Month }).IsUnique();
    }
}

/// <summary>
/// Design time factory for constructing DatabaseContexts
/// </summary>
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    /// <inheritdoc/>
    public DatabaseContext CreateDbContext(string[] args) =>
        new DatabaseContext(new Mediator(new ServiceCollection().BuildServiceProvider()));
}