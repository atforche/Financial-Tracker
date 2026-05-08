using System.Diagnostics.CodeAnalysis;
using Data.Funds;
using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Funds to Fund Models
/// </summary>
public sealed class FundConverter(FundBalanceService fundBalanceService, FundRepository fundRepository)
{
    /// <summary>
    /// Converts the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description,
        CurrentBalance = FundBalanceConverter.ToModel(fund, fundBalanceService.GetCurrentBalance(fund.Id))
    };

    /// <summary>
    /// Attempts to convert the provided ID to a Fund
    /// </summary>
    public bool TryToDomain(Guid fundId, [NotNullWhen(true)] out Fund? fund) => fundRepository.TryGetById(fundId, out fund);
}