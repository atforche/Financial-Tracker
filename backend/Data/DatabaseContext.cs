using Domain.AccountingPeriods;
using Domain.AccountGoals;
using Domain.Accounts;
using Domain.FundGoals;
using Domain.Funds;
using Domain.Locations;
using Domain.Transactions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>
/// Main DbContext for this application
/// </summary>
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
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
    /// Collection of pending Account Balance effects.
    /// </summary>
    internal DbSet<PendingAccountBalanceEffect> PendingAccountBalanceEffects { get; set; } = default!;

    /// <summary>
    /// Collection of Account Goals in the database.
    /// </summary>
    internal DbSet<AccountGoal> AccountGoals { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Periods in the database
    /// </summary>
    internal DbSet<AccountingPeriod> AccountingPeriods { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Period Balance Histories in the database
    /// </summary>
    internal DbSet<AccountingPeriodBalanceHistory> AccountingPeriodBalanceHistories { get; set; } = default!;

    /// <summary>
    /// Collection of Funds in the database
    /// </summary>
    internal DbSet<Fund> Funds { get; set; } = default!;

    /// <summary>
    /// Collection of Fund Balance Histories in the database
    /// </summary>
    internal DbSet<FundBalanceHistory> FundBalanceHistories { get; set; } = default!;

    /// <summary>
    /// Collection of pending Fund Balance effects.
    /// </summary>
    internal DbSet<PendingFundBalanceEffect> PendingFundBalanceEffects { get; set; } = default!;

    /// <summary>
    /// Collection of Fund Goals in the database.
    /// </summary>
    internal DbSet<FundGoal> FundGoals { get; set; } = default!;

    /// <summary>
    /// Collection of Fund Goal totals history entries in the database.
    /// </summary>
    internal DbSet<FundGoalTotalsHistory> FundGoalTotalsHistories { get; set; } = default!;

    /// <summary>
    /// Collection of pending Fund Goal totals effects.
    /// </summary>
    internal DbSet<PendingFundGoalTotalsEffect> PendingFundGoalTotalsEffects { get; set; } = default!;

    /// <summary>
    /// Collection of Transactions in the database
    /// </summary>
    internal DbSet<Transaction> Transactions { get; set; } = default!;

    /// <summary>
    /// Collection of Locations in the database.
    /// </summary>
    internal DbSet<Location> Locations { get; set; } = default!;

    /// <summary>
    /// Collection of application users in the database.
    /// </summary>
    internal DbSet<User> Users { get; set; } = default!;

    /// <summary>
    /// Collection of user invitations in the database.
    /// </summary>
    internal DbSet<UserInvitation> UserInvitations { get; set; } = default!;

    /// <summary>
    /// Collection of user-management audit events in the database.
    /// </summary>
    internal DbSet<UserAdministrationAuditEvent> UserAdministrationAuditEvents { get; set; } = default!;

    /// <summary>
    /// Gets the database path for this Database Context
    /// </summary>
    protected virtual string DatabasePath => EnvironmentManager.Instance.DatabasePath;

    /// <summary>
    /// Run a health check to ensure the database is in the correct state
    /// </summary>
    public void RunHealthCheck()
    {
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
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = Database.BeginTransaction();
        int affectedRows = Database.ExecuteSqlRaw("""
            UPDATE "__EFMigrationsHistory"
            SET "ProductVersion" = "ProductVersion" || '-health-check'
            WHERE "MigrationId" = (
                SELECT "MigrationId"
                FROM "__EFMigrationsHistory"
                ORDER BY "MigrationId"
                LIMIT 1)
            """);
        if (affectedRows != 1)
        {
            throw new InvalidOperationException("The database migration history is unavailable.");
        }
        transaction.Rollback();
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            _ = optionsBuilder.UseSqlite(
                $"Data Source={DatabasePath}",
                options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
}
