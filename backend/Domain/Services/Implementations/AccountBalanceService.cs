using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountBalanceService : IAccountBalanceService
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    public AccountBalanceService(IAccountingPeriodRepository accountingPeriodRepository)
    {
        _accountingPeriodRepository = accountingPeriodRepository;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceByDate> GetAccountBalancesForDateRange(Account account, DateRange dateRange)
    {
        List<AccountBalanceByDate> results = [];

        // Find all the Accounting Periods needed to calculate the balances for the provided date range
        List<AccountingPeriod> accountingPeriods = _accountingPeriodRepository
            .FindAccountingPeriodsWithBalanceEventsInDateRange(dateRange)
            .OrderBy(accountingPeriod => new DateOnly(accountingPeriod.Year, accountingPeriod.Month, 1)).ToList();
        while (accountingPeriods.Count == 0 || accountingPeriods.First().StartOfPeriodBalanceCheckpoints.Count == 0)
        {
            DateOnly nextAccountingPeriodToAdd = accountingPeriods.Count != 0
                ? new DateOnly(accountingPeriods.First().Year, accountingPeriods.First().Month, 1).AddDays(-1)
                : dateRange.GetInclusiveDates().First();
            accountingPeriods.Insert(0, _accountingPeriodRepository.FindByDateOrNull(nextAccountingPeriodToAdd)
                ?? throw new InvalidOperationException());
        }
        // Initialize the current balance to be equal to the balance checkpoint at the beginning of the first accounting period
        AccountBalance currentBalance = accountingPeriods.First().StartOfPeriodBalanceCheckpoints
            .Single(balanceCheckpoint => balanceCheckpoint.Account == account).GetAccountBalance();
        // Get the list of all Balance Events that occur from the start of the earliest Accounting Period through
        // the end of the provided Date Range
        Dictionary<DateOnly, List<IBalanceEvent>> balanceEvents = accountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.GetAllBalanceEvents())
            .Where(balanceEvent => dateRange.IsWithinEndDate(balanceEvent.EventDate) && balanceEvent.Account == account)
            .GroupBy(balanceEvent => balanceEvent.EventDate)
            .ToDictionary(group => group.Key,
                group => group
                    .OrderBy(balanceEvent => new DateOnly(balanceEvent.AccountingPeriod.Year, balanceEvent.AccountingPeriod.Month, 1))
                    .ThenBy(balanceEvent => balanceEvent.EventSequence).ToList());
        // Apply all the events that fall prior to the Date Range to the starting balance
        foreach (IBalanceEvent balanceEvent in balanceEvents.Keys.Where(eventDate => !dateRange.IsInRange(eventDate))
            .SelectMany(key => balanceEvents[key]))
        {
            currentBalance = balanceEvent.ApplyEventToBalance(account, currentBalance);
        }
        // Now calculate the balance for each date in the date range
        foreach (DateOnly date in dateRange.GetInclusiveDates())
        {
            foreach (IBalanceEvent balanceEvent in balanceEvents.GetValueOrDefault(date) ?? [])
            {
                currentBalance = balanceEvent.ApplyEventToBalance(account, currentBalance);
            }
            results.Add(new AccountBalanceByDate(date, currentBalance));
        }
        return results;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountBalanceByEvent> GetAccountBalancesForEvents(
        Account account,
        DateRange dateRange)
    {
        List<AccountBalanceByEvent> results = [];

        // Find all the Accounting Periods that have Balance Events in the provided Date Range
        List<AccountingPeriod> accountingPeriods = _accountingPeriodRepository
            .FindAccountingPeriodsWithBalanceEventsInDateRange(dateRange)
            .OrderBy(accountingPeriod => new DateOnly(accountingPeriod.Year, accountingPeriod.Month, 1)).ToList();
        // Determine the Account Balance Checkpoint that we'll use as the starting point for calculating the balances
        AccountingPeriod firstAccountingPeriod = accountingPeriods.First();
        AccountBalance currentBalance = firstAccountingPeriod.StartOfPeriodBalanceCheckpoints
            .SingleOrDefault(balanceCheckpoint => balanceCheckpoint.Account == account)?.GetAccountBalance()
            ?? new AccountBalance
            {
                FundBalances = [],
                PendingFundBalanceChanges = []
            };
        // Get the list of all Balance Events that occur from the start of the earliest Accounting Period through
        // the end of the provided Date Range
        List<DateOnly> dates = dateRange.GetInclusiveDates().ToList();
        List<IBalanceEvent> balanceEvents = accountingPeriods
            .SelectMany(accountingPeriod => accountingPeriod.GetAllBalanceEvents())
            .Where(balanceEvent => dateRange.IsWithinEndDate(balanceEvent.EventDate) && balanceEvent.Account == account)
            .OrderBy(balanceEvent => balanceEvent.EventDate)
            .ThenBy(balanceEvent => new DateOnly(balanceEvent.AccountingPeriod.Year, balanceEvent.AccountingPeriod.Month, 1))
            .ThenBy(balanceEvent => balanceEvent.EventSequence).ToList();
        // Apply each of the Balance Events to the starting balance
        foreach (IBalanceEvent balanceEvent in balanceEvents)
        {
            currentBalance = balanceEvent.ApplyEventToBalance(account, currentBalance);
            if (balanceEvent.EventDate >= dates.Min())
            {
                results.Add(new AccountBalanceByEvent(balanceEvent, currentBalance));
            }
        }
        return results;
    }

    /// <inheritdoc/>
    public AccountBalanceByAccountingPeriod GetAccountBalancesForAccountingPeriod(
        Account account,
        AccountingPeriod accountingPeriod)
    {
        AccountBalance startingBalance = accountingPeriod.StartOfPeriodBalanceCheckpoints
            .SingleOrDefault(balanceCheckpoint => balanceCheckpoint.Account == account)?.GetAccountBalance()
            ?? new AccountBalance
            {
                FundBalances = [],
                PendingFundBalanceChanges = [],
            };
        AccountBalance endingBalance = startingBalance;

        // If this accounting period is closed and there's a future period, just use the start of period
        // checkpoint from the future period as the ending balance
        AccountingPeriod? nextAccountingPeriod = _accountingPeriodRepository
            .FindByDateOrNull(new DateOnly(accountingPeriod.Year, accountingPeriod.Month, 1).AddMonths(1));
        if (!accountingPeriod.IsOpen &&
            nextAccountingPeriod != null &&
            nextAccountingPeriod.StartOfPeriodBalanceCheckpoints.Count > 0)
        {
            endingBalance = nextAccountingPeriod.StartOfPeriodBalanceCheckpoints
                .Single(balanceCheckpoint => balanceCheckpoint.Account == account).GetAccountBalance();
            return new AccountBalanceByAccountingPeriod(startingBalance, endingBalance);
        }
        // Otherwise, calculate the ending balance by applying all the Balance Events currently in the Accounting Period
        foreach (IBalanceEvent balanceEvent in accountingPeriod.GetAllBalanceEvents())
        {
            endingBalance = balanceEvent.ApplyEventToBalance(account, endingBalance);
        }
        return new AccountBalanceByAccountingPeriod(startingBalance, endingBalance);
    }
}