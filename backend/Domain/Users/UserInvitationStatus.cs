namespace Domain.Users;

/// <summary>
/// Lifecycle status of an invitation.
/// </summary>
public enum UserInvitationStatus
{
    /// <summary>
    /// The invitation can be accepted.
    /// </summary>
    Pending,

    /// <summary>
    /// The invitation was accepted by a newly provisioned user.
    /// </summary>
    Accepted,

    /// <summary>
    /// An administrator revoked the invitation before acceptance.
    /// </summary>
    Revoked,

    /// <summary>
    /// The invitation passed its optional expiration time.
    /// </summary>
    Expired,
}
