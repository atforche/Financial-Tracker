using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Funds to Fund Models
/// </summary>
internal sealed class FundMapper
{
    /// <summary>
    /// Converts the provided Fund into a Fund Model
    /// </summary>
    /// <param name="fund">Fund to convert into a model</param>
    /// <returns>The Fund Model corresponding to the provided Fund</returns>
    public static FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description
    };
}