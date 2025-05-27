using System.Text.Json.Serialization;
using Domain.Accounts;

namespace Rest.Models.Account;

/// <summary>
/// REST model representing an Account
/// </summary>
public class AccountModel
{
    /// <inheritdoc cref="AccountId"/>
    public Guid Id { get; }

    /// <inheritdoc cref="Domain.Accounts.Account.Name"/>
    public string Name { get; }

    /// <inheritdoc cref="Domain.Accounts.Account.Type"/>
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
    public AccountModel(Domain.Accounts.Account account)
    {
        Id = account.Id.Value;
        Name = account.Name;
        Type = account.Type;
    }
}