namespace Models.UserInvitations;

/// <summary>
/// Invitation lifecycle status exposed through the REST API.
/// </summary>
public enum UserInvitationStatusModel
{
    /// <summary>
    /// The invitation can be accepted.
    /// </summary>
    Pending,

    /// <summary>
    /// The invitation was accepted.
    /// </summary>
    Accepted,

    /// <summary>
    /// An administrator revoked the invitation.
    /// </summary>
    Revoked,

    /// <summary>
    /// The invitation passed its optional expiration time.
    /// </summary>
    Expired,
}