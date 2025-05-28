using Domain.Funds;

namespace Rest.Models.Funds;

/// <summary>
/// REST model representing a request to create an Fund Amount
/// </summary>
public class CreateFundAmountModel
{
    /// <inheritdoc cref="FundAmount.FundId"/>
    public required Guid FundId { get; init; }

    /// <inheritdoc cref="FundAmount.Amount"/>
    public required decimal Amount { get; init; }
}