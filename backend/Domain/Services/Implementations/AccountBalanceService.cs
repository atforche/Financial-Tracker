using Domain.Aggregates;
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
            foreach (BalanceEvent balanceEvent in balanceEvents.GetValueOrDefault(date) ?? [])
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
        foreach (BalanceEvent balanceEvent in GetAllBalanceEventsInDateRange(account, dateRange))
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
    public AccountBalanceByAccountingPeriod GetAccountBalancesByAccountingPeriod(
        Account account,
        AccountingPeriod accountingPeriod)
    {
        AccountBalance startingBalance = DetermineAccountBalanceAtStartOfAccountingPeriod(account, accountingPeriod);
        AccountBalance endingBalance = startingBalance;

        DateOnly futureAccountingPeriodDate = accountingPeriod.PeriodStartDate.AddMonths(1);
        AccountBalanceCheckpoint? futurePeriodCheckpoint = account.AccountBalanceCheckpoints
            .FirstOrDefault(checkpoint => checkpoint.AccountingPeriodKey.ConvertToDate() == futureAccountingPeriodDate);
        if (futurePeriodCheckpoint != null)
        {
            // If there's a balance checkpoint for the future period, just use that checkpoint as the ending balance
            endingBalance = futurePeriodCheckpoint.ConvertToAccountBalance();
        }
        else
        {
            // Otherwise, calculate the ending balance by applying all the Balance Events currently in the Accounting Period
            foreach (BalanceEvent balanceEvent in accountingPeriod.GetAllBalanceEvents())
            {
                endingBalance = balanceEvent.ApplyEventToBalance(endingBalance);
            }
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
        AccountBalanceCheckpoint? balanceCheckpoint = account.AccountBalanceCheckpoints
            .FirstOrDefault(checkpoint => checkpoint.AccountingPeriodKey.ConvertToDate() <= date);
        if (balanceCheckpoint != null)
        {
            return DetermineAccountBalanceAtStartOfDateWithCheckpoint(account, date, balanceCheckpoint);
        }
        return DetermineAccountBalanceAtStartOfDateWithoutCheckpoint(account, date);
    }

    /// <summary>
    /// Determines the balance of an Account at the beginning of the provided date using a balance checkpoint
    /// </summary>
    /// <param name="account">Account whose balance to calculate</param>
    /// <param name="date">Date to find the account balance at the beginning of</param>
    /// <param name="balanceCheckpoint">Account Balance Checkpoint to use</param>
    /// <returns>The Account Balance of the provided Account at the beginning of the provided date</returns>
    private AccountBalance DetermineAccountBalanceAtStartOfDateWithCheckpoint(
        Account account,
        DateOnly date,
        AccountBalanceCheckpoint balanceCheckpoint)
    {
        AccountBalance accountBalance = balanceCheckpoint.ConvertToAccountBalance();

        // Determine all of the Balance Events from the past Accounting Period that fall in the checkpoint Accounting Period.
        // All of these events have already been applied to our balance checkpoint. But we really want to individually 
        // apply them as they occur on each date. So reverse all of them so they can be reapplied on their respective dates.
        AccountingPeriod? checkpointPeriod = _accountingPeriodRepository.FindByDate(balanceCheckpoint.AccountingPeriodKey.ConvertToDate());
        AccountingPeriod? pastAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(checkpointPeriod.PeriodStartDate.AddMonths(-1));
        List<BalanceEvent> pastPeriodBalanceEventsDuringOrAfterDateRange = pastAccountingPeriod?.GetAllBalanceEvents()
            .Where(balanceEvent => balanceEvent.EventDate >= checkpointPeriod.PeriodStartDate).ToList() ?? [];
        foreach (BalanceEvent balanceEvent in Enumerable.Reverse(pastPeriodBalanceEventsDuringOrAfterDateRange))
        {
            accountBalance = balanceEvent.ReverseEventFromBalance(accountBalance);
        }

        // Grab all of the Balance Events that fall from the start of the checkpoint Accounting Period up until the 
        // date and apply them all to our Account Balance
        if (date != checkpointPeriod.PeriodStartDate)
        {
            var beforeDateRange = new DateRange(checkpointPeriod.PeriodStartDate, date, endDateType: EndpointType.Exclusive);
            IEnumerable<BalanceEvent> balanceEventsBeforeDate = checkpointPeriod.GetAllBalanceEvents()
                .Where(balanceEvent => !beforeDateRange.IsWithinStartDate(balanceEvent.EventDate))
                .Concat(GetAllBalanceEventsInDateRange(account, beforeDateRange));
            foreach (BalanceEvent balanceEvent in balanceEventsBeforeDate)
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance);
            }
        }
        return accountBalance;
    }

    /// <summary>
    /// Determines the balance of an Account at the beginning of the provided date without a balance checkpoint
    /// </summary>
    /// <param name="account">Account whose balance to calculate</param>
    /// <param name="date">Date to find the account balance at the beginning of</param>
    /// <returns>The Account Balance of the provided Account at the beginning of the provided date</returns>
    private AccountBalance DetermineAccountBalanceAtStartOfDateWithoutCheckpoint(Account account, DateOnly date)
    {
        var accountBalance = new AccountBalance(account, [], []);
        if (account.AccountAddedBalanceEvent.EventDate >= date)
        {
            return accountBalance;
        }
        var balanceEventDateRange = new DateRange(account.AccountAddedBalanceEvent.EventDate, date, endDateType: EndpointType.Exclusive);
        foreach (BalanceEvent balanceEvent in GetAllBalanceEventsInDateRange(account, balanceEventDateRange))
        {
            accountBalance = balanceEvent.ApplyEventToBalance(accountBalance);
        }
        return accountBalance;
    }

    /// <summary>
    /// Determines the balance of an Account at the beginning of the provided Accounting Period
    /// </summary>
    /// <param name="account">Account whose balance to calculate</param>
    /// <param name="accountingPeriod">Accounting Period to find the Account Balance at the beginning of</param>
    /// <returns>The Account Balance at the beginning of the provided Accounting Period</returns>
    private AccountBalance DetermineAccountBalanceAtStartOfAccountingPeriod(Account account, AccountingPeriod accountingPeriod)
    {
        AccountBalanceCheckpoint? checkpoint = account.AccountBalanceCheckpoints
            .FirstOrDefault(checkpoint => checkpoint.AccountingPeriodKey == accountingPeriod.Key);
        if (checkpoint != null)
        {
            return checkpoint.ConvertToAccountBalance();
        }
        AccountingPeriod? pastAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(
            accountingPeriod.PeriodStartDate.AddMonths(-1));
        if (pastAccountingPeriod != null)
        {
            return GetAccountBalancesByAccountingPeriod(account, pastAccountingPeriod).EndingBalance;
        }
        return new AccountBalance(account, [], []);
    }

    /// <summary>
    /// Gets all of the Balance Events that fall in the provided date range
    /// </summary>
    /// <param name="account">Account for the Balance Events to find</param>
    /// <param name="dateRange">Date range to get all the Balance Events from</param>
    /// <returns>The list of Balance Events that fall in the provided date range</returns>
    private IEnumerable<BalanceEvent> GetAllBalanceEventsInDateRange(Account account, DateRange dateRange) =>
        _accountingPeriodRepository
            .FindAccountingPeriodsWithBalanceEventsInDateRange(dateRange)
            .SelectMany(period => period.GetAllBalanceEvents())
            .Where(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate) && balanceEvent.Account == account)
            .Order();
}