namespace Domain.Users;

/// <summary>
/// Audit record for a user or invitation administration operation.
/// </summary>
public sealed class UserAdministrationAuditEvent : Entity<UserAdministrationAuditEventId>
{
    /// <summary>
    /// Application user that performed the operation, when applicable.
    /// </summary>
    public UserId? ActorUserId { get; private set; }

    /// <summary>
    /// Indicates that the operation was performed by an explicit system/bootstrap actor.
    /// </summary>
    public bool IsSystemActor { get; private set; }

    /// <summary>
    /// Operation that occurred.
    /// </summary>
    public UserAdministrationAction Action { get; private set; }

    /// <summary>
    /// User affected by the operation, when applicable.
    /// </summary>
    public UserId? TargetUserId { get; private set; }

    /// <summary>
    /// Invitation affected by the operation, when applicable.
    /// </summary>
    public UserInvitationId? TargetInvitationId { get; private set; }

    /// <summary>
    /// Role before the operation, when applicable.
    /// </summary>
    public UserRole? PreviousRole { get; private set; }

    /// <summary>
    /// Role after the operation, when applicable.
    /// </summary>
    public UserRole? NewRole { get; private set; }

    /// <summary>
    /// UTC time at which the operation occurred.
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal UserAdministrationAuditEvent(
        UserAdministrationAction action,
        UserId? actorUserId,
        bool isSystemActor,
        UserId? targetUserId,
        UserInvitationId? targetInvitationId,
        UserRole? previousRole,
        UserRole? newRole,
        DateTime occurredAt)
        : base(new UserAdministrationAuditEventId(Guid.NewGuid()))
    {
        Action = action;
        ActorUserId = actorUserId;
        IsSystemActor = isSystemActor;
        TargetUserId = targetUserId;
        TargetInvitationId = targetInvitationId;
        PreviousRole = previousRole;
        NewRole = newRole;
        OccurredAt = occurredAt;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    private UserAdministrationAuditEvent()
        : base()
    {
    }
}

/// <summary>
/// Value object identifying a user-management audit event.
/// </summary>
public record UserAdministrationAuditEventId : EntityId
{
    /// <summary>
    /// Constructs an audit event identifier.
    /// </summary>
    /// <param name="value">Identifier value.</param>
    public UserAdministrationAuditEventId(Guid value)
        : base(value)
    {
    }
}