using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountingPeriodService : IAccountingPeriodService
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly IAccountStartingBalanceFactory _accountStartingBalanceFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountBalanceService">Service that calculates account balances</param>
    /// <param name="accountStartingBalanceFactory">Factory that creates instances of Account Starting Balance</param>
    public AccountingPeriodService(IAccountingPeriodRepository accountingPeriodRepository,
        IAccountRepository accountRepository,
        IAccountBalanceService accountBalanceService,
        IAccountStartingBalanceFactory accountStartingBalanceFactory)
    {
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountRepository = accountRepository;
        _accountBalanceService = accountBalanceService;
        _accountStartingBalanceFactory = accountStartingBalanceFactory;
    }

    /// <inheritdoc/>
    public void CreateNewAccountingPeriod(int year, int month,
        out AccountingPeriod newAccountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances)
    {
        newAccountStartingBalances = [];

        ValidateNewAccountingPeriod(year, month);
        newAccountingPeriod = new AccountingPeriod(Guid.NewGuid(), year, month, true);
        newAccountingPeriod.Validate();

        DateOnly previousAccountingPeriodMonth = new DateOnly(year, month, 1).AddMonths(-1);
        AccountingPeriod? previousAccountingPeriod = _accountingPeriodRepository.FindOrNullByDate(previousAccountingPeriodMonth);
        if (previousAccountingPeriod == null || previousAccountingPeriod.IsOpen)
        {
            return;
        }
        // If the previous accounting period that has already closed, we'll need to add
        // the Account Starting Balances for this new accounting period
        DateOnly endOfPreviousMonth = new DateOnly(year, month, 1).AddDays(-1);
        foreach (Account account in _accountRepository.FindAll())
        {
            newAccountStartingBalances.Add(
                _accountStartingBalanceFactory.Create(account,
                    new CreateAccountStartingBalanceRequest
                    {
                        AccountingPeriod = newAccountingPeriod,
                        StartingBalance = _accountBalanceService.GetAccountBalanceAsOfDate(account, endOfPreviousMonth).Balance
                    }));
        }
    }

    /// <inheritdoc/>
    public void ClosePeriod(
        AccountingPeriod accountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances)
    {
        newAccountStartingBalances = [];

        ValidateCloseAccountingPeriod(accountingPeriod);
        accountingPeriod.IsOpen = false;

        DateOnly nextAccountingPeriodMonth = new DateOnly(accountingPeriod.Year, accountingPeriod.Month, 1).AddMonths(1);
        AccountingPeriod? nextAccountingPeriod = _accountingPeriodRepository.FindOrNullByDate(nextAccountingPeriodMonth);
        if (nextAccountingPeriod == null)
        {
            return;
        }
        // If there's a future accounting period that exists (and is open), we'll need to add the
        // Account Starting Balances for the future Accounting Period
        DateOnly endOfCurrentMonth = nextAccountingPeriodMonth.AddDays(-1);
        foreach (Account account in _accountRepository.FindAll())
        {
            newAccountStartingBalances.Add(
                _accountStartingBalanceFactory.Create(account,
                    new CreateAccountStartingBalanceRequest
                    {
                        AccountingPeriod = nextAccountingPeriod,
                        StartingBalance = _accountBalanceService.GetAccountBalanceAsOfDate(account, endOfCurrentMonth).Balance
                    }));
        }
    }

    /// <summary>
    /// Validates a new accounting period that will be added
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

    private void ValidateCloseAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        if (!accountingPeriod.IsOpen)
        {
            throw new InvalidOperationException();
        }
        List<AccountingPeriod> openPeriods = _accountingPeriodRepository.FindOpenPeriods().ToList();
        if (openPeriods.Any(openPeriod => new DateOnly(openPeriod.Year, openPeriod.Month, 1) <
            new DateOnly(accountingPeriod.Year, accountingPeriod.Month, 1)))
        {
            // We should always have a contiguous group of open accounting periods.
            // Only close the earliest open accounting period
            throw new InvalidOperationException();
        }
    }
}