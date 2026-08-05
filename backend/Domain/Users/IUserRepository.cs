using System.Diagnostics.CodeAnalysis;

namespace Domain.Users;

/// <summary>
/// Persistence operations for application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all application users.
    /// </summary>
    IReadOnlyCollection<User> GetAll();

    /// <summary>
    /// Attempts to retrieve a user by application identifier.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out User? user);

    /// <summary>
    /// Gets a user by its immutable Google subject.
    /// </summary>
    User? GetByGoogleSubject(string googleSubject);

    /// <summary>
    /// Gets a user by its normalized email address.
    /// </summary>
    User? GetByNormalizedEmail(string normalizedEmail);

    /// <summary>
    /// Gets the number of active administrators.
    /// </summary>
    int GetActiveAdministratorCount();

    /// <summary>
    /// Adds a user to the current unit of work.
    /// </summary>
    void Add(User user);
}