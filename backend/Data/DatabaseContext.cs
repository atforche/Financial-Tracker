using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
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
    /// Collection of Account Balance Histories in the database
    /// </summary>
    internal DbSet<AccountBalanceHistory> AccountBalanceHistories { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Periods in the database
    /// </summary>
    internal DbSet<AccountingPeriod> AccountingPeriods { get; set; } = default!;

    /// <summary>
    /// Collection of Funds in the database
    /// </summary>
    internal DbSet<Fund> Funds { get; set; } = default!;

    /// <summary>
    /// Collection of Fund Balance Histories in the database
    /// </summary>
    internal DbSet<FundBalanceHistory> FundBalanceHistories { get; set; } = default!;

    /// <summary>
    /// Collection of Transactions in the database
    /// </summary>
    internal DbSet<Transaction> Transactions { get; set; } = default!;

    /// <summary>
    /// Gets the database path for this Database Context
    /// </summary>
    protected virtual string DatabasePath => EnvironmentManager.Instance.DatabasePath;

    /// <summary>
    /// Run a health check to ensure the database is in the correct state
    /// </summary>
    public void RunHealthCheck()
    {
        if (!Path.Exists(DatabasePath))
        {
            throw new InvalidOperationException();
        }
        if (!Database.CanConnect())
        {
            throw new InvalidOperationException();
        }
        if (Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException();
        }
        if (Database.GetPendingMigrations().Any())
        {
            throw new InvalidOperationException();
        }
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
}