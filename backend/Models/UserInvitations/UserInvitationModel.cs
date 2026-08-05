using Models.Users;

namespace Models.UserInvitations;

/// <summary>
/// User invitation details exposed through the REST API.
/// </summary>
public sealed class UserInvitationModel
{
    /// <summary>
    /// Invitation identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Invited email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role assigned when the invitation is accepted.
    /// </summary>
    public required UserRoleModel Role { get; init; }

    /// <summary>
    /// Current invitation status.
    /// </summary>
    public required UserInvitationStatusModel Status { get; init; }

    /// <summary>
    /// UTC time at which the invitation was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Optional UTC expiration time.
    /// </summary>
    public required DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// User that created the invitation, when applicable.
    /// </summary>
    public required Guid? InvitedByUserId { get; init; }

    /// <summary>
    /// UTC time at which the invitation was accepted.
    /// </summary>
    public required DateTime? AcceptedAt { get; init; }

    /// <summary>
    /// User created by accepting the invitation.
    /// </summary>
    public required Guid? AcceptedByUserId { get; init; }

    /// <summary>
    /// UTC time at which the invitation was revoked.
    /// </summary>
    public required DateTime? RevokedAt { get; init; }

    /// <summary>
    /// User that revoked the invitation.
    /// </summary>
    public required Guid? RevokedByUserId { get; init; }
}