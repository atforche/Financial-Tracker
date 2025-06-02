using Domain;
using Domain.AccountingPeriods;
using Domain.BalanceEvents;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Balance Events to be retrieved from the database
/// </summary>
public class BalanceEventRepository(DatabaseContext databaseContext) : IBalanceEventRepository
{
    /// <inheritdoc/>
    public int GetHighestEventSequenceOnDate(DateOnly eventDate)
    {
        IReadOnlyCollection<IBalanceEvent> balanceEvents = FindAllByDateRange(new DateRange(eventDate, eventDate));
        if (balanceEvents.Count == 0)
        {
            return 0;
        }
        return balanceEvents.Max(balanceEvent => balanceEvent.EventSequence);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<IBalanceEvent> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId)
    {
        IEnumerable<IBalanceEvent> accountAddedBalanceEvents = databaseContext.Accounts
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodId == accountingPeriodId);
        IEnumerable<IBalanceEvent> transactionBalanceEvents = databaseContext.Transactions
            .Where(transaction => transaction.AccountingPeriodId == accountingPeriodId)
            .SelectMany(transaction => transaction.TransactionBalanceEvents);
        IEnumerable<IBalanceEvent> fundConversions = databaseContext.FundConversions
            .Where(fundConversion => fundConversion.AccountingPeriodId == accountingPeriodId);

        AccountingPeriod accountingPeriod = databaseContext.AccountingPeriods.Single(accountingPeriod => accountingPeriod.Id == accountingPeriodId);
        IEnumerable<IBalanceEvent> changeInValues = accountingPeriod.ChangeInValues;

        return accountAddedBalanceEvents
            .Concat(transactionBalanceEvents)
            .Concat(fundConversions)
            .Concat(changeInValues)
            .Order(new BalanceEventComparer())
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<IBalanceEvent> FindAllByDate(DateOnly eventDate) =>
        FindAllByDateRange(new DateRange(eventDate, eventDate));

    /// <inheritdoc/>
    public IReadOnlyCollection<IBalanceEvent> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();

        IEnumerable<IBalanceEvent> accountAddedBalanceEvents = databaseContext.Accounts
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.EventDate >= dates.First() && accountAddedBalanceEvent.EventDate <= dates.Last());
        IEnumerable<IBalanceEvent> transactionBalanceEvents = databaseContext.Transactions
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Where(transactionBalanceEvent => transactionBalanceEvent.EventDate >= dates.First() && transactionBalanceEvent.EventDate <= dates.Last());
        IEnumerable<IBalanceEvent> fundConversions = databaseContext.FundConversions
            .Where(fundConversion => fundConversion.EventDate >= dates.First() && fundConversion.EventDate <= dates.Last());
        IEnumerable<IBalanceEvent> changeInValues = databaseContext.AccountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .Where(changeInValue => changeInValue.EventDate >= dates.First() && changeInValue.EventDate <= dates.Last());

        return accountAddedBalanceEvents
            .Concat(transactionBalanceEvents)
            .Concat(fundConversions)
            .Concat(changeInValues)
            .Order(new BalanceEventComparer())
            .ToList();
    }
}