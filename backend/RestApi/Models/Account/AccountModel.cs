using Domain.Entities;

namespace RestApi.Models.Account;

/// <summary>
/// Rest model representing an Account.
/// </summary>
public class AccountModel
{
    /// <summary>
    /// Id for this Account.
    /// </summary>
    public required Guid Id { get; set; }

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
    public required bool IsActive { get; set; }
}