namespace RestApi.Models.FundAmount;

/// <summary>
/// REST model representing a request to create an Fund Amount
/// </summary>
public class CreateFundAmountModel
{
    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.FundId"/>
    public required Guid FundId { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.Amount"/>
    public required decimal Amount { get; init; }
}