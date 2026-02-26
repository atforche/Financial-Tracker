using System.Diagnostics.CodeAnalysis;
using Data.Funds;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Funds to Fund Models
/// </summary>
public sealed class FundMapper(FundBalanceService fundBalanceService, FundBalanceMapper fundBalanceMapper, FundRepository fundRepository)
{
    /// <summary>
    /// Maps the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description,
        CurrentBalance = fundBalanceMapper.ToModel(fundBalanceService.GetCurrentBalance(fund.Id))
    };

    /// <summary>
    /// Attempts to map the provided ID to a Fund
    /// </summary>
    public bool TryToDomain(Guid fundId, [NotNullWhen(true)] out Fund? fund) => fundRepository.TryGetById(fundId, out fund);
}