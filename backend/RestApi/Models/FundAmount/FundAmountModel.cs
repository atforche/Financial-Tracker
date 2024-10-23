namespace RestApi.Models.FundAmount;

/// <summary>
/// REST model representing an Fund Amount
/// </summary>
public class FundAmountModel
{
    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.FundId"/>
    public required Guid FundId { get; init; }

    /// <inheritdoc cref="Domain.ValueObjects.FundAmount.Amount"/>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Converts the Fund Amount domain value object into a Fund Amount REST model
    /// </summary>
    /// <param name="fundAmount">Fund Amount domain value object to be converted</param>
    /// <returns>The converted Fund Amount model</returns>
    internal static FundAmountModel ConvertValueObjectToModel(Domain.ValueObjects.FundAmount fundAmount) =>
        new FundAmountModel
        {
            FundId = fundAmount.FundId,
            Amount = fundAmount.Amount,
        };
}