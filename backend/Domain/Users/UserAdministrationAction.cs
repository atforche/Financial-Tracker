namespace Domain.Users;

/// <summary>
/// Administrative operation recorded in the user-management audit history.
/// </summary>
public enum UserAdministrationAction
{
    /// <summary>
    /// An invitation was created.
    /// </summary>
    InvitationCreated,

    /// <summary>
    /// An invitation was revoked.
    /// </summary>
    InvitationRevoked,

    /// <summary>
    /// An invitation was accepted.
    /// </summary>
    InvitationAccepted,

    /// <summary>
    /// A user's role was changed.
    /// </summary>
    RoleChanged,

    /// <summary>
    /// A user's access was disabled.
    /// </summary>
    UserDisabled,

    /// <summary>
    /// A user's access was enabled.
    /// </summary>
    UserEnabled,

    /// <summary>
    /// A deterministic user was created for guarded local development.
    /// </summary>
    DevelopmentUserBootstrapped,
}