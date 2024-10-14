using Data.EntityModels;
using Data.ValueObjectModels;
using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>
/// Main DbContext for this application
/// </summary>
public class DatabaseContext : DbContext
{
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
    /// Collection of Transactions in the database
    /// </summary>
    public DbSet<TransactionData> Transactions { get; set; } = default!;

    /// <summary>
    /// Collection of Accounting Entries in the database
    /// </summary>
    public DbSet<AccountingEntryData> AccountingEntries { get; set; } = default!;

    /// <summary>
    /// Collection of Transaction Details in the database
    /// </summary>
    public DbSet<TransactionDetailData> TransactionDetails { get; set; } = default!;

    /// <summary>
    /// Collection of Account Starting Balances in the database
    /// </summary>
    public DbSet<AccountStartingBalanceData> AccountStartingBalances { get; set; } = default!;

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

        modelBuilder.Entity<TransactionData>().HasKey(transaction => transaction.PrimaryKey);
        modelBuilder.Entity<TransactionData>().HasIndex(transaction => transaction.Id);

        modelBuilder.Entity<AccountingEntryData>().HasKey(accountingEntry => accountingEntry.PrimaryKey);

        modelBuilder.Entity<TransactionDetailData>().HasKey(transactionDetail => transactionDetail.PrimaryKey);

        modelBuilder.Entity<AccountStartingBalanceData>().HasKey(accountStartingBalance => accountStartingBalance.PrimaryKey);
        modelBuilder.Entity<AccountStartingBalanceData>().HasIndex(accountStartingBalance => accountStartingBalance.AccountId);
        modelBuilder.Entity<AccountStartingBalanceData>().HasIndex(accountStartingBalance => accountStartingBalance.AccountingPeriodId);
    }
}