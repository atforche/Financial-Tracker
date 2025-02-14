using System.Text.Json.Serialization;
using Domain.Aggregates.Accounts;

namespace Rest.Models.Account;

/// <summary>
/// REST model representing an Account
/// </summary>
public class AccountModel
{
    /// <inheritdoc cref="Domain.Aggregates.EntityBase.Id"/>
    public Guid Id { get; }

    /// <inheritdoc cref="Domain.Aggregates.Accounts.Account.Name"/>
    public string Name { get; }

    /// <inheritdoc cref="Domain.Aggregates.Accounts.Account.Type"/>
    public AccountType Type { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public AccountModel(Guid id, string name, AccountType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Account entity to build this Account REST model from</param>
    public AccountModel(Domain.Aggregates.Accounts.Account account)
    {
        Id = account.Id.ExternalId;
        Name = account.Name;
        Type = account.Type;
    }
}