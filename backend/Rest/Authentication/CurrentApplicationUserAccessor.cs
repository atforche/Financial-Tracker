using Domain.Users;

namespace Rest.Authentication;

/// <summary>
/// Resolves and caches the database user for the current authenticated request.
/// </summary>
internal interface ICurrentApplicationUserAccessor
{
    /// <summary>
    /// Gets the database user associated with the current provider subject.
    /// </summary>
    User? GetCurrentUser();
}

/// <summary>
/// Request-local application-user resolver.
/// </summary>
internal sealed class CurrentApplicationUserAccessor(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository) : ICurrentApplicationUserAccessor
{
    private static readonly object UserCacheKey = new();
    private static readonly object MissingUser = new();

    /// <inheritdoc/>
    public User? GetCurrentUser()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }
        if (httpContext.Items.TryGetValue(UserCacheKey, out object? cachedUser))
        {
            return ReferenceEquals(cachedUser, MissingUser) ? null : (User?)cachedUser;
        }

        string? subject = httpContext.User.FindFirst("sub")?.Value;
        User? user = string.IsNullOrWhiteSpace(subject)
            ? null
            : userRepository.GetByGoogleSubject(subject);
        httpContext.Items[UserCacheKey] = user ?? MissingUser;
        return user;
    }
}