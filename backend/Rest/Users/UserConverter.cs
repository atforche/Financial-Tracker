using System.Diagnostics.CodeAnalysis;
using Domain.Users;
using Models;
using Models.Users;

namespace Rest.Users;

/// <summary>
/// Converts application users to and from REST API models.
/// </summary>
internal static class UserConverter
{
    /// <summary>
    /// Converts a domain user to its safe REST representation.
    /// </summary>
    internal static UserModel ToModel(User user) => new()
    {
        Id = user.Id.Value,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Role = ToModel(user.Role),
        Status = ToModel(user.Status),
        CreatedAt = user.CreatedAt,
        ActivatedAt = user.ActivatedAt,
        LastLoginAt = user.LastLoginAt,
        UpdatedAt = user.UpdatedAt,
    };

    /// <summary>
    /// Converts a collection of domain users to a REST collection.
    /// </summary>
    internal static CollectionModel<UserModel> ToModel(IReadOnlyCollection<User> users) => new()
    {
        Items = users.Select(ToModel).ToList(),
        TotalCount = users.Count,
    };

    /// <summary>
    /// Converts a domain role to its REST representation.
    /// </summary>
    private static UserRoleModel ToModel(UserRole role) => role switch
    {
        UserRole.Admin => UserRoleModel.Admin,
        UserRole.Standard => UserRoleModel.Standard,
        UserRole.ReadOnly => UserRoleModel.ReadOnly,
        _ => throw new InvalidOperationException($"Unrecognized user role: {role}"),
    };

    /// <summary>
    /// Converts a domain status to its REST representation.
    /// </summary>
    private static UserStatusModel ToModel(UserStatus status) => status switch
    {
        UserStatus.Active => UserStatusModel.Active,
        UserStatus.Disabled => UserStatusModel.Disabled,
        _ => throw new InvalidOperationException($"Unrecognized user status: {status}"),
    };

    /// <summary>
    /// Converts an optional REST role to a domain role.
    /// </summary>
    internal static bool TryToDomain(UserRoleModel? role, [NotNullWhen(true)] out UserRole? domainRole)
    {
        domainRole = role switch
        {
            UserRoleModel.Admin => UserRole.Admin,
            UserRoleModel.Standard => UserRole.Standard,
            UserRoleModel.ReadOnly => UserRole.ReadOnly,
            _ => null,
        };
        return domainRole.HasValue;
    }
}
