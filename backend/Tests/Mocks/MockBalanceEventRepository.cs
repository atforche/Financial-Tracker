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
        var accountAddedBalanceEvents = accountRepository.FindAll()
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(balanceEvent => balanceEvent.EventDate == eventDate)
            .ToList();
        int highestAccountBalanceEvent = accountAddedBalanceEvents.Count != 0
            ? accountAddedBalanceEvents.Max(balanceEvent => balanceEvent.EventSequence)
            : 0;

        var accountingPeriodBalanceEvents = accountingPeriodRepository.FindAll()
            .SelectMany(accountingPeriod => accountingPeriod.GetAllBalanceEvents())
            .Where(balanceEvent => balanceEvent.EventDate == eventDate)
            .ToList();
        int highestAccountingPeriodBalanceEvent = accountingPeriodBalanceEvents.Count != 0
            ? accountingPeriodBalanceEvents.Max(balanceEvent => balanceEvent.EventSequence)
            : 0;

        return Math.Max(highestAccountBalanceEvent, highestAccountingPeriodBalanceEvent);
    }
}