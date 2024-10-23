using Domain.ValueObjects;

namespace Data.ValueObjectModels;

/// <summary>
/// Data model representing an Fund Amount
/// </summary>
public class FundAmountData
{
    /// <summary>
    /// Database primary key for this Fund Amount
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="FundAmount.FundId"/>
    public required Guid FundId { get; set; }

    /// <inheritdoc cref="FundAmount.Amount"/>
    public required decimal Amount { get; set; }
}