namespace Models.Funds;

/// <summary>
/// Model representing a request to create a Fund Amount
/// </summary>
public class CreateFundAmountModel
{
    /// <summary>
    /// Fund for this Fund Amount
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}