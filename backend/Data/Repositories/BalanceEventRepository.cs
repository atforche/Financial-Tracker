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
        var accountAddedBalanceEvents = databaseContext.Accounts
            .Select(account => account.AccountAddedBalanceEvent)
            .Where(balanceEvent => balanceEvent.EventDate == eventDate)
            .ToList();
        int highestAccountBalanceEvent = accountAddedBalanceEvents.Count != 0
            ? accountAddedBalanceEvents.Max(balanceEvent => balanceEvent.EventSequence)
            : 0;

        var accountingPeriodBalanceEvents = databaseContext.AccountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.GetAllBalanceEvents())
            .Where(balanceEvent => balanceEvent.EventDate == eventDate)
            .ToList();
        int highestAccountingPeriodBalanceEvent = accountingPeriodBalanceEvents.Count != 0
            ? accountingPeriodBalanceEvents.Max(balanceEvent => balanceEvent.EventSequence)
            : 0;

        return Math.Max(highestAccountBalanceEvent, highestAccountingPeriodBalanceEvent);
    }
}