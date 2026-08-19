using System.Data;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Data.Users;

/// <summary>
/// Executes first-administrator and guarded-development bootstrap operations.
/// </summary>
public sealed class UserManagementBootstrapper(
    DatabaseContext databaseContext,
    IUserRepository userRepository,
    IUserAdministrationAuditEventRepository auditEventRepository,
    UserManagementService userManagementService)
{
    /// <summary>
    /// Creates a bootstrap administrator invitation in a serialized transaction.
    /// </summary>
    public async Task CreateBootstrapInvitationAsync(string email, CancellationToken cancellationToken = default)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await databaseContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!userManagementService.TryCreateBootstrapInvitation(email, DateTime.UtcNow, out _, out IEnumerable<UserManagementError> errors))
        {
            throw new InvalidOperationException(string.Join(" ", errors.Select(error => error.Message)));
        }

        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Creates or validates the deterministic user used by guarded local development authentication.
    /// </summary>
    public async Task EnsureDevelopmentUserAsync(
        string googleSubject,
        string email,
        string displayName,
        UserRole role = UserRole.Admin,
        CancellationToken cancellationToken = default)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await databaseContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        User? existingUser = userRepository.GetByGoogleSubject(googleSubject);
        if (existingUser != null)
        {
            if (existingUser.Status == UserStatus.Disabled)
            {
                throw new InvalidOperationException("The deterministic development user is disabled.");
            }
        }
        else
        {
            if (!UserEmail.TryNormalize(email, out string? displayEmail, out string? normalizedEmail))
            {
                throw new InvalidOperationException("The deterministic development email is invalid.");
            }

            var user = new User(googleSubject, displayEmail!, normalizedEmail!, displayName, role, DateTime.UtcNow);
            userRepository.Add(user);
            auditEventRepository.Add(new UserAdministrationAuditEvent(
                UserAdministrationAction.DevelopmentUserBootstrapped,
                null,
                true,
                user.Id,
                null,
                null,
                role,
                DateTime.UtcNow));
        }

        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
