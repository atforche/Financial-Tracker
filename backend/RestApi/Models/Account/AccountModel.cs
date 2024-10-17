using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// REST model representing an Account
/// </summary>
public class AccountModel
{
    /// <inheritdoc cref="Domain.Entities.Account.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="Domain.Entities.Account.Name"/>
    public required string Name { get; set; }

    /// <inheritdoc cref="Domain.Entities.Account.Type"/>
    public required AccountType Type { get; set; }
}