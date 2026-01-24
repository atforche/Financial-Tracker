namespace Models.Accounts;

/// <summary>
/// Model representing a request to update an Account
/// </summary>
public class UpdateAccountModel
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }
}