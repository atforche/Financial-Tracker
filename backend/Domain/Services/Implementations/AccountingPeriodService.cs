using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
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
        var newAccountingPeriod = new AccountingPeriod(year,
            month,
            _accountingPeriodRepository.FindAll().Select(accountingPeriod => accountingPeriod.PeriodStartDate));

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
        IEnumerable<FundAmount> accountingEntries) =>
        accountingPeriod.AddTransaction(transactionDate,
            debitAccount != null ? GetCreateBalanceEventAccountInfo(debitAccount, transactionDate) : null,
            creditAccount != null ? GetCreateBalanceEventAccountInfo(creditAccount, transactionDate) : null,
            accountingEntries);

    /// <inheritdoc/>
    public void PostTransaction(Transaction transaction, Account account, DateOnly postedStatementDate) =>
        transaction.Post(GetCreateBalanceEventAccountInfo(account, postedStatementDate), postedStatementDate);

    /// <inheritdoc/>
    public FundConversion AddFundConversion(AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount) =>
        accountingPeriod.AddFundConversion(eventDate,
            GetCreateBalanceEventAccountInfo(account, eventDate),
            fromFund,
            toFund,
            amount);

    /// <inheritdoc/>
    public ChangeInValue AddChangeInValue(AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        FundAmount accountingEntry) =>
        accountingPeriod.AddChangeInValue(eventDate,
            GetCreateBalanceEventAccountInfo(account, eventDate),
            accountingEntry);


    /// <inheritdoc/>
    public void ClosePeriod(AccountingPeriod accountingPeriod)
    {
        accountingPeriod.ClosePeriod(
            _accountingPeriodRepository.FindOpenPeriods().Select(period => period.PeriodStartDate).Where(startDate => startDate != accountingPeriod.PeriodStartDate),
            _accountRepository.FindAll().Select(account => _accountBalanceService.GetAccountBalancesByAccountingPeriod(account, accountingPeriod)));

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
    /// Adds the necessary Account Balance Checkpoints to the next Accounting Period
    /// </summary>
    /// <param name="nextAccountingPeriod">Next Accounting Period</param>
    /// <param name="previousAccountingPeriod">Previous Accounting Period</param>
    private void AddAccountBalanceCheckpointsToNextPeriod(
        AccountingPeriod nextAccountingPeriod,
        AccountingPeriod previousAccountingPeriod)
    {
        foreach (Account account in _accountRepository.FindAll())
        {
            nextAccountingPeriod.AddAccountBalanceCheckpoint(account,
                _accountBalanceService.GetAccountBalancesByAccountingPeriod(account, previousAccountingPeriod).EndingBalance.FundBalances);
        }
    }

    /// <summary>
    /// Builds a Create Balance Event Account Info for the provided Account and Event Date
    /// </summary>
    /// <param name="account">Account for this Create Balance Event Account Info</param>
    /// <param name="eventDate">Event Date for this Create Balance Event Account Info</param>
    /// <returns>The newly created Create Balance Event Account Info</returns>
    private CreateBalanceEventAccountInfo GetCreateBalanceEventAccountInfo(Account account, DateOnly eventDate) =>
        new(account,
            _accountBalanceService.GetAccountBalancesByDate(
                account,
                new DateRange(eventDate, eventDate)).First().AccountBalance,
            _accountBalanceService.GetAccountBalancesByEvent(
                account,
                new DateRange(eventDate, DateOnly.MaxValue))
                .Select(accountBalanceByEvent => accountBalanceByEvent.BalanceEvent).ToList());
}