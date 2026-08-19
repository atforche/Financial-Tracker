namespace Domain.Users;

/// <summary>
/// Application-owned identity and authorization state for a Google subject.
/// </summary>
public sealed class User : Entity<UserId>
{
    /// <summary>
    /// Immutable Google subject used as the durable identity key.
    /// </summary>
    public string GoogleSubject { get; private set; }

    /// <summary>
    /// Current display email supplied by the identity provider.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Normalized email used for invitation lookup.
    /// </summary>
    public string NormalizedEmail { get; private set; }

    /// <summary>
    /// Optional display name supplied by the identity provider.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Authorization role assigned to the user.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Whether the user is allowed to access the application.
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    /// UTC time at which the user record was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC time at which the invitation was accepted.
    /// </summary>
    public DateTime ActivatedAt { get; private set; }

    /// <summary>
    /// UTC time of the most recent successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// UTC time at which the record was most recently changed.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal User(
        string googleSubject,
        string email,
        string normalizedEmail,
        string? displayName,
        UserRole role,
        DateTime createdAt)
        : base(new UserId(Guid.NewGuid()))
    {
        GoogleSubject = googleSubject;
        Email = email;
        NormalizedEmail = normalizedEmail;
        DisplayName = displayName;
        Role = role;
        Status = UserStatus.Active;
        CreatedAt = createdAt;
        ActivatedAt = createdAt;
        LastLoginAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    private User()
        : base()
    {
        GoogleSubject = "";
        Email = "";
        NormalizedEmail = "";
    }

    /// <summary>
    /// Updates the user profile with the latest information from the identity provider.
    /// </summary>
    internal void RefreshProviderProfile(string email, string normalizedEmail, string? displayName, DateTime loginAt)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        DisplayName = displayName;
        LastLoginAt = loginAt;
        UpdatedAt = loginAt;
    }

    /// <summary>
    /// Updates the user role and records the time of the change.
    /// </summary>
    internal void ChangeRole(UserRole role, DateTime changedAt)
    {
        Role = role;
        UpdatedAt = changedAt;
    }

    /// <summary>
    /// Disables the user and records the time of the change.
    /// </summary>
    internal void Disable(DateTime changedAt)
    {
        Status = UserStatus.Disabled;
        UpdatedAt = changedAt;
    }

    /// <summary>
    /// Enables the user and records the time of the change.
    /// </summary>
    internal void Enable(DateTime changedAt)
    {
        Status = UserStatus.Active;
        UpdatedAt = changedAt;
    }
}

/// <summary>
/// Value object identifying an application user.
/// </summary>
public record UserId : EntityId
{
    /// <summary>
    /// Constructs a user identifier.
    /// </summary>
    /// <param name="value">Identifier value.</param>
    public UserId(Guid value)
        : base(value)
    {
    }
}
