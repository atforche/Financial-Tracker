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
    Console.WriteLine("Bootstrap administrator invitation is ready.");
}

string? authenticationMode = Environment.GetEnvironmentVariable("AUTH_MODE");
string? developmentSubject = Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_SUBJECT");
if (string.Equals(authenticationMode, "development", StringComparison.OrdinalIgnoreCase)
    && !string.IsNullOrWhiteSpace(developmentSubject))
{
    string developmentRoleValue = Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_ROLE")
        ?? nameof(UserRole.Admin);
    if (!Enum.TryParse(developmentRoleValue, true, out UserRole developmentRole))
    {
        throw new InvalidOperationException("DEVELOPMENT_AUTH_ROLE must name a UserRole.");
    }

    string[] readOnlySubjects = (Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS")
        ?? (string.Equals(developmentSubject, "local-developer", StringComparison.Ordinal)
            ? "local-read-only"
            : ""))
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    string[] additionalSubjects = (Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS")
        ?? (string.Equals(developmentSubject, "local-developer", StringComparison.Ordinal)
            ? "local-standard,local-read-only"
            : ""))
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var developmentUsers = new List<(string Subject, string Email, string DisplayName, UserRole Role)>
    {
        (
            developmentSubject,
            Environment.GetEnvironmentVariable("DEVELOPMENT_AUTH_EMAIL") ?? "local-developer@example.test",
            $"Local {developmentRole} user",
            developmentRole),
    };
    developmentUsers.AddRange(additionalSubjects
        .Where(subject => !string.Equals(subject, developmentSubject, StringComparison.Ordinal))
        .Distinct(StringComparer.Ordinal)
        .Select(subject => (
            subject,
            $"{subject}@example.test",
            readOnlySubjects.Contains(subject, StringComparer.Ordinal) ? "Local read-only user" : "Local standard user",
            readOnlySubjects.Contains(subject, StringComparer.Ordinal) ? UserRole.ReadOnly : UserRole.Standard)));

    foreach ((string subject, string email, string displayName, UserRole role) in developmentUsers)
    {
        await userManagementBootstrapper.EnsureDevelopmentUserAsync(subject, email, displayName, role);
    }

    Console.WriteLine("Development application users ready.");
}