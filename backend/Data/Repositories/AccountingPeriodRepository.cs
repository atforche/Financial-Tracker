using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository : AggregateRepositoryBase<AccountingPeriod>, IAccountingPeriodRepository
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public AccountingPeriodRepository(DatabaseContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => DatabaseContext.AccountingPeriods
        .AsEnumerable()
        .OrderBy(entity => entity.PeriodStartDate).ToList();

    /// <inheritdoc/>
    public AccountingPeriod FindByDate(DateOnly asOfDate) => FindByDateOrNull(asOfDate) ?? throw new InvalidOperationException();

    /// <inheritdoc/>
    public AccountingPeriod? FindByDateOrNull(DateOnly asOfDate) => DatabaseContext.AccountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindOpenPeriods() => DatabaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public AccountingPeriod FindLatestAccountingPeriodWithBalanceCheckpoints(DateOnly asOfDate)
    {
        AccountingPeriod? accountingPeriod = FindByDateOrNull(asOfDate);
        if (accountingPeriod != null && !accountingPeriod.IsOpen)
        {
            return accountingPeriod;
        }
        return DatabaseContext.AccountingPeriods
            .Where(accountingPeriod => accountingPeriod.IsOpen)
            .OrderBy(accountingPeriod => accountingPeriod.Year)
            .ThenBy(accountingPeriod => accountingPeriod.Month)
            .First();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAccountingPeriodsWithBalanceEventsInDateRange(DateRange dateRange)
    {
        List<DateOnly> dates = dateRange.GetInclusiveDates().ToList();
        return DatabaseContext.AccountingPeriods
            .Where(accountingPeriod => accountingPeriod.Transactions
                    .SelectMany(transaction => transaction.TransactionBalanceEvents)
                    .Any(balanceEvent => balanceEvent.EventDate >= dates.First() && balanceEvent.EventDate <= dates.Last()) ||
                accountingPeriod.FundConversions
                    .Any(balanceEvent => balanceEvent.EventDate >= dates.First() && balanceEvent.EventDate <= dates.Last()))
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => DatabaseContext.Add(accountingPeriod);
}