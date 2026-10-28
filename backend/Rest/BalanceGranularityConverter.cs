using Domain;
using Models;

namespace Rest;

/// <summary>
/// Converter class that handles converting Balance Granularities to Balance Granularity Models
/// </summary>
internal static class BalanceGranularityConverter
{
    /// <summary>
    /// Converts the provided Balance Granularity to a Balance Granularity Model
    /// </summary>
    public static BalanceGranularityModel ToModel(BalanceGranularity granularity) => granularity switch
    {
        BalanceGranularity.Day => BalanceGranularityModel.Day,
        BalanceGranularity.Week => BalanceGranularityModel.Week,
        BalanceGranularity.Month => BalanceGranularityModel.Month,
        _ => throw new InvalidOperationException($"Unrecognized Balance Granularity: {granularity}")
    };
}
