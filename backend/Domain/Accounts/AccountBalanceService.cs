using Domain.AccountingPeriods;
using Domain.BalanceEvents;

namespace Domain.Accounts;

/// <summary>
/// Service used to calculate the balance of an Account as of a particular point in time
/// </summary>
public class AccountBalanceService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository)
{
    /// <summary>
    /// Gets the Account Balance for a single date
    /// </summary>
    /// <param name="accountId">Account ID to get the balance for</param>
    /// <param name="date">Date to get the balance for</param>
    /// <returns>The Account Balance for the provided date</returns>
    public AccountBalanceByDate GetAccountBalanceByDate(AccountId accountId, DateOnly date) =>
        GetAccountBalancesByDateRange(accountId, new DateRange(date, date)).First();

    /// <summary>
    /// Gets the list of Account Balances for each date over the provided Date Range
    /// </summary>
    /// <param name="accountId">Account to get the balances for</param>
    /// <param name="dateRange">Date Range to get the daily balances for</param>
    /// <returns>The list of Account Balances for each date over the Date Range</returns>
    public IReadOnlyCollection<AccountBalanceByDate> GetAccountBalancesByDateRange(AccountId accountId, DateRange dateRange)
    {
        List<AccountBalanceByDate> results = [];

        Account account = accountRepository.FindById(accountId);
        AccountBalance accountBalance = DetermineAccountBalanceAtStartOfDate(account, dateRange.GetInclusiveDates().First());
        var balanceEvents = GetAllBalanceEventsForAccountInDateRange(accountId, dateRange)
            .GroupBy(balanceEvent => balanceEvent.EventDate)
            .ToDictionary(group => group.Key, group => group.ToList());
        foreach (DateOnly date in dateRange.GetInclusiveDates())
        {
            foreach (IBalanceEvent balanceEvent in balanceEvents.GetValueOrDefault(date) ?? [])
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Standard);
            }
            results.Add(new AccountBalanceByDate
            {
                Date = date,
                AccountBalance = accountBalance,
            });
        }
        return results;
    }

    /// <summary>
    /// Gets the list of Account Balances for each Balance Event over the provided Date Range
    /// </summary>
    /// <param name="accountId">Account ID to get the balances for</param>
    /// <param name="dateRange">Date Range to get the event balances for</param>
    /// <returns>The list of Account Balances for each Balance Event over the Date Range</returns>
    public IReadOnlyCollection<AccountBalanceByEvent> GetAccountBalancesByEvent(AccountId accountId, DateRange dateRange)
    {
        List<AccountBalanceByEvent> results = [];

        Account account = accountRepository.FindById(accountId);
        AccountBalance accountBalance = DetermineAccountBalanceAtStartOfDate(account, dateRange.GetInclusiveDates().First());
        foreach (IBalanceEvent balanceEvent in GetAllBalanceEventsForAccountInDateRange(accountId, dateRange))
        {
            accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Standard);
            results.Add(new AccountBalanceByEvent
            {
                BalanceEvent = balanceEvent,
                AccountBalance = accountBalance
            });
        }
        return results;
    }

    /// <summary>
    /// Gets the Account Balance at the beginning and end of the provided Accounting Period.
    /// </summary>
    /// <param name="accountId">Account ID to get the Account Balance for</param>
    /// <param name="accountingPeriodId">Accounting Period ID to get the Account Balance for</param>
    /// <returns>The beginning and ending Account Balance for the Accounting Period</returns>
    public AccountBalanceByAccountingPeriod GetAccountBalanceByAccountingPeriod(
        AccountId accountId,
        AccountingPeriodId accountingPeriodId)
    {
        Account account = accountRepository.FindById(accountId);
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodId);
        AccountBalance startingBalance = DetermineAccountBalanceAtStartOfAccountingPeriod(account, accountingPeriod);
        AccountBalance endingBalance = startingBalance;

        AccountingPeriod? futureAccountingPeriod = accountingPeriodRepository.FindNextAccountingPeriod(accountingPeriod.Id);
        AccountBalanceCheckpoint? futurePeriodCheckpoint = account.AccountBalanceCheckpoints
            .FirstOrDefault(checkpoint => checkpoint.AccountingPeriodId == futureAccountingPeriod?.Id);
        if (futurePeriodCheckpoint != null)
        {
            // If there's a balance checkpoint for the future period, just use that checkpoint as the ending balance
            endingBalance = futurePeriodCheckpoint.ConvertToAccountBalance();
        }
        else
        {
            // Otherwise, calculate the ending balance by applying all the Balance Events currently in the Accounting Period
            foreach (IBalanceEvent balanceEvent in GetAllBalanceEventsForAccountInAccountingPeriod(accountId, accountingPeriod.Id))
            {
                endingBalance = balanceEvent.ApplyEventToBalance(endingBalance, ApplicationDirection.Standard);
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
        IEnumerable<AccountBalanceCheckpoint> balanceCheckpoints = account.AccountBalanceCheckpoints.Reverse();
        foreach (AccountBalanceCheckpoint accountBalanceCheckpoint in balanceCheckpoints)
        {
            AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountBalanceCheckpoint.AccountingPeriodId);
            if (accountingPeriod.PeriodStartDate < date)
            {
                return DetermineAccountBalanceAtStartOfDateWithCheckpoint(account, date, accountBalanceCheckpoint);
            }
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
        AccountingPeriod checkpointPeriod = accountingPeriodRepository.FindById(balanceCheckpoint.AccountingPeriodId);
        AccountingPeriod? pastAccountingPeriod = accountingPeriodRepository.FindPreviousAccountingPeriod(checkpointPeriod.Id);
        if (pastAccountingPeriod != null)
        {
            List<IBalanceEvent> pastPeriodBalanceEventsDuringOrAfterDateRange = balanceEventRepository
                .FindAllByAccountingPeriod(pastAccountingPeriod.Id)
                .Where(balanceEvent => balanceEvent.EventDate >= checkpointPeriod.PeriodStartDate).ToList() ?? [];
            foreach (IBalanceEvent balanceEvent in Enumerable.Reverse(pastPeriodBalanceEventsDuringOrAfterDateRange))
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Reverse);
            }
        }

        // Grab all of the Balance Events that fall from the start of the checkpoint Accounting Period up until the 
        // date and apply them all to our Account Balance
        if (date != checkpointPeriod.PeriodStartDate)
        {
            var beforeDateRange = new DateRange(checkpointPeriod.PeriodStartDate, date, endDateType: EndpointType.Exclusive);
            IEnumerable<IBalanceEvent> balanceEventsBeforeDate = balanceEventRepository
                .FindAllByAccountingPeriod(checkpointPeriod.Id)
                .Where(balanceEvent => !beforeDateRange.IsWithinStartDate(balanceEvent.EventDate))
                .Concat(GetAllBalanceEventsForAccountInDateRange(account.Id, beforeDateRange));
            foreach (IBalanceEvent balanceEvent in balanceEventsBeforeDate)
            {
                accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Standard);
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
        foreach (IBalanceEvent balanceEvent in GetAllBalanceEventsForAccountInDateRange(account.Id, balanceEventDateRange))
        {
            accountBalance = balanceEvent.ApplyEventToBalance(accountBalance, ApplicationDirection.Standard);
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
            .FirstOrDefault(checkpoint => checkpoint.AccountingPeriodId == accountingPeriod.Id);
        if (checkpoint != null)
        {
            return checkpoint.ConvertToAccountBalance();
        }
        AccountingPeriod? pastAccountingPeriod = accountingPeriodRepository.FindPreviousAccountingPeriod(accountingPeriod.Id);
        if (pastAccountingPeriod != null)
        {
            AccountBalance pastPeriodEndingBalance = GetAccountBalanceByAccountingPeriod(account.Id, pastAccountingPeriod.Id).EndingBalance;
            return new AccountBalance(account, pastPeriodEndingBalance.FundBalances.Concat(pastPeriodEndingBalance.PendingFundBalanceChanges), []);
        }
        return new AccountBalance(account, [], []);
    }

    /// <summary>
    /// Gets all the Balance Events for the provided Account that fall in the provided date range
    /// </summary>
    /// <param name="accountId">Account ID of the Balance Events to find</param>
    /// <param name="dateRange">Date range to get all the Balance Events from</param>
    /// <returns>The list of Balance Events for the provided Account that fall in the provided date range</returns>
    private IEnumerable<IBalanceEvent> GetAllBalanceEventsForAccountInDateRange(AccountId accountId, DateRange dateRange) =>
        balanceEventRepository
            .FindAllByDateRange(dateRange)
            .Where(balanceEvent => dateRange.IsInRange(balanceEvent.EventDate) && balanceEvent.GetAccountIds().Contains(accountId))
            .Order(new BalanceEventComparer());

    /// <summary>
    /// Gets all the Balance Events for the provided Account that fall in the provided Accounting Period
    /// </summary>
    /// <param name="accountId">Account ID of the Balance Events to find</param>
    /// <param name="accountingPeriodId">Accounting Period ID of the Balance Events to find</param>
    /// <returns>The list of Balance Events for the provided Account that fall in the provided Accounting Period</returns>
    private IEnumerable<IBalanceEvent> GetAllBalanceEventsForAccountInAccountingPeriod(
        AccountId accountId,
        AccountingPeriodId accountingPeriodId) =>
        balanceEventRepository.FindAllByAccountingPeriod(accountingPeriodId)
            .Where(balanceEvent => balanceEvent.GetAccountIds().Contains(accountId))
            .Order(new BalanceEventComparer());
}