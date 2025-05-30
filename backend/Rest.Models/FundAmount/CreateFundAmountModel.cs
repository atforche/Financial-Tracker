namespace Rest.Models.FundAmount;

/// <summary>
/// REST model representing a request to create an Fund Amount
/// </summary>
public class CreateFundAmountModel
{
    /// <inheritdoc cref="Domain.Funds.FundAmount.Fund"/>
    public required Guid FundId { get; init; }

    /// <inheritdoc cref="Domain.Funds.FundAmount.Amount"/>
    public required decimal Amount { get; init; }
}