namespace Models.Users;

/// <summary>
/// Role assigned to an application user in the REST API.
/// </summary>
public enum UserRoleModel
{
    /// <summary>
    /// Can read and modify financial data and manage users.
    /// </summary>
    Admin,

    /// <summary>
    /// Can read and modify financial data but cannot manage users.
    /// </summary>
    Standard,

    /// <summary>
    /// Can read financial data but cannot modify data or manage users.
    /// </summary>
    ReadOnly,
}