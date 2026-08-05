namespace Domain.Users;

/// <summary>
/// Email-based invitation used to provision a first application user.
/// </summary>
public sealed class UserInvitation : Entity<UserInvitationId>
{
    /// <summary>
    /// Email shown to administrators and matched during first login.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Normalized email used for matching.
    /// </summary>
    public string NormalizedEmail { get; private set; }

    /// <summary>
    /// Role assigned when the invitation is accepted.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Current invitation lifecycle status.
    /// </summary>
    public UserInvitationStatus Status { get; private set; }

    /// <summary>
    /// UTC time at which the invitation was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Optional UTC expiration time.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Administrator who created the invitation, or <see langword="null"/> for bootstrap.
    /// </summary>
    public UserId? InvitedByUserId { get; private set; }

    /// <summary>
    /// UTC time at which the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// User created by accepting the invitation.
    /// </summary>
    public UserId? AcceptedByUserId { get; private set; }

    /// <summary>
    /// UTC time at which the invitation was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Administrator who revoked the invitation.
    /// </summary>
    public UserId? RevokedByUserId { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal UserInvitation(
        string email,
        string normalizedEmail,
        UserRole role,
        DateTime? expiresAt,
        UserId? invitedByUserId,
        DateTime createdAt)
        : base(new UserInvitationId(Guid.NewGuid()))
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        Role = role;
        Status = UserInvitationStatus.Pending;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        InvitedByUserId = invitedByUserId;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    private UserInvitation()
        : base()
    {
        Email = "";
        NormalizedEmail = "";
    }

    /// <summary>
    /// Determines whether the invitation is still pending and has not expired.
    /// </summary>
    internal bool IsPending(DateTime now) => Status == UserInvitationStatus.Pending
        && (!ExpiresAt.HasValue || ExpiresAt.Value > now);

    /// <summary>
    /// Marks the invitation as accepted and records the accepting user and time.
    /// </summary>
    internal void Accept(UserId userId, DateTime acceptedAt)
    {
        Status = UserInvitationStatus.Accepted;
        AcceptedAt = acceptedAt;
        AcceptedByUserId = userId;
    }

    /// <summary>
    /// Marks the invitation as revoked and records the revoking administrator and time.
    /// </summary>
    internal void Revoke(UserId revokedByUserId, DateTime revokedAt)
    {
        Status = UserInvitationStatus.Revoked;
        RevokedAt = revokedAt;
        RevokedByUserId = revokedByUserId;
    }

    /// <summary>
    /// Marks the invitation as expired.
    /// </summary>
    internal void Expire() => Status = UserInvitationStatus.Expired;
}

/// <summary>
/// Value object identifying a user invitation.
/// </summary>
public record UserInvitationId : EntityId
{
    /// <summary>
    /// Constructs an invitation identifier.
    /// </summary>
    /// <param name="value">Identifier value.</param>
    public UserInvitationId(Guid value)
        : base(value)
    {
    }
}