using Domain;
using Domain.AccountingPeriods;
using Domain.ChangeInValues;

namespace Tests.Old.Mocks;

/// <summary>
/// Mock repository of Change In Values for testing
/// </summary>
public class MockChangeInValueRepository : IChangeInValueRepository
{
    private readonly Dictionary<Guid, ChangeInValue> _changeInValues = [];

    /// <inheritdoc/>
    public bool DoesChangeInValueWithIdExist(Guid id) => _changeInValues.ContainsKey(id);

    /// <inheritdoc/>
    public ChangeInValue FindById(ChangeInValueId id) => _changeInValues[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<ChangeInValue> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _changeInValues.Values.Where(changeInValue => changeInValue.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<ChangeInValue> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return _changeInValues.Values
            .Where(changeInValue => changeInValue.EventDate >= dates.First() && changeInValue.EventDate <= dates.Last())
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(ChangeInValue changeInValue) => _changeInValues.Add(changeInValue.Id.Value, changeInValue);
}