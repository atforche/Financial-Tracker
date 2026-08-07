using System.Diagnostics.CodeAnalysis;

namespace Domain.Users;

/// <summary>
/// Persistence operations for user invitations.
/// </summary>
public interface IUserInvitationRepository
{
    /// <summary>
    /// Gets all invitations, including completed history.
    /// </summary>
    IReadOnlyCollection<UserInvitation> GetAll();

    /// <summary>
    /// Attempts to retrieve an invitation by identifier.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out UserInvitation? invitation);

    /// <summary>
    /// Gets the pending invitation for a normalized email address.
    /// </summary>
    UserInvitation? GetPendingByNormalizedEmail(string normalizedEmail);

    /// <summary>
    /// Adds an invitation to the current unit of work.
    /// </summary>
    void Add(UserInvitation invitation);

    /// <summary>
    /// Conditionally claims a pending invitation in the current transaction.
    /// </summary>
    bool TryClaimPending(UserInvitation invitation, DateTime acceptedAt);
}