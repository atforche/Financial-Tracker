using Models.Users;

namespace Models.UserInvitations;

/// <summary>
/// Request to create a user invitation.
/// </summary>
public sealed class CreateUserInvitationModel
{
    /// <summary>
    /// Email address to invite.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Role assigned when the invitation is accepted.
    /// </summary>
    public UserRoleModel? Role { get; init; }

    /// <summary>
    /// Optional UTC expiration time.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}
