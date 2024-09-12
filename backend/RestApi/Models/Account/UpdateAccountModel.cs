namespace RestApi.Models.Account;

/// <summary>
/// Rest model representing a request to update an account
/// </summary>
public class UpdateAccountModel
{
    /// <summary>
    /// New name for this Account
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New is active flag for this Account
    /// </summary>
    public bool? IsActive { get; set; }
}