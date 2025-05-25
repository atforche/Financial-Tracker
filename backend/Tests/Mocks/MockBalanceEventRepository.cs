using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Balance Events for testing
/// </summary>
internal sealed class MockBalanceEventRepository(
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository) : IBalanceEventRepository
{
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
        IEnumerable<BalanceEvent> accountAddedBalanceEvents = accountRepository.FindAll()
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodId == accountingPeriodId);

        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodId);
        IEnumerable<BalanceEvent> transactionBalanceEvents = accountingPeriod.Transactions
            .SelectMany(transaction => transaction.TransactionBalanceEvents);
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

        IEnumerable<BalanceEvent> accountAddedBalanceEvents = accountRepository.FindAll()
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.EventDate >= dates.First() && accountAddedBalanceEvent.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> transactionBalanceEvents = accountingPeriodRepository.FindAll()
            .SelectMany(accountingPeriod => accountingPeriod.Transactions)
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Where(transactionBalanceEvent => transactionBalanceEvent.EventDate >= dates.First() && transactionBalanceEvent.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> fundConversions = accountingPeriodRepository.FindAll()
            .SelectMany(accountingPeriod => accountingPeriod.FundConversions)
            .Where(fundConversion => fundConversion.EventDate >= dates.First() && fundConversion.EventDate <= dates.Last());
        IEnumerable<BalanceEvent> changeInValues = accountingPeriodRepository.FindAll()
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