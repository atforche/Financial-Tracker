using Domain;
using Domain.AccountingPeriods;
using Domain.ChangeInValues;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Change In Values to be persisted to the database
/// </summary>
public class ChangeInValueRepository(DatabaseContext databaseContext) : IChangeInValueRepository
{
    /// <inheritdoc/>
    public bool DoesChangeInValueWithIdExist(Guid id) => databaseContext.ChangeInValues.Any(changeInValue => ((Guid)(object)changeInValue.Id) == id);

    /// <inheritdoc/>
    public ChangeInValue FindById(ChangeInValueId id) => databaseContext.ChangeInValues.Single(changeInValue => changeInValue.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<ChangeInValue> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.ChangeInValues.Where(changeInValue => changeInValue.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<ChangeInValue> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return databaseContext.ChangeInValues
            .Where(changeInValue => changeInValue.EventDate >= dates.First() && changeInValue.EventDate <= dates.Last())
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(ChangeInValue changeInValue) => databaseContext.Add(changeInValue);
}