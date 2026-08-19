namespace Models.Users;

/// <summary>
/// Access status assigned to an application user in the REST API.
/// </summary>
public enum UserStatusModel
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
