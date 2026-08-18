using Domain.Users;
using Models;
using Models.UserInvitations;
namespace Rest.UserInvitations;

/// <summary>
/// Converts user invitations to REST API models.
/// </summary>
internal static class UserInvitationConverter
{
    /// <summary>
    /// Converts a domain invitation to its REST representation.
    /// </summary>
    internal static UserInvitationModel ToModel(UserInvitation invitation) => new()
    {
        Id = invitation.Id.Value,
        Email = invitation.Email,
        Role = ToModelRole(invitation.Role),
        Status = invitation.Status switch
        {
            UserInvitationStatus.Pending => UserInvitationStatusModel.Pending,
            UserInvitationStatus.Accepted => UserInvitationStatusModel.Accepted,
            UserInvitationStatus.Revoked => UserInvitationStatusModel.Revoked,
            UserInvitationStatus.Expired => UserInvitationStatusModel.Expired,
            _ => throw new InvalidOperationException($"Unrecognized invitation status: {invitation.Status}"),
        },
        CreatedAt = invitation.CreatedAt,
        ExpiresAt = invitation.ExpiresAt,
        InvitedByUserId = invitation.InvitedByUserId?.Value,
        AcceptedAt = invitation.AcceptedAt,
        AcceptedByUserId = invitation.AcceptedByUserId?.Value,
        RevokedAt = invitation.RevokedAt,
        RevokedByUserId = invitation.RevokedByUserId?.Value,
    };

    /// <summary>
    /// Converts a collection of domain invitations to a REST collection.
    /// </summary>
    internal static CollectionModel<UserInvitationModel> ToModel(IReadOnlyCollection<UserInvitation> invitations) => new()
    {
        Items = invitations.Select(ToModel).ToList(),
        TotalCount = invitations.Count,
    };

    private static Models.Users.UserRoleModel ToModelRole(UserRole role) => role switch
    {
        UserRole.Admin => Models.Users.UserRoleModel.Admin,
        UserRole.Standard => Models.Users.UserRoleModel.Standard,
        UserRole.ReadOnly => Models.Users.UserRoleModel.ReadOnly,
        _ => throw new InvalidOperationException($"Unrecognized user role: {role}"),
    };
}
