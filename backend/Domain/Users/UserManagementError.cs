using Domain.Validation;

namespace Domain.Users;

/// <summary>
/// Describes a user-management lifecycle failure.
/// </summary>
public sealed record UserManagementError(
    UserManagementErrorKind Kind,
    ValidationErrorPath Path,
    string Message);
