using System.Data.Common;
using Data;
using Data.Users;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Tests.Users;

/// <summary>
/// Verifies the persisted user-management lifecycle and its authorization invariants.
/// </summary>
public sealed class UserManagementLifecycleTests
{
    /// <summary>
    /// Verifies bootstrap invitation persistence, acceptance, and audit history.
    /// </summary>
    [Fact]
    public async Task BootstrapInvitationCanBeAcceptedAndRemainsAudited()
    {
        await using UserManagementTestStore store = await UserManagementTestStore.CreateAsync();
        DateTime now = new(2026, 8, 5, 17, 30, 0, DateTimeKind.Utc);

        Assert.True(store.Service.TryCreateBootstrapInvitation(
            "  Owner@Example.COM ",
            now,
            out UserInvitation? invitation,
            out IEnumerable<UserManagementError> creationErrors));
        Assert.Empty(creationErrors);
        _ = await store.Context.SaveChangesAsync();

        UserInvitation persistedInvitation = Assert.Single(store.Invitations.GetAll());
        Assert.Equal("Owner@Example.COM", persistedInvitation.Email);
        Assert.Equal("owner@example.com", persistedInvitation.NormalizedEmail);
        Assert.Equal(UserRole.Admin, persistedInvitation.Role);
        Assert.Equal(UserInvitationStatus.Pending, persistedInvitation.Status);
        Assert.Null(persistedInvitation.ExpiresAt);

        Assert.True(store.Service.TryResolveIdentity(
            "google-owner",
            "owner@example.com",
            true,
            "Owner",
            now.AddMinutes(1),
            out User? user,
            out IEnumerable<UserManagementError> acceptanceErrors));
        Assert.Empty(acceptanceErrors);
        _ = await store.Context.SaveChangesAsync();

        User persistedUser = Assert.IsType<User>(user);
        Assert.Equal(UserRole.Admin, persistedUser.Role);
        Assert.Equal(UserStatus.Active, persistedUser.Status);
        Assert.Equal(UserInvitationStatus.Accepted, persistedInvitation.Status);
        Assert.Equal(persistedUser.Id, persistedInvitation.AcceptedByUserId);
        Assert.Contains(store.Audits.GetAll(), audit =>
            audit.Action == UserAdministrationAction.InvitationCreated
            && audit.IsSystemActor
            && audit.TargetInvitationId == persistedInvitation.Id);
        Assert.Contains(store.Audits.GetAll(), audit =>
            audit.Action == UserAdministrationAction.InvitationAccepted
            && audit.ActorUserId == persistedUser.Id
            && audit.TargetUserId == persistedUser.Id);
        Assert.False(store.Service.TryCreateBootstrapInvitation(
            "second-owner@example.com",
            now.AddMinutes(2),
            out _,
            out IEnumerable<UserManagementError> secondBootstrapErrors));
        Assert.Contains(secondBootstrapErrors, error => error.Kind == UserManagementErrorKind.Conflict);
    }

    /// <summary>
    /// Verifies pending invitation uniqueness after email normalization.
    /// </summary>
    [Fact]
    public async Task PendingInvitationsAreUniqueByNormalizedEmail()
    {
        await using UserManagementTestStore store = await UserManagementTestStore.CreateAsync();
        await store.BootstrapAdminAsync();
        User actor = store.GetUserBySubject("local-admin");
        DateTime now = new(2026, 8, 5, 17, 30, 0, DateTimeKind.Utc);

        Assert.False(store.Service.TryCreateInvitation(
            actor,
            "not-an-email",
            UserRole.Standard,
            null,
            now,
            out _,
            out IEnumerable<UserManagementError> invalidEmailErrors));
        Assert.Contains(invalidEmailErrors, error => error.Kind == UserManagementErrorKind.Validation);

        Assert.True(store.Service.TryCreateInvitation(
            actor,
            "Person@Example.com",
            UserRole.Standard,
            null,
            now,
            out _,
            out IEnumerable<UserManagementError> firstErrors));
        Assert.Empty(firstErrors);
        _ = await store.Context.SaveChangesAsync();

        Assert.False(store.Service.TryCreateInvitation(
            actor,
            " person@example.com ",
            UserRole.ReadOnly,
            null,
            now.AddMinutes(1),
            out _,
            out IEnumerable<UserManagementError> duplicateErrors));
        Assert.Contains(duplicateErrors, error => error.Kind == UserManagementErrorKind.Conflict);
        _ = Assert.Single(store.Invitations.GetAll());
    }

    /// <summary>
    /// Verifies role and status mutations preserve the final-administrator invariant.
    /// </summary>
    [Fact]
    public async Task AdministrativeLifecyclePreservesFinalActiveAdministrator()
    {
        await using UserManagementTestStore store = await UserManagementTestStore.CreateAsync();
        await store.BootstrapAdminAsync();
        User actor = store.GetUserBySubject("local-admin");
        DateTime now = new(2026, 8, 5, 17, 30, 0, DateTimeKind.Utc);

        Assert.True(store.Service.TryCreateInvitation(
            actor,
            "reader@example.com",
            UserRole.ReadOnly,
            null,
            now,
            out _,
            out IEnumerable<UserManagementError> creationErrors));
        Assert.Empty(creationErrors);
        _ = await store.Context.SaveChangesAsync();

        Assert.True(store.Service.TryResolveIdentity(
            "google-reader",
            "reader@example.com",
            true,
            "Reader",
            now.AddMinutes(1),
            out User? resolvedReader,
            out IEnumerable<UserManagementError> acceptanceErrors));
        Assert.Empty(acceptanceErrors);
        _ = await store.Context.SaveChangesAsync();
        User reader = Assert.IsType<User>(resolvedReader);

        Assert.True(store.Service.TryChangeRole(actor, reader, UserRole.Standard, now.AddMinutes(2), out _));
        _ = await store.Context.SaveChangesAsync();
        Assert.True(store.Service.TryDisable(actor, reader, now.AddMinutes(3), out _));
        _ = await store.Context.SaveChangesAsync();
        Assert.True(store.Service.TryEnable(actor, reader, now.AddMinutes(4), out _));
        _ = await store.Context.SaveChangesAsync();

        Assert.Equal(UserRole.Standard, reader.Role);
        Assert.Equal(UserStatus.Active, reader.Status);
        Assert.False(store.Service.TryChangeRole(actor, actor, UserRole.Standard, now.AddMinutes(5), out IEnumerable<UserManagementError> demotionErrors));
        Assert.Contains(demotionErrors, error => error.Kind == UserManagementErrorKind.Conflict);
        Assert.False(store.Service.TryDisable(actor, actor, now.AddMinutes(5), out IEnumerable<UserManagementError> disableErrors));
        Assert.Contains(disableErrors, error => error.Kind == UserManagementErrorKind.Conflict);

        Assert.True(store.Service.TryCreateInvitation(
            actor,
            "revocable@example.com",
            UserRole.ReadOnly,
            null,
            now.AddMinutes(6),
            out UserInvitation? revocableInvitation,
            out IEnumerable<UserManagementError> revocableCreationErrors));
        Assert.Empty(revocableCreationErrors);
        _ = await store.Context.SaveChangesAsync();
        UserInvitation persistedRevocableInvitation = Assert.IsType<UserInvitation>(revocableInvitation);
        Assert.True(store.Service.TryRevokeInvitation(actor, persistedRevocableInvitation, now.AddMinutes(7), out _));
        _ = await store.Context.SaveChangesAsync();
        Assert.Equal(UserInvitationStatus.Revoked, persistedRevocableInvitation.Status);
        Assert.False(store.Service.TryRevokeInvitation(actor, persistedRevocableInvitation, now.AddMinutes(8), out IEnumerable<UserManagementError> repeatedRevokeErrors));
        Assert.Contains(repeatedRevokeErrors, error => error.Kind == UserManagementErrorKind.Conflict);
        Assert.False(store.Service.TryResolveIdentity(
            "google-revoked",
            "revocable@example.com",
            true,
            "Revoked",
            now.AddMinutes(8),
            out _,
            out IEnumerable<UserManagementError> revokedAcceptanceErrors));
        Assert.Contains(revokedAcceptanceErrors, error => error.Kind == UserManagementErrorKind.Forbidden);
        Assert.Contains(store.Audits.GetAll(), audit => audit.Action == UserAdministrationAction.RoleChanged);
        Assert.Contains(store.Audits.GetAll(), audit => audit.Action == UserAdministrationAction.UserDisabled);
        Assert.Contains(store.Audits.GetAll(), audit => audit.Action == UserAdministrationAction.UserEnabled);
        Assert.Contains(store.Audits.GetAll(), audit => audit.Action == UserAdministrationAction.InvitationRevoked);
    }

    /// <summary>
    /// Verifies the database rejects duplicate immutable provider subjects.
    /// </summary>
    [Fact]
    public async Task DatabaseRejectsDuplicateGoogleSubjects()
    {
        await using UserManagementTestStore store = await UserManagementTestStore.CreateAsync();
        DateTime now = new(2026, 8, 5, 17, 30, 0, DateTimeKind.Utc);
        User first = new(
            "duplicate-subject",
            "first@example.com",
            "first@example.com",
            "First",
            UserRole.Standard,
            now);
        User duplicate = new(
            "duplicate-subject",
            "second@example.com",
            "second@example.com",
            "Second",
            UserRole.Standard,
            now);
        store.Users.Add(first);
        store.Users.Add(duplicate);

        _ = await Assert.ThrowsAsync<DbUpdateException>(() => store.Context.SaveChangesAsync());
    }

