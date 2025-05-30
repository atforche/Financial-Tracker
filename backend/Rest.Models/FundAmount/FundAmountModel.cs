using System.Text.Json.Serialization;
using Rest.Models.Fund;

namespace Rest.Models.FundAmount;

/// <summary>
/// REST model representing an Fund Amount
/// </summary>
public class FundAmountModel
{
    /// <inheritdoc cref="Domain.Funds.FundAmount.Fund"/>
    public FundModel Fund { get; init; }

    /// <inheritdoc cref="Domain.Funds.FundAmount.Amount"/>
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
    public FundAmountModel(Domain.Funds.FundAmount fundAmount)
    {
        Fund = new FundModel(fundAmount.Fund);
        Amount = fundAmount.Amount;
    }
}