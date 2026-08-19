namespace Domain.Users;

/// <summary>
/// Access status assigned to an application user.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The user may access the application.
    /// </summary>
    Active,

    /// <summary>
    /// The user remains in the database but may not access the application.
    /// </summary>
    Disabled,
}
