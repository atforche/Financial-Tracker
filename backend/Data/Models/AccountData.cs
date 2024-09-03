using Domain.Entities;

namespace Data.Models;

/// <summary>
/// Data model representing an Account
/// </summary>
public class AccountData
{
    /// <summary>
    /// Database primary key for this Account
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <summary>
    /// Id for this Account
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public required AccountType Type { get; set; }

    /// <summary>
    /// Is active flag for this Account
    /// </summary>
    public required bool IsActive { get; set; }
}