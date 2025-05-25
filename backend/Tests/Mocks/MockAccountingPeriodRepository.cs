using Domain;
using Domain.AccountingPeriods;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Accounting Periods for testing
/// </summary>
internal sealed class MockAccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly List<AccountingPeriod> _accountingPeriods;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockAccountingPeriodRepository() => _accountingPeriods = [];

    /// <inheritdoc/>
    public bool DoesAccountingPeriodWithIdExist(Guid id) => _accountingPeriods.Any(accountingPeriod => accountingPeriod.Id.Value == id);

    /// <inheritdoc/>
    public AccountingPeriod FindById(AccountingPeriodId id) => _accountingPeriods.Single(accountingPeriod => accountingPeriod.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => _accountingPeriods;

    /// <inheritdoc/>
    public AccountingPeriod? FindByDateOrNull(DateOnly asOfDate) => _accountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindOpenPeriods() => _accountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAccountingPeriodsWithBalanceEventsInDateRange(DateRange dateRange) =>
        _accountingPeriods
        .Where(accountingPeriod => accountingPeriod.Transactions
                .SelectMany(transaction => transaction.TransactionBalanceEvents)
                .Any(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate)) ||
            accountingPeriod.FundConversions
                .Any(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate)) ||
            accountingPeriod.ChangeInValues
                .Any(changeInValue => dateRange.IsInRange(changeInValue.EventDate)) ||
            accountingPeriod.AccountAddedBalanceEvents
                .Any(changeInValue => dateRange.IsInRange(changeInValue.EventDate)))
        .ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => _accountingPeriods.Add(accountingPeriod);
}