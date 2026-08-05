namespace Models.Users;

/// <summary>
/// Application user details exposed through the REST API.
/// </summary>
public sealed class UserModel
{
    /// <summary>
    /// Application user identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Current provider email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Optional provider display name.
    /// </summary>
    public required string? DisplayName { get; init; }

    /// <summary>
    /// User role.
    /// </summary>
    public required UserRoleModel Role { get; init; }

    /// <summary>
    /// User access status.
    /// </summary>
    public required UserStatusModel Status { get; init; }

    /// <summary>
    /// UTC time at which the user was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// UTC time at which the user was activated.
    /// </summary>
    public required DateTime ActivatedAt { get; init; }

    /// <summary>
    /// UTC time of the most recent successful login.
    /// </summary>
    public required DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// UTC time at which the user record was most recently changed.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}