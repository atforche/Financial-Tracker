using Data;
using Microsoft.EntityFrameworkCore;

string databasePath = Environment.GetEnvironmentVariable("DATABASE_PATH")
    ?? throw new InvalidOperationException("Environment variable 'DATABASE_PATH' is not set.");
if (string.IsNullOrWhiteSpace(databasePath))
{
    throw new InvalidOperationException("Environment variable 'DATABASE_PATH' must not be empty.");
}

DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
    .UseSqlite($"Data Source={databasePath}")
    .Options;
await using DatabaseContext databaseContext = new(options);

IEnumerable<string> pendingMigrations = await databaseContext.Database.GetPendingMigrationsAsync();
string[] migrationNames = pendingMigrations.ToArray();
if (migrationNames.Length == 0)
{
    Console.WriteLine("The database is already up to date.");
    return;
}

foreach (string migrationName in migrationNames)
{
    Console.WriteLine($"Applying migration {migrationName}.");
}

await databaseContext.Database.MigrateAsync();
Console.WriteLine("Database migrations completed successfully.");