using System.Diagnostics.CodeAnalysis;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Data.Users;

/// <summary>
/// EF Core repository for user invitations.
/// </summary>
public sealed class UserInvitationRepository(DatabaseContext databaseContext) : IUserInvitationRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<UserInvitation> GetAll() => databaseContext.UserInvitations
        .OrderByDescending(invitation => invitation.CreatedAt)
        .ThenBy(invitation => invitation.Id)
        .ToList();

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out UserInvitation? invitation)
    {
        invitation = databaseContext.UserInvitations.SingleOrDefault(candidate => candidate.Id == new UserInvitationId(id))
            ?? databaseContext.UserInvitations.Local.SingleOrDefault(candidate => candidate.Id == new UserInvitationId(id));
        return invitation != null;
    }

    /// <inheritdoc/>
    public UserInvitation? GetPendingByNormalizedEmail(string normalizedEmail) => databaseContext.UserInvitations
        .SingleOrDefault(invitation => invitation.NormalizedEmail == normalizedEmail
            && invitation.Status == UserInvitationStatus.Pending)
        ?? databaseContext.UserInvitations.Local.SingleOrDefault(invitation => invitation.NormalizedEmail == normalizedEmail
            && invitation.Status == UserInvitationStatus.Pending);

    /// <inheritdoc/>
    public void Add(UserInvitation invitation) => databaseContext.Add(invitation);

    /// <inheritdoc/>
    public bool TryClaimPending(UserInvitation invitation, DateTime acceptedAt)
    {
        int affectedRows = databaseContext.UserInvitations
            .Where(candidate => candidate.Id == invitation.Id && candidate.Status == UserInvitationStatus.Pending)
            .ExecuteUpdate(setters => setters
                .SetProperty(candidate => candidate.Status, UserInvitationStatus.Accepted)
                .SetProperty(candidate => candidate.AcceptedAt, acceptedAt));
        return affectedRows == 1;
    }
}
