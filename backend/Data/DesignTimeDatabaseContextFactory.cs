using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data;

/// <summary>
/// Creates the database context for Entity Framework Core design-time commands.
/// </summary>
public sealed class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    /// <inheritdoc />
    public DatabaseContext CreateDbContext(string[] args)
    {
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new DatabaseContext(options);
    }
}