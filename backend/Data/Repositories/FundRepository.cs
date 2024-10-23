using Data.EntityModels;
using Data.Requests;
using Domain.Entities;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Funds to be persisted to the database
/// </summary>
public class FundRepository : IFundRepository
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public FundRepository(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Fund> FindAll() => _context.Funds.Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public Fund? FindOrNull(Guid id)
    {
        FundData? fundData = _context.Funds.FirstOrDefault(fund => fund.Id == id);
        return fundData != null ? ConvertToEntity(fundData) : null;
    }

    /// <inheritdoc/>
    public Fund? FindByNameOrNull(string name)
    {
        FundData? fundData = _context.Funds.FirstOrDefault(fund => fund.Name == name);
        return fundData != null ? ConvertToEntity(fundData) : null;
    }

    /// <inheritdoc/>
    public void Add(Fund fund)
    {
        var fundData = PopulateFromFund(fund, null);
        _context.Add(fundData);
    }

    /// <summary>
    /// Converts the provided <see cref="FundData"/> object into a <see cref="Fund"/> domain entity.
    /// </summary>
    /// <param name="fundData">Fund Data to be converted</param>
    /// <returns>The converted Fund domain entity</returns>
    private Fund ConvertToEntity(FundData fundData) => new Fund(
        new FundRecreateRequest
        {
            Id = fundData.Id,
            Name = fundData.Name,
        });

    /// <summary>
    /// Converts the provided <see cref="Fund"/> entity into a <see cref="FundData"/> data object
    /// </summary>
    /// <param name="fund">Fund entity to convert</param>
    /// <param name="existingFundData">Existing Fund Data model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted Fund data</returns>
    private static FundData PopulateFromFund(Fund fund, FundData? existingFundData)
    {
        FundData newFundData = new FundData
        {
            Id = fund.Id,
            Name = fund.Name,
        };
        existingFundData?.Replace(newFundData);
        return existingFundData ?? newFundData;
    }
}