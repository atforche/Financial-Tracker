using System.Text.Json.Serialization;
using RestApi.Models.Fund;

namespace RestApi.Models.FundAmount;

/// <summary>
/// REST model representing an Fund Amount
/// </summary>
public class FundAmountModel
{
    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.Fund"/>
    public FundModel Fund { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.Amount"/>
    public decimal Amount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public FundAmountModel(FundModel fund, decimal amount)
    {
        Fund = fund;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundAmount">Fund Amount value object to build this Fund Amount REST model from</param>
    public FundAmountModel(Domain.ValueObjects.FundAmount fundAmount)
    {
        Fund = new FundModel(fundAmount.Fund);
        Amount = fundAmount.Amount;
    }
}