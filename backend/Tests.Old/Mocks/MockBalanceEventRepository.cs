using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.ChangeInValues;
using Domain.FundConversions;
using Domain.Transactions;

namespace Tests.Old.Mocks;

/// <summary>
/// Mock repository of Balance Events for testing
/// </summary>
internal sealed class MockBalanceEventRepository(
    IAccountRepository accountRepository,
    IChangeInValueRepository changeInValueRepository,
    IFundConversionRepository fundConversionRepository,
    ITransactionRepository transactionRepository) : IBalanceEventRepository
{
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
        IEnumerable<IBalanceEvent> accountAddedBalanceEvents = accountRepository.FindAll()
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.AccountingPeriodId == accountingPeriodId);
        IEnumerable<IBalanceEvent> transactionBalanceEvents = transactionRepository.FindAllByAccountingPeriod(accountingPeriodId)
            .SelectMany(transaction => transaction.TransactionBalanceEvents);
        IEnumerable<IBalanceEvent> fundConversions = fundConversionRepository.FindAllByAccountingPeriod(accountingPeriodId);
        IEnumerable<IBalanceEvent> changeInValues = changeInValueRepository.FindAllByAccountingPeriod(accountingPeriodId);
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

        IEnumerable<IBalanceEvent> accountAddedBalanceEvents = accountRepository.FindAll()
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(accountAddedBalanceEvent => accountAddedBalanceEvent.EventDate >= dates.First() && accountAddedBalanceEvent.EventDate <= dates.Last());
        IEnumerable<IBalanceEvent> transactionBalanceEvents = transactionRepository.FindAllByDateRange(dateRange)
            .SelectMany(transaction => transaction.TransactionBalanceEvents)
            .Where(transactionBalanceEvent => transactionBalanceEvent.EventDate >= dates.First() && transactionBalanceEvent.EventDate <= dates.Last());
        IEnumerable<IBalanceEvent> fundConversions = fundConversionRepository.FindAllByDateRange(dateRange);
        IEnumerable<IBalanceEvent> changeInValues = changeInValueRepository.FindAllByDateRange(dateRange);

        return accountAddedBalanceEvents
            .Concat(transactionBalanceEvents)
            .Concat(fundConversions)
            .Concat(changeInValues)
            .Order(new BalanceEventComparer())
            .ToList();
    }
}