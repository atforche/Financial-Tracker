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
        IReadOnlyCollection<BalanceEvent> balanceEvents = FindAllByDateRange(new DateRange(eventDate, eventDate));
        if (balanceEvents.Count == 0)
        {
            return 0;
        }
        return balanceEvents.Max(balanceEvent => balanceEvent.EventSequence);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<BalanceEvent> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId)
    {
        IEnumerable<BalanceEvent> accountAddedBalanceEvents = databaseContext.Accounts
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodId == accountingPeriodId);
        IEnumerable<BalanceEvent> transactionBalanceEvents = databaseContext.Transactions
            .Where(transaction => transaction.AccountingPeriodId == accountingPeriodId)
            .SelectMany(transaction => transaction.TransactionBalanceEvents);

        AccountingPeriod accountingPeriod = databaseContext.AccountingPeriods.Single(accountingPeriod => accountingPeriod.Id == accountingPeriodId);
        IEnumerable<BalanceEvent> fundConversions = accountingPeriod.FundConversions;
        IEnumerable<BalanceEvent> changeInValues = accountingPeriod.ChangeInValues;

        return accountAddedBalanceEvents
            .Concat(transactionBalanceEvents)
            .Concat(fundConversions)
            .Concat(changeInValues)
            .Order()
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<BalanceEvent> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();

        IEnumerable<BalanceEvent> accountAddedBalanceEvents = databaseContext.Accounts
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.EventDate >= dates.First() && accountAddedBalanceEvent.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> transactionBalanceEvents = databaseContext.Transactions
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Where(transactionBalanceEvent => transactionBalanceEvent.EventDate >= dates.First() && transactionBalanceEvent.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> fundConversions = databaseContext.AccountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.FundConversions)
            .Where(fundConversion => fundConversion.EventDate >= dates.First() && fundConversion.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> changeInValues = databaseContext.AccountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .Where(changeInValue => changeInValue.EventDate >= dates.First() && changeInValue.EventDate <= dates.Last());

        return accountAddedBalanceEvents
            .Concat(transactionBalanceEvents)
            .Concat(fundConversions)
            .Concat(changeInValues)
            .Order()
            .ToList();
    }
}