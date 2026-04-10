using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Types to Fund Type Models
/// </summary>
internal sealed class FundTypeMapper
{
    /// <summary>
    /// Maps the provided Fund Type to a Fund Type Model
    /// </summary>
    public static FundTypeModel ToModel(FundType fundType) => fundType switch
    {
        FundType.Monthly => FundTypeModel.Monthly,
        FundType.Rolling => FundTypeModel.Rolling,
        FundType.Savings => FundTypeModel.Savings,
        FundType.Debt => FundTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Fund Type: {fundType}")
    };

    /// <summary>
    /// Attempts to map the provided Fund Type Model to a Fund Type
    /// </summary>
    public static bool TryToDomain(FundTypeModel fundTypeModel, [NotNullWhen(true)] out FundType? fundType)
    {
        fundType = fundTypeModel switch
        {
            FundTypeModel.Monthly => FundType.Monthly,
            FundTypeModel.Rolling => FundType.Rolling,
            FundTypeModel.Savings => FundType.Savings,
            FundTypeModel.Debt => FundType.Debt,
            _ => null
        };
        return fundType != null;
    }
}