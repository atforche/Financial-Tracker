using Data.Configuration.Accounts;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>
/// Main DbContext for this application
/// </summary>
public class DatabaseContext : DbContext
{
    /// <summary>
    /// Collection of Accounts in the database
    /// </summary>
    internal DbSet<Account> Accounts { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Periods in the database
    /// </summary>
    internal DbSet<AccountingPeriod> AccountingPeriods { get; set; } = default!;

    /// <summary>
    /// Collection of Funds in the database
    /// </summary>
    internal DbSet<Fund> Funds { get; set; } = default!;

    /// <summary>
    /// Gets the database path for this Database Context
    /// </summary>
    protected virtual string GetDatabasePath() => Path.Join("/workspaces/Financial-Tracker/backend", "backend.db");

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={GetDatabasePath()}");

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountConfiguration).Assembly);
}