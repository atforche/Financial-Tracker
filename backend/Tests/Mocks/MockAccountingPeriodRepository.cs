using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Accounting Periods for testing
/// </summary>
public class MockAccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly List<AccountingPeriod> _accountingPeriods;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockAccountingPeriodRepository()
    {
        _accountingPeriods = [];
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindByExternalIdOrNull(Guid id) => _accountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Id.ExternalId == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => _accountingPeriods;

    /// <inheritdoc/>
    public AccountingPeriod FindByDate(DateOnly asOfDate) => FindByDateOrNull(asOfDate) ?? throw new InvalidOperationException();

    /// <inheritdoc/>
    public AccountingPeriod? FindByDateOrNull(DateOnly asOfDate) => _accountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindOpenPeriods() => _accountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public AccountingPeriod FindLatestAccountingPeriodWithBalanceCheckpoints(DateOnly asOfDate)
    {
        AccountingPeriod? accountingPeriod = FindByDateOrNull(asOfDate);
        if (accountingPeriod != null && !accountingPeriod.IsOpen)
        {
            return accountingPeriod;
        }
        return _accountingPeriods
            .Where(accountingPeriod => accountingPeriod.IsOpen)
            .OrderBy(accountingPeriod => accountingPeriod.Year)
            .ThenBy(accountingPeriod => accountingPeriod.Month)
            .First();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAccountingPeriodsWithBalanceEventsInDateRange(DateRange dateRange) =>
        _accountingPeriods
        .Where(accountingPeriod => accountingPeriod.Transactions
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Any(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate)))
        .ToList();

    /// <inheritdoc/>
    public int FindMaximumBalanceEventSequenceForDate(DateOnly eventDate)
    {
        List<TransactionBalanceEvent> existingBalanceEventsOnDate = _accountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.Transactions)
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Where(balanceEvent => balanceEvent.EventDate == eventDate).ToList();
        if (existingBalanceEventsOnDate.Count > 0)
        {
            return existingBalanceEventsOnDate.Max(balanceEvent => balanceEvent.EventSequence + 1);
        }
        return 1;
    }

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => _accountingPeriods.Add(accountingPeriod);
}