using Data;
using Data.Users;
using Domain.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            _ = services.AddDbContext<DatabaseContext>(options =>
            {
                _ = options.UseSqlite($"Data Source={_databasePath}");
                _ = options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            });
        });
    }

    /// <summary>
    /// Applies the application's migrations to this test database.
    /// </summary>
    public virtual async Task InitializeDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await databaseContext.Database.MigrateAsync();
        await SeedUserAsync("test-user", "test-user@example.test", UserRole.Admin);
    }

    /// <summary>
    /// Seeds an application user for an authenticated integration test.
    /// </summary>
    public async Task SeedUserAsync(string subject, string email, UserRole role, UserStatus status = UserStatus.Active)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        UserRepository userRepository = new(databaseContext);
        if (userRepository.GetByGoogleSubject(subject) != null)
        {
            return;
        }
        if (!UserEmail.TryNormalize(email, out string? displayEmail, out string? normalizedEmail))
        {
            throw new InvalidOperationException($"The test email '{email}' is invalid.");
        }

        var user = new User(subject, displayEmail!, normalizedEmail!, subject, role, DateTime.UtcNow);
        if (status == UserStatus.Disabled)
        {
            user.Disable(DateTime.UtcNow);
        }
        userRepository.Add(user);
        _ = await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates an invitation using an existing administrator in the test database.
    /// </summary>
    public async Task CreateInvitationAsync(string actorSubject, string email, UserRole role)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        UserRepository userRepository = new(databaseContext);
        IUserInvitationRepository invitationRepository = new UserInvitationRepository(databaseContext);
        IUserAdministrationAuditEventRepository auditEventRepository = new UserAdministrationAuditEventRepository(databaseContext);
        UserManagementService userManagementService = new(userRepository, invitationRepository, auditEventRepository);
        User actor = userRepository.GetByGoogleSubject(actorSubject)
            ?? throw new InvalidOperationException($"The test actor '{actorSubject}' was not found.");
        if (!userManagementService.TryCreateInvitation(
            actor,
            email,
            role,
            null,
            DateTime.UtcNow,
            out _,
            out IEnumerable<UserManagementError> errors))
        {
            throw new InvalidOperationException(string.Join(" ", errors.Select(error => error.Message)));
        }

        _ = await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a persisted test user by provider subject.
    /// </summary>
    public Task<User?> GetUserAsync(string subject)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        return Task.FromResult(new UserRepository(databaseContext).GetByGoogleSubject(subject));
    }

    /// <summary>
    /// Gets a persisted invitation by normalized email.
    /// </summary>
    public Task<UserInvitation?> GetInvitationAsync(string email)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        if (!UserEmail.TryNormalize(email, out _, out string? normalizedEmail))
        {
            return Task.FromResult<UserInvitation?>(null);
        }
        return Task.FromResult(new UserInvitationRepository(databaseContext).GetAll()
            .SingleOrDefault(invitation => invitation.NormalizedEmail == normalizedEmail));
    }

    /// <summary>
    /// Changes a persisted test user's role through the application lifecycle method.
    /// </summary>
    public async Task ChangeUserRoleAsync(string actorSubject, string targetSubject, UserRole role)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        UserRepository userRepository = new(databaseContext);
        IUserInvitationRepository invitationRepository = new UserInvitationRepository(databaseContext);
        IUserAdministrationAuditEventRepository auditEventRepository = new UserAdministrationAuditEventRepository(databaseContext);
        UserManagementService userManagementService = new(userRepository, invitationRepository, auditEventRepository);
        User actor = userRepository.GetByGoogleSubject(actorSubject)
            ?? throw new InvalidOperationException($"The test actor '{actorSubject}' was not found.");
        User target = userRepository.GetByGoogleSubject(targetSubject)
            ?? throw new InvalidOperationException($"The test target '{targetSubject}' was not found.");
        if (!userManagementService.TryChangeRole(actor, target, role, DateTime.UtcNow, out IEnumerable<UserManagementError> errors))
        {
            throw new InvalidOperationException(string.Join(" ", errors.Select(error => error.Message)));
        }

        _ = await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Disables a persisted test user through the application lifecycle method.
    /// </summary>
    public async Task DisableUserAsync(string actorSubject, string targetSubject)
    {
        using IServiceScope scope = Services.CreateScope();
        DatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        UserRepository userRepository = new(databaseContext);
        IUserInvitationRepository invitationRepository = new UserInvitationRepository(databaseContext);
        IUserAdministrationAuditEventRepository auditEventRepository = new UserAdministrationAuditEventRepository(databaseContext);
        UserManagementService userManagementService = new(userRepository, invitationRepository, auditEventRepository);
        User actor = userRepository.GetByGoogleSubject(actorSubject)
            ?? throw new InvalidOperationException($"The test actor '{actorSubject}' was not found.");
        User target = userRepository.GetByGoogleSubject(targetSubject)
            ?? throw new InvalidOperationException($"The test target '{targetSubject}' was not found.");
        if (!userManagementService.TryDisable(actor, target, DateTime.UtcNow, out IEnumerable<UserManagementError> errors))
        {
            throw new InvalidOperationException(string.Join(" ", errors.Select(error => error.Message)));
        }

        _ = await databaseContext.SaveChangesAsync();
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