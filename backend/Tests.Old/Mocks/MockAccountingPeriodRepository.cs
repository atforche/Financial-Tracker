using Domain.AccountingPeriods;

namespace Tests.Old.Mocks;

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
    public bool DoesAccountingPeriodWithIdExist(Guid id) => _accountingPeriods.ContainsKey(id);

    /// <inheritdoc/>
    public AccountingPeriod FindById(AccountingPeriodId id) => _accountingPeriods[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => _accountingPeriods.Values;

    /// <inheritdoc/>
    public AccountingPeriod? FindLatestAccountingPeriod() => _accountingPeriods.Values
        .OrderBy(accountingPeriod => accountingPeriod.Year)
        .ThenBy(accountingPeriod => accountingPeriod.Month)
        .LastOrDefault();

    /// <inheritdoc/>
    public AccountingPeriod? FindNextAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = FindById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(1);
        return _accountingPeriods.Values
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindPreviousAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = FindById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(-1);
        return _accountingPeriods.Values
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAllOpenPeriods() => _accountingPeriods.Values
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => _accountingPeriods.Add(accountingPeriod.Id.Value, accountingPeriod);
}