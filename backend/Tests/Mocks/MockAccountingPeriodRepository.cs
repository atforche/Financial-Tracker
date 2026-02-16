using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Accounting Periods for testing
/// </summary>
internal sealed class MockAccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly Dictionary<Guid, AccountingPeriod> _accountingPeriods;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockAccountingPeriodRepository() => _accountingPeriods = [];

    /// <inheritdoc/>
    public AccountingPeriod GetById(AccountingPeriodId id) => _accountingPeriods[id.Value];

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod) => _accountingPeriods.TryGetValue(id, out accountingPeriod);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAll(GetAllAccountingPeriodsRequest request) => _accountingPeriods.Values;

    /// <inheritdoc/>
    public AccountingPeriod? GetLatestAccountingPeriod() => _accountingPeriods.Values
        .OrderBy(accountingPeriod => accountingPeriod.Year)
        .ThenBy(accountingPeriod => accountingPeriod.Month)
        .LastOrDefault();

    /// <inheritdoc/>
    public AccountingPeriod? GetNextAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = GetById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(1);
        return _accountingPeriods.Values
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public AccountingPeriod? GetPreviousAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = GetById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(-1);
        return _accountingPeriods.Values
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => _accountingPeriods.Values
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => _accountingPeriods.Add(accountingPeriod.Id.Value, accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => _accountingPeriods.Remove(accountingPeriod.Id.Value);
}