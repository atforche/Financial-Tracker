using Data.Configuration.Accounts;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;
using Domain.FundConversions;
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
    /// Collection of Accounting Periods in the database
    /// </summary>
    internal DbSet<AccountingPeriod> AccountingPeriods { get; set; } = default!;

    /// <summary>
    /// Collection of Change In Values in the database
    /// </summary>
    internal DbSet<ChangeInValue> ChangeInValues { get; set; } = default!;

    /// <summary>
    /// Collection of Fund Conversions in the database
    /// </summary>
    internal DbSet<FundConversion> FundConversions { get; set; } = default!;

    /// <summary>
    /// Collection of Funds in the database
    /// </summary>
    internal DbSet<Fund> Funds { get; set; } = default!;

    /// <summary>
    /// Collection of Transactions in the database
    /// </summary>
    internal DbSet<Transaction> Transactions { get; set; } = default!;

    /// <summary>
    /// Gets the database path for this Database Context
    /// </summary>
    protected virtual string DatabasePath { get; } = Path.Join("/workspaces/Financial-Tracker/backend", "backend.db");

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountConfiguration).Assembly);
}