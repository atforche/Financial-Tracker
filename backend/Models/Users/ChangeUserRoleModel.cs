namespace Models.Users;

/// <summary>
/// Request to change an application user's role.
/// </summary>
public sealed class ChangeUserRoleModel
{
    /// <summary>
    /// Role to assign to the user.
    /// </summary>
    public UserRoleModel? Role { get; init; }
}
