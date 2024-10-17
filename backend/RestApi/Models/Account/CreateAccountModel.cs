using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// REST model representing a request to create an Account
/// </summary>
public class CreateAccountModel
{
    /// <inheritdoc cref="Domain.Entities.Account.Name"/>
    public required string Name { get; set; }

    /// <inheritdoc cref="Domain.Entities.Account.Type"/>
    public required AccountType Type { get; set; }

    /// <inheritdoc cref="Domain.Entities.AccountStartingBalance.StartingBalance"/>
    public decimal StartingBalance { get; set; }
}