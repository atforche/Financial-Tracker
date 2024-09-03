using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>
/// Main DbContext for this application
/// </summary>
public class DatabaseContext : DbContext
{
    private readonly string _dbPath;

    /// <summary>
    /// Collection of Accounts in the database
    /// </summary>
    public DbSet<AccountData> Accounts { get; set; } = default!;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public DatabaseContext()
    {
        _dbPath = Path.Join("/workspaces/Financial-Tracker/backend", "backend.db");
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={_dbPath}");

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountData>().HasKey(account => account.PrimaryKey);
        modelBuilder.Entity<AccountData>().HasIndex(account => account.Id);
        modelBuilder.Entity<AccountData>().HasIndex(account => account.Name).IsUnique();
        modelBuilder.Entity<AccountData>().Property(account => account.Type).HasConversion<string>();
    }
}