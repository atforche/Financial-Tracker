using Domain;
using Domain.AccountingPeriods;
using Domain.FundConversions;
using Domain.Funds;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Fund Conversions for testing
/// </summary>
public class MockFundConversionRepository : IFundConversionRepository
{
    private readonly Dictionary<Guid, FundConversion> _fundConversions = [];

    /// <inheritdoc/>
    public bool DoesFundConversionWithIdExist(Guid id) => _fundConversions.ContainsKey(id);

    /// <inheritdoc/>
    public bool DoesFundConversionWithFundExist(Fund fund) =>
        _fundConversions.Values.Any(fundConversion => fundConversion.FromFundId == fund.Id || fundConversion.ToFundId == fund.Id);

    /// <inheritdoc/>
    public FundConversion FindById(FundConversionId id) => _fundConversions[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<FundConversion> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _fundConversions.Values.Where(fundConversion => fundConversion.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundConversion> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return _fundConversions.Values
            .Where(fundConversion => fundConversion.EventDate >= dates.First() && fundConversion.EventDate <= dates.Last())
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(FundConversion fundConversion) => _fundConversions.Add(fundConversion.Id.Value, fundConversion);
}