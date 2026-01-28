using Models.Funds;

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
    /// Description for the Account
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Accounting Period that the Account is being added to
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Date the Account is being added
    /// </summary>
    public required DateOnly AddDate { get; init; }

    /// <summary>
    /// Initial amounts for each Fund associated with the Account
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> InitialFundAmounts { get; init; }
}