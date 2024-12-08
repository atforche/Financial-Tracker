using Domain.Aggregates;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountingPeriodService : IAccountingPeriodService
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountBalanceService">Service that calculates account balances</param>
    public AccountingPeriodService(IAccountingPeriodRepository accountingPeriodRepository,
        IAccountRepository accountRepository,
        IAccountBalanceService accountBalanceService)
    {
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountRepository = accountRepository;
        _accountBalanceService = accountBalanceService;
    }

    /// <inheritdoc/>
    public AccountingPeriod CreateNewAccountingPeriod(int year, int month)
    {
        ValidateNewAccountingPeriod(year, month);
        AccountingPeriod newAccountingPeriod = new AccountingPeriod(year, month);

        DateOnly previousAccountingPeriodMonth = newAccountingPeriod.PeriodStartDate.AddMonths(-1);
        AccountingPeriod? previousAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(previousAccountingPeriodMonth);
        if (previousAccountingPeriod == null || previousAccountingPeriod.IsOpen)
        {
            return newAccountingPeriod;
        }
        // If the previous accounting period that has already closed, we'll need to add the 
        // Account Starting Balances for this new accounting period
        AddAccountBalanceCheckpointsToNextPeriod(newAccountingPeriod, previousAccountingPeriod);
        return newAccountingPeriod;
    }

    /// <inheritdoc/>
    public Transaction AddTransaction(
        AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries)
    {
        List<AccountBalanceByDate> currentAccountBalances = [];
        List<AccountBalanceByEvent> futureBalanceEventsForAccounts = [];
        if (debitAccount != null)
        {
            currentAccountBalances.Add(_accountBalanceService.GetAccountBalancesByDate(debitAccount,
                new DateRange(transactionDate, transactionDate)).First());
            futureBalanceEventsForAccounts.AddRange(_accountBalanceService.GetAccountBalancesByEvent(debitAccount,
                new DateRange(transactionDate, DateOnly.MaxValue, EndpointType.Exclusive)));
        }
        if (creditAccount != null)
        {
            currentAccountBalances.Add(_accountBalanceService.GetAccountBalancesByDate(creditAccount,
                new DateRange(transactionDate, transactionDate)).First());
            futureBalanceEventsForAccounts.AddRange(_accountBalanceService.GetAccountBalancesByEvent(creditAccount,
                new DateRange(transactionDate, DateOnly.MaxValue, EndpointType.Exclusive)));
        }
        return accountingPeriod.AddTransaction(transactionDate,
            debitAccount,
            creditAccount,
            accountingEntries,
            currentAccountBalances,
            futureBalanceEventsForAccounts);
    }


    /// <inheritdoc/>
    public void ClosePeriod(AccountingPeriod accountingPeriod)
    {
        ValidateCloseAccountingPeriod(accountingPeriod);
        accountingPeriod.IsOpen = false;

        DateOnly nextAccountingPeriodMonth = accountingPeriod.PeriodStartDate.AddMonths(1);
        AccountingPeriod? nextAccountingPeriod = _accountingPeriodRepository.FindByDateOrNull(nextAccountingPeriodMonth);
        if (nextAccountingPeriod == null)
        {
            return;
        }
        // If there's a future accounting period that exists (and is open), we'll need to add the
        // Account Starting Balances for the future Accounting Period
        AddAccountBalanceCheckpointsToNextPeriod(nextAccountingPeriod, accountingPeriod);
    }

    /// <summary>
    /// Validates a new Accounting Period that will be added
    /// </summary>
    /// <param name="year">Year of the Accounting Period to add</param>
    /// <param name="month">Month of the Accounting Period to add</param>
    private void ValidateNewAccountingPeriod(int year, int month)
    {
        List<AccountingPeriod> accountingPeriods = _accountingPeriodRepository.FindAll().ToList();
        if (accountingPeriods.Count == 0)
        {
            return;
        }
        // Validate that there are no duplicate accounting periods
        AccountingPeriod? duplicatePeriod = accountingPeriods
            .SingleOrDefault(period => period.Year == year && period.Month == month);
        if (duplicatePeriod != null)
        {
            throw new InvalidOperationException();
        }
        // Validate that accounting periods can only be added after existing accounting periods
        DateTime previousMonth = new DateTime(year, month, 1).AddMonths(-1);
        AccountingPeriod? previousAccountingPeriod = accountingPeriods
            .SingleOrDefault(period => period.Year == previousMonth.Year && period.Month == previousMonth.Month);
        if (previousAccountingPeriod == null)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Validates closing the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to be closed</param>
    private void ValidateCloseAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        if (!accountingPeriod.IsOpen)
        {
            throw new InvalidOperationException();
        }
        List<AccountingPeriod> openPeriods = _accountingPeriodRepository.FindOpenPeriods().ToList();
        if (openPeriods.Any(openPeriod => openPeriod.PeriodStartDate < accountingPeriod.PeriodStartDate))
        {
            // We should always have a contiguous group of open accounting periods.
            // Only close the earliest open accounting period
            throw new InvalidOperationException();
        }
        // Validate that there are no pending balance changes in this Accounting Period
        foreach (Account account in _accountRepository.FindAll())
        {
            if (_accountBalanceService.GetAccountBalancesByAccountingPeriod(account, accountingPeriod).EndingBalance.PendingFundBalanceChanges.Count != 0)
            {
                throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    /// Adds the necessary Account Balance Checkpoints to the next Accounting Period
    /// </summary>
    /// <param name="nextAccountingPeriod">Next Accounting Period</param>
    /// <param name="previousAccountingPeriod">Previous Accounting Period</param>
    private void AddAccountBalanceCheckpointsToNextPeriod(
        AccountingPeriod nextAccountingPeriod,
        AccountingPeriod previousAccountingPeriod)
    {
        DateOnly endOfPreviousPeriod = nextAccountingPeriod.PeriodStartDate.AddDays(-1);
        foreach (Account account in _accountRepository.FindAll())
        {
            nextAccountingPeriod.AddAccountBalanceCheckpoint(account,
                _accountBalanceService.GetAccountBalancesByAccountingPeriod(account, previousAccountingPeriod).EndingBalance.FundBalances);
        }
    }
}