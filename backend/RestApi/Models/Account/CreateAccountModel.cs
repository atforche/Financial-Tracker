using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// Rest model representing a request to create an Account
/// </summary>
public class CreateAccountModel
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public required AccountType Type { get; set; }

    /// <summary>
    /// Starting Balance for this Account
    /// </summary>
    public decimal StartingBalance { get; set; }
}