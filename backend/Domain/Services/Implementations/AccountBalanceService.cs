using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountBalanceService(IAccountingPeriodRepository accountingPeriodRepository) : IAccountBalanceService
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository = accountingPeriodRepository;

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceByDate> GetAccountBalancesByDate(Account account, DateRange dateRange)
    {
        List<AccountBalanceByDate> results = [];
        AccountBalance accountBalance = DetermineAccountBalanceAtStartOfDate(account, dateRange.GetInclusiveDates().First());
        var balanceEvents = GetAllBalanceEventsInDateRange(account, dateRange)
            .GroupBy(balanceEvent => balanceEvent.EventDate)
            .ToDictionary(group => group.Key, group => group.ToList());
        foreach (DateOnly date in dateRange.GetInclusiveDates())
        {
            foreach (BalanceEventBase balanceEvent in balanceEvents.GetValueOrDefault(date) ?? [])
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance);
            }
            results.Add(new AccountBalanceByDate
            {
                Date = date,
                AccountBalance = accountBalance,
            });
        }
        return results;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceByEvent> GetAccountBalancesByEvent(Account account, DateRange dateRange)
    {
        List<AccountBalanceByEvent> results = [];
        AccountBalance accountBalance = DetermineAccountBalanceAtStartOfDate(account, dateRange.GetInclusiveDates().First());
        foreach (BalanceEventBase balanceEvent in GetAllBalanceEventsInDateRange(account, dateRange))
        {
            accountBalance = balanceEvent.ApplyEventToBalance(accountBalance);
            results.Add(new AccountBalanceByEvent
            {
                BalanceEvent = balanceEvent,
                AccountBalance = accountBalance
            });
        }
        return results;
    }

    /// <inheritdoc/>
    public AccountBalanceByAccountingPeriod GetAccountBalancesByAccountingPeriod(Account account, AccountingPeriod accountingPeriod)
    {
        AccountingPeriod? pastAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(
            accountingPeriod.PeriodStartDate.AddMonths(-1));
        AccountingPeriod? futureAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(
            accountingPeriod.PeriodStartDate.AddMonths(1));

        AccountBalance? startingBalance = null;
        if (accountingPeriod.AccountBalanceCheckpoints.Count != 0)
        {
            startingBalance = accountingPeriod.AccountBalanceCheckpoints
                .SingleOrDefault(balanceCheckpoint => balanceCheckpoint.Account == account)?.ConvertToAccountBalance();
        }
        else if (pastAccountingPeriod != null)
        {
            startingBalance = GetAccountBalancesByAccountingPeriod(account, pastAccountingPeriod).EndingBalance;
        }
        if (startingBalance == null)
        {
            // If the current period doesn't have a balance checkpoint for this Account and there's no past periods
            // to check, return a balance of zero
            return new AccountBalanceByAccountingPeriod(accountingPeriod, new AccountBalance(account, [], []), new AccountBalance(account, [], []));
        }
        AccountBalance endingBalance = startingBalance;

        // If this accounting period is closed and there's a future period, just use the start of period
        // checkpoint from the future period as the ending balance
        if (!accountingPeriod.IsOpen &&
            futureAccountingPeriod != null &&
            futureAccountingPeriod.AccountBalanceCheckpoints.Count > 0)
        {
            endingBalance = futureAccountingPeriod.AccountBalanceCheckpoints
                .Single(balanceCheckpoint => balanceCheckpoint.Account == account).ConvertToAccountBalance();
            return new AccountBalanceByAccountingPeriod(accountingPeriod, startingBalance, endingBalance);
        }
        // Otherwise, calculate the ending balance by applying all the Balance Events currently in the Accounting Period
        foreach (BalanceEventBase balanceEvent in accountingPeriod.GetAllBalanceEvents())
        {
            endingBalance = balanceEvent.ApplyEventToBalance(endingBalance);
        }
        return new AccountBalanceByAccountingPeriod(accountingPeriod, startingBalance, endingBalance);
    }

    /// <summary>
    /// Determines the balance of an Account at the beginning of the provided date
    /// </summary>
    /// <param name="account">Account whose balance to calculate</param>
    /// <param name="date">Date to find the account balance at the beginning of</param>
    /// <returns>The Account Balance of the provided Account at the beginning of the provided date</returns>
    private AccountBalance DetermineAccountBalanceAtStartOfDate(Account account, DateOnly date)
    {
        AccountingPeriod? checkpointPeriod = DetermineCheckpointPeriod(date);
        AccountBalance? accountBalance = checkpointPeriod?.AccountBalanceCheckpoints
            .SingleOrDefault(checkpoint => checkpoint.Account == account)?.ConvertToAccountBalance();

        // If there was no Account Balance Checkpoint to fall back to, the Account Balance is zero
        if (checkpointPeriod == null || accountBalance == null)
        {
            return new AccountBalance(account, [], []);
        }
        // Determine all of the Balance Events from the past Accounting Period that fall in the checkpoint Accounting Period
        AccountingPeriod? pastAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(checkpointPeriod.PeriodStartDate.AddMonths(-1));
        List<BalanceEventBase> pastPeriodBalanceEventsDuringOrAfterDateRange = pastAccountingPeriod?.GetAllBalanceEvents()
            .Where(balanceEvent => balanceEvent.EventDate >= checkpointPeriod.PeriodStartDate).ToList() ?? [];

        // All of the above events have already been applied to our balance checkpoint. But we really want to individually 
        // apply them as they occur on each date. So reverse all of them so they can be reapplied on their respective dates.
        foreach (BalanceEventBase balanceEvent in Enumerable.Reverse(pastPeriodBalanceEventsDuringOrAfterDateRange))
        {
            accountBalance = balanceEvent.ReverseEventFromBalance(accountBalance);
        }
        // Grab all of the Balance Events that fall from the start of the checkpoint Accounting Period up until the 
        // date and apply them all to our Account Balance
        if (date != checkpointPeriod.PeriodStartDate)
        {
            var beforeDateRange = new DateRange(checkpointPeriod.PeriodStartDate, date, endDateType: EndpointType.Exclusive);
            IEnumerable<BalanceEventBase> balanceEventsBeforeDate = checkpointPeriod.GetAllBalanceEvents()
                .Where(balanceEvent => !beforeDateRange.IsWithinStartDate(balanceEvent.EventDate))
                .Concat(GetAllBalanceEventsInDateRange(account, beforeDateRange));
            foreach (BalanceEventBase balanceEvent in balanceEventsBeforeDate)
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance);
            }
        }
        return accountBalance;
    }

    /// <summary>
    /// Determines the Accounting Period whose Account Balance Checkpoints should be used as the baseline for
    /// calculating the balance on the provided date
    /// </summary>
    /// <param name="date">Date to find the account balance at the beginning of</param>
    /// <returns>The Accounting Period whose Account Balance Checkpoints should be used in the balance calculation</returns>
    private AccountingPeriod? DetermineCheckpointPeriod(DateOnly date)
    {
        AccountingPeriod? result = _accountingPeriodRepository.FindByDateOrNull(date);
        while (result == null || result.AccountBalanceCheckpoints.Count == 0)
        {
            DateOnly previousAccountingPeriodDate = date.AddMonths(-1);
            AccountingPeriod? previousAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(previousAccountingPeriodDate);
            if (previousAccountingPeriod != null)
            {
                date = previousAccountingPeriodDate;
                result = previousAccountingPeriod;
            }
            else
            {
                // There wasn't an Accounting Period further in the past to search, so return null
                return null;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets all of the Balance Events that fall in the provided date range
    /// </summary>
    /// <param name="account">Account for the Balance Events to find</param>
    /// <param name="dateRange">Date range to get all the Balance Events from</param>
    /// <returns>The list of Balance Events that fall in the provided date range</returns>
    private IEnumerable<BalanceEventBase> GetAllBalanceEventsInDateRange(Account account, DateRange dateRange) =>
        _accountingPeriodRepository
            .FindAccountingPeriodsWithBalanceEventsInDateRange(dateRange)
            .SelectMany(period => period.GetAllBalanceEvents())
            .Where(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate) && balanceEvent.Account == account)
            .OrderBy(balanceEvent => balanceEvent.EventDate)
            .ThenBy(balanceEvent => balanceEvent.AccountingPeriod.PeriodStartDate)
            .ThenBy(balanceEvent => balanceEvent.EventSequence);
}