using Data;
using Data.Users;
using Domain.Users;
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
UserRepository userRepository = new(databaseContext);
UserInvitationRepository invitationRepository = new(databaseContext);
UserAdministrationAuditEventRepository auditEventRepository = new(databaseContext);
UserManagementService userManagementService = new(userRepository, invitationRepository, auditEventRepository);
UserManagementBootstrapper userManagementBootstrapper = new(
    databaseContext,
    userRepository,
    auditEventRepository,
    userManagementService);

IEnumerable<string> pendingMigrations = await databaseContext.Database.GetPendingMigrationsAsync();
string[] migrationNames = pendingMigrations.ToArray();
if (migrationNames.Length == 0)
{
    Console.WriteLine("The database is already up to date.");
}
else
{
    foreach (string migrationName in migrationNames)
    {
        Console.WriteLine($"Applying migration {migrationName}.");
    }

    await databaseContext.Database.MigrateAsync();
    Console.WriteLine("Database migrations completed successfully.");
}

string? bootstrapAdminEmail = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_EMAIL");
if (!string.IsNullOrWhiteSpace(bootstrapAdminEmail))
{
    await userManagementBootstrapper.CreateBootstrapInvitationAsync(bootstrapAdminEmail);
    Console.WriteLine("Bootstrap administrator invitation created.");
}

string? authenticationMode = Environment.GetEnvironmentVariable("AUTH_MODE");
string? developmentSubject = Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_SUBJECT");
if (string.Equals(authenticationMode, "development", StringComparison.OrdinalIgnoreCase)
    && !string.IsNullOrWhiteSpace(developmentSubject))
{
    await userManagementBootstrapper.EnsureDevelopmentUserAsync(
        developmentSubject,
        Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_EMAIL") ?? "local-developer@example.test",
        "Local developer");
    Console.WriteLine("Development application user ready.");
}