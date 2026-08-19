using System.Diagnostics.CodeAnalysis;
using Domain.Users;

namespace Data.Users;

/// <summary>
/// EF Core repository for application users.
/// </summary>
public sealed class UserRepository(DatabaseContext databaseContext) : IUserRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<User> GetAll() => databaseContext.Users
        .OrderBy(user => user.Email)
        .ThenBy(user => user.Id)
        .ToList();

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out User? user)
    {
        user = databaseContext.Users.SingleOrDefault(candidate => candidate.Id == new UserId(id))
            ?? databaseContext.Users.Local.SingleOrDefault(candidate => candidate.Id == new UserId(id));
        return user != null;
    }

    /// <inheritdoc/>
    public User? GetByGoogleSubject(string googleSubject) => databaseContext.Users
        .SingleOrDefault(user => user.GoogleSubject == googleSubject)
        ?? databaseContext.Users.Local.SingleOrDefault(user => user.GoogleSubject == googleSubject);

    /// <inheritdoc/>
    public User? GetByNormalizedEmail(string normalizedEmail) => databaseContext.Users
        .SingleOrDefault(user => user.NormalizedEmail == normalizedEmail)
        ?? databaseContext.Users.Local.SingleOrDefault(user => user.NormalizedEmail == normalizedEmail);

    /// <inheritdoc/>
    public int GetActiveAdministratorCount() => databaseContext.Users.Count(user =>
        user.Status == UserStatus.Active && user.Role == UserRole.Admin);

    /// <inheritdoc/>
    public void Add(User user) => databaseContext.Add(user);
}