    /// <summary>
    /// Verifies the migration creates the user-management tables and partial uniqueness index.
    /// </summary>
    [Fact]
    public async Task MigrationCreatesUserManagementSchema()
    {
        await using UserManagementTestStore store = await UserManagementTestStore.CreateAsync("20260805090000_AddFinancialInstitution");
        var existingAccountId = Guid.NewGuid();
        _ = await store.Context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"Accounts\" (\"Id\", \"Name\", \"Type\") VALUES ({0}, {1}, {2})",
            existingAccountId,
            "Existing checking",
            "Checking");
        await store.Context.Database.MigrateAsync();

        Assert.Contains(
            "20260805171721_AddUserManagement",
            await store.Context.Database.GetAppliedMigrationsAsync());

        await store.Context.Database.OpenConnectionAsync();
        try
        {
            IReadOnlyCollection<string> tables = await ReadFirstColumnAsync(
                store.Context.Database.GetDbConnection());
            Assert.Contains("Users", tables);
            Assert.Contains("UserInvitations", tables);
            Assert.Contains("UserAdministrationAuditEvents", tables);

            IReadOnlyCollection<string> invitationIndexes = await ReadSecondColumnAsync(
                store.Context.Database.GetDbConnection());
            Assert.Contains("IX_UserInvitations_NormalizedEmail_Status", invitationIndexes);

            await using DbCommand accountCommand = store.Context.Database.GetDbConnection().CreateCommand();
            accountCommand.CommandText = "SELECT \"Name\" FROM \"Accounts\" WHERE \"Id\" = $id";
            DbParameter accountIdParameter = accountCommand.CreateParameter();
            accountIdParameter.ParameterName = "$id";
            accountIdParameter.Value = existingAccountId;
            _ = accountCommand.Parameters.Add(accountIdParameter);
            Assert.Equal("Existing checking", await accountCommand.ExecuteScalarAsync());
        }
        finally
        {
            await store.Context.Database.CloseConnectionAsync();
        }
    }

    private static async Task<IReadOnlyCollection<string>> ReadFirstColumnAsync(DbConnection connection)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        List<string> values = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            values.Add(reader.GetString(0));
        }
        return values;
    }

    private static async Task<IReadOnlyCollection<string>> ReadSecondColumnAsync(DbConnection connection)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA index_list('UserInvitations')";
        List<string> values = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            values.Add(reader.GetString(1));
        }
        return values;
    }
}

/// <summary>
/// Provides an isolated migrated SQLite database and user-management services for tests.
/// </summary>
internal sealed class UserManagementTestStore : IAsyncDisposable
{
    private readonly string _databasePath;

    private UserManagementTestStore(
        string databasePath,
        DatabaseContext context,
        IUserRepository users,
        IUserInvitationRepository invitations,
        IUserAdministrationAuditEventRepository audits,
        UserManagementService service,
        UserManagementBootstrapper bootstrapper)
    {
        _databasePath = databasePath;
        Context = context;
        Users = users;
        Invitations = invitations;
        Audits = audits;
        Service = service;
        Bootstrapper = bootstrapper;
    }

    public DatabaseContext Context { get; }

    public IUserRepository Users { get; }

    public IUserInvitationRepository Invitations { get; }

    public IUserAdministrationAuditEventRepository Audits { get; }

    public UserManagementService Service { get; }

    private UserManagementBootstrapper Bootstrapper { get; }

    public static async Task<UserManagementTestStore> CreateAsync(string? targetMigration = null)
    {
        string path = Path.Combine(Path.GetTempPath(), $"financial-tracker-user-tests-{Guid.NewGuid():N}.db");
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        DatabaseContext context = new(options);
        if (targetMigration is null)
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.MigrateAsync(targetMigration);
        }

        IUserRepository users = new UserRepository(context);
        IUserInvitationRepository invitations = new UserInvitationRepository(context);
        IUserAdministrationAuditEventRepository audits = new UserAdministrationAuditEventRepository(context);
        UserManagementService service = new(users, invitations, audits);
        UserManagementBootstrapper bootstrapper = new(context, users, audits, service);
        return new UserManagementTestStore(path, context, users, invitations, audits, service, bootstrapper);
    }

    public Task BootstrapAdminAsync() => Bootstrapper.EnsureDevelopmentUserAsync("local-admin", "admin@example.com", "Local Admin");

    public User GetUserBySubject(string subject) => Users.GetByGoogleSubject(subject)
        ?? throw new InvalidOperationException($"Expected user '{subject}' was not created.");

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}