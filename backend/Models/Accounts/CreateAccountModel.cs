namespace Models.Accounts;

/// <summary>
/// Model representing a request to create an Account.
/// </summary>
public class CreateAccountModel
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of the Account
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Opening Accounting Period for the Account
    /// </summary>
    public required Guid OpeningAccountingPeriodId { get; init; }

    /// <summary>
    /// Date the Account is being opened
    /// </summary>
    public required DateOnly DateOpened { get; init; }
}