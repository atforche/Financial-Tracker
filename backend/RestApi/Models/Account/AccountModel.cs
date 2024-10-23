using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// REST model representing an Account
/// </summary>
public class AccountModel
{
    /// <inheritdoc cref="Domain.Entities.Account.Id"/>
    public required Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Entities.Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Domain.Entities.Account.Type"/>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Converts the Account domain entity into an Account REST model
    /// </summary>
    /// <param name="account">Account domain entity to be converted</param>
    /// <returns>The converted Account REST model</returns>
    internal static AccountModel ConvertEntityToModel(Domain.Entities.Account account) =>
        new AccountModel
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
        };
}