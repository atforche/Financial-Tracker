using Domain.Validation;

namespace Domain.Users;

/// <summary>
/// Owns user, invitation, and administration-audit lifecycle rules.
/// </summary>
public sealed class UserManagementService(
    IUserRepository userRepository,
    IUserInvitationRepository invitationRepository,
    IUserAdministrationAuditEventRepository auditEventRepository)
{
    /// <summary>
    /// Creates an invitation on behalf of an active administrator.
    /// </summary>
    public bool TryCreateInvitation(
        User actor,
        string? email,
        UserRole role,
        DateTime? expiresAt,
        DateTime now,
        out UserInvitation? invitation,
        out IEnumerable<UserManagementError> errors)
    {
        invitation = null;
        errors = ValidateAdministrator(actor);
        if (!TryNormalizeEmail(email, out string displayEmail, out string normalizedEmail, out UserManagementError? emailError))
        {
            errors = errors.Append(emailError!);
        }
        if (!Enum.IsDefined(role))
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Validation,
                new ValidationErrorPath(nameof(role)),
                "The invitation role is invalid."));
        }
        if (expiresAt.HasValue && expiresAt.Value <= now)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Validation,
                new ValidationErrorPath(nameof(expiresAt)),
                "The invitation expiration must be in the future."));
        }
        if (errors.Any())
        {
            return false;
        }

        if (invitationRepository.GetPendingByNormalizedEmail(normalizedEmail) != null)
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Conflict,
                new ValidationErrorPath(nameof(email)),
                "A pending invitation already exists for this email address.")];
            return false;
        }

        invitation = new UserInvitation(displayEmail, normalizedEmail, role, expiresAt, actor.Id, now);
        invitationRepository.Add(invitation);
        AddAudit(UserAdministrationAction.InvitationCreated, actor, null, invitation, null, role, now);
        return true;
    }

    /// <summary>
    /// Creates the first administrator invitation in an uninitialized system.
    /// </summary>
    public bool TryCreateBootstrapInvitation(
        string? email,
        DateTime now,
        out UserInvitation? invitation,
        out IEnumerable<UserManagementError> errors)
    {
        invitation = null;
        errors = [];
        if (userRepository.GetActiveAdministratorCount() > 0)
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "An active administrator already exists.")];
            return false;
        }
        if (!TryNormalizeEmail(email, out string displayEmail, out string normalizedEmail, out UserManagementError? emailError))
        {
            errors = [emailError!];
            return false;
        }
        if (invitationRepository.GetPendingByNormalizedEmail(normalizedEmail) != null)
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Conflict,
                new ValidationErrorPath(nameof(email)),
                "A pending invitation already exists for this email address.")];
            return false;
        }

        invitation = new UserInvitation(displayEmail, normalizedEmail, UserRole.Admin, null, null, now);
        invitationRepository.Add(invitation);
        AddAudit(UserAdministrationAction.InvitationCreated, null, null, invitation, null, UserRole.Admin, now);
        return true;
    }

    /// <summary>
    /// Resolves an authenticated provider identity and accepts a matching invitation when needed.
    /// </summary>
    public bool TryResolveIdentity(
        string? googleSubject,
        string? email,
        bool emailVerified,
        string? displayName,
        DateTime now,
        out User? user,
        out IEnumerable<UserManagementError> errors)
    {
        user = null;
        errors = [];
        if (string.IsNullOrWhiteSpace(googleSubject)
            || googleSubject.Trim().Length > 255
            || !emailVerified)
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "The authenticated identity could not be provisioned.")];
            return false;
        }

        string subject = googleSubject.Trim();
        user = userRepository.GetByGoogleSubject(subject);
        if (user != null)
        {
            if (user.Status == UserStatus.Disabled)
            {
                user = null;
                errors = [new UserManagementError(
                    UserManagementErrorKind.Forbidden,
                    ValidationErrorPath.Empty,
                    "The authenticated identity could not be provisioned.")];
                return false;
            }
            if (!TryNormalizeEmail(email, out string currentEmail, out string currentNormalizedEmail, out _))
            {
                user = null;
                errors = [new UserManagementError(
                    UserManagementErrorKind.Forbidden,
                    ValidationErrorPath.Empty,
                    "The authenticated identity could not be provisioned.")];
                return false;
            }
            User? conflictingUser = userRepository.GetByNormalizedEmail(currentNormalizedEmail);
            if (conflictingUser != null && conflictingUser.Id != user.Id)
            {
                user = null;
                errors = [new UserManagementError(
                    UserManagementErrorKind.Forbidden,
                    ValidationErrorPath.Empty,
                    "The authenticated identity could not be provisioned.")];
                return false;
            }
            user.RefreshProviderProfile(currentEmail, currentNormalizedEmail, displayName, now);
            return true;
        }

        if (!TryNormalizeEmail(email, out string displayEmail, out string normalizedEmail, out _))
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "The authenticated identity could not be provisioned.")];
            return false;
        }

        if (userRepository.GetByNormalizedEmail(normalizedEmail) != null)
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "The authenticated identity could not be provisioned.")];
            return false;
        }

        UserInvitation? invitation = invitationRepository.GetPendingByNormalizedEmail(normalizedEmail);
        if (invitation == null || !invitation.IsPending(now))
        {
            errors = [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "The authenticated identity could not be provisioned.")];
            return false;
        }

        user = new User(subject, displayEmail, normalizedEmail, displayName, invitation.Role, now);
        userRepository.Add(user);
        if (!invitationRepository.TryClaimPending(invitation, now))
        {
            user = null;
            errors = [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "The authenticated identity could not be provisioned.")];
            return false;
        }
        invitation.Accept(user.Id, now);
        AddAudit(UserAdministrationAction.InvitationAccepted, user, user, invitation, null, invitation.Role, now);
        return true;
    }

    /// <summary>
    /// Revokes a pending invitation on behalf of an active administrator.
    /// </summary>
    public bool TryRevokeInvitation(
        User actor,
        UserInvitation invitation,
        DateTime now,
        out IEnumerable<UserManagementError> errors)
    {
        errors = ValidateAdministrator(actor);
        if (invitation.Status != UserInvitationStatus.Pending)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "Only a pending invitation can be revoked."));
        }
        if (errors.Any())
        {
            return false;
        }

        invitation.Revoke(actor.Id, now);
        AddAudit(UserAdministrationAction.InvitationRevoked, actor, null, invitation, null, null, now);
        return true;
    }

    /// <summary>
    /// Changes a user's role while preserving the active-administrator invariant.
    /// </summary>
    public bool TryChangeRole(
        User actor,
        User target,
        UserRole newRole,
        DateTime now,
        out IEnumerable<UserManagementError> errors)
    {
        errors = ValidateAdministrator(actor);
        if (!Enum.IsDefined(newRole))
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Validation,
                new ValidationErrorPath(nameof(newRole)),
                "The user role is invalid."));
        }
        if (target.Role == newRole)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "The user already has the requested role."));
        }
        if (target.Role == UserRole.Admin && newRole != UserRole.Admin
            && userRepository.GetActiveAdministratorCount() <= 1)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "The final active administrator cannot be demoted."));
        }
        if (errors.Any())
        {
            return false;
        }

        UserRole previousRole = target.Role;
        target.ChangeRole(newRole, now);
        AddAudit(UserAdministrationAction.RoleChanged, actor, target, null, previousRole, newRole, now);
        return true;
    }

    /// <summary>
    /// Disables a user while preserving the active-administrator invariant.
    /// </summary>
    public bool TryDisable(User actor, User target, DateTime now, out IEnumerable<UserManagementError> errors)
    {
        errors = ValidateAdministrator(actor);
        if (target.Status == UserStatus.Disabled)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "The user is already disabled."));
        }
        if (target.Role == UserRole.Admin && userRepository.GetActiveAdministratorCount() <= 1)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "The final active administrator cannot be disabled."));
        }
        if (errors.Any())
        {
            return false;
        }

        target.Disable(now);
        AddAudit(UserAdministrationAction.UserDisabled, actor, target, null, null, null, now);
        return true;
    }

    /// <summary>
    /// Enables a disabled user on behalf of an active administrator.
    /// </summary>
    public bool TryEnable(User actor, User target, DateTime now, out IEnumerable<UserManagementError> errors)
    {
        errors = ValidateAdministrator(actor);
        if (target.Status == UserStatus.Active)
        {
            errors = errors.Append(new UserManagementError(
                UserManagementErrorKind.Conflict,
                ValidationErrorPath.Empty,
                "The user is already active."));
        }
        if (errors.Any())
        {
            return false;
        }

        target.Enable(now);
        AddAudit(UserAdministrationAction.UserEnabled, actor, target, null, null, null, now);
        return true;
    }

    /// <summary>
    /// Validates that the actor is an active administrator.
    /// </summary>
    private static IEnumerable<UserManagementError> ValidateAdministrator(User actor) =>
        actor.Status != UserStatus.Active || actor.Role != UserRole.Admin
            ? [new UserManagementError(
                UserManagementErrorKind.Forbidden,
                ValidationErrorPath.Empty,
                "An active administrator is required for this operation.")]
            : [];

    /// <summary>
    /// Normalizes and validates an email address for use in a user invitation.
    /// </summary>
    private static bool TryNormalizeEmail(
        string? email,
        out string displayEmail,
        out string normalizedEmail,
        out UserManagementError? error)
    {
        if (UserEmail.TryNormalize(email, out string? validDisplayEmail, out string? validNormalizedEmail))
        {
            displayEmail = validDisplayEmail!;
            normalizedEmail = validNormalizedEmail!;
            error = null;
            return true;
        }

        displayEmail = "";
        normalizedEmail = "";
        error = new UserManagementError(
            UserManagementErrorKind.Validation,
            new ValidationErrorPath("email"),
            "A valid email address is required.");
        return false;
    }

    /// <summary>
    /// Adds an audit event to the repository.
    /// </summary>
    private void AddAudit(
        UserAdministrationAction action,
        User? actor,
        User? targetUser,
        UserInvitation? targetInvitation,
        UserRole? previousRole,
        UserRole? newRole,
        DateTime occurredAt) =>
        auditEventRepository.Add(new UserAdministrationAuditEvent(
            action,
            actor?.Id,
            actor == null,
            targetUser?.Id,
            targetInvitation?.Id,
            previousRole,
            newRole,
            occurredAt));
}