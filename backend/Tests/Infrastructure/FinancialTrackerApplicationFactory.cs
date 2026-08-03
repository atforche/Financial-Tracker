using Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests.Infrastructure;

/// <summary>
/// Creates an application instance backed by an isolated SQLite database.
/// </summary>
internal class FinancialTrackerApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"financial-tracker-tests-{Guid.NewGuid():N}.db");

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public FinancialTrackerApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Testing");
        _ = builder.ConfigureServices(services =>
        {
            _ = services.RemoveAll<DbContextOptions<DatabaseContext>>();
            _ = services.RemoveAll<DbContextOptions>();
            _ = services.RemoveAll<DatabaseContext>();
            _ = services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source={_databasePath}"));
        });
    }

    /// <summary>
    /// Applies the application's migrations to this test database.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await databaseContext.Database.MigrateAsync();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}