using System.Text.Json.Serialization;

namespace Rest.Models.FundAmount;

/// <summary>
/// REST model representing an Fund Amount
/// </summary>
public class FundAmountModel
{
    /// <inheritdoc cref="Domain.Funds.FundAmount.FundId"/>
    public Guid FundId { get; init; }

    /// <inheritdoc cref="Domain.Funds.FundAmount.Amount"/>
    public decimal Amount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public FundAmountModel(Guid fundId, decimal amount)
    {
        FundId = fundId;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundAmount">Fund Amount value object to build this Fund Amount REST model from</param>
    public FundAmountModel(Domain.Funds.FundAmount fundAmount)
    {
        FundId = fundAmount.FundId.Value;
        Amount = fundAmount.Amount;
    }
}