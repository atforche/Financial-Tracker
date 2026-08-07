namespace Domain.Users;

/// <summary>
/// Category of a user-management lifecycle failure.
/// </summary>
public enum UserManagementErrorKind
{
    /// <summary>
    /// Input does not satisfy the domain contract.
    /// </summary>
    Validation,

    /// <summary>
    /// The requested state transition conflicts with current state.
    /// </summary>
    Conflict,

    /// <summary>
    /// The requested record does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The caller cannot perform the requested operation.
    /// </summary>
    Forbidden,
}