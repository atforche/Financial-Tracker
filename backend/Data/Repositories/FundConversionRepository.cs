using Domain;
using Domain.AccountingPeriods;
using Domain.FundConversions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Fund Conversions to be persisted to the database
/// </summary>
public class FundConversionRepository(DatabaseContext databaseContext) : IFundConversionRepository
{
    /// <inheritdoc/>
    public bool DoesFundConversionWithIdExist(Guid id) => databaseContext.FundConversions.Any(fundConversion => ((Guid)(object)fundConversion.Id) == id);

    /// <inheritdoc/>
    public FundConversion FindById(FundConversionId id) => databaseContext.FundConversions.Single(fundConversion => fundConversion.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<FundConversion> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundConversions.Where(fundConversion => fundConversion.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundConversion> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return databaseContext.FundConversions
            .Where(fundConversion => fundConversion.EventDate >= dates.First() && fundConversion.EventDate <= dates.Last())
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(FundConversion fundConversion) => databaseContext.Add(fundConversion);
}