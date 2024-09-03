using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// Rest model representing a request to create an account.
/// </summary>
public class CreateAccountModel
{
    /// <summary>
    /// Name for this Account.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Type for this Account.
    /// </summary>
    public required AccountType Type { get; set; }

    /// <summary>
    /// Is Active flag for this Account.
    /// </summary>
    public bool IsActive { get; set; } = true;
}