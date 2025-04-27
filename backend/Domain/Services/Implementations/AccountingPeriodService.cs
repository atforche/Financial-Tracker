using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountingPeriodService(IAccountBalanceService accountBalanceService) : IAccountingPeriodService
{
    private readonly IAccountBalanceService _accountBalanceService = accountBalanceService;

    /// <inheritdoc/>
    public Transaction AddTransaction(
        AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries) =>
        accountingPeriod.AddTransaction(transactionDate,
            debitAccount != null ? GetCreateBalanceEventAccountInfo(accountingPeriod, debitAccount, transactionDate) : null,
            creditAccount != null ? GetCreateBalanceEventAccountInfo(accountingPeriod, creditAccount, transactionDate) : null,
            accountingEntries);

    /// <inheritdoc/>
    public void PostTransaction(Transaction transaction, Account account, DateOnly postedStatementDate) =>
        transaction.Post(GetCreateBalanceEventAccountInfo(transaction.AccountingPeriod, account, postedStatementDate), postedStatementDate);

    /// <inheritdoc/>
    public FundConversion AddFundConversion(AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        Fund fromFund,
        Fund toFund,
        decimal amount) =>
        accountingPeriod.AddFundConversion(eventDate,
            GetCreateBalanceEventAccountInfo(accountingPeriod, account, eventDate),
            fromFund,
            toFund,
            amount);

    /// <inheritdoc/>
    public ChangeInValue AddChangeInValue(AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        Account account,
        FundAmount accountingEntry) =>
        accountingPeriod.AddChangeInValue(eventDate,
            GetCreateBalanceEventAccountInfo(accountingPeriod, account, eventDate),
            accountingEntry);

    /// <summary>
    /// Builds a Create Balance Event Account Info for the provided Account and Event Date
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Create Balance Event Account Info</param>
    /// <param name="account">Account for this Create Balance Event Account Info</param>
    /// <param name="eventDate">Event Date for this Create Balance Event Account Info</param>
    /// <returns>The newly created Create Balance Event Account Info</returns>
    private CreateBalanceEventAccountInfo GetCreateBalanceEventAccountInfo(
        AccountingPeriod accountingPeriod,
        Account account,
        DateOnly eventDate) =>
        new(account,
            _accountBalanceService.GetAccountBalancesByDate(
                account,
                new DateRange(eventDate, eventDate)).First().AccountBalance,
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(account, accountingPeriod).EndingBalance,
            _accountBalanceService.GetAccountBalancesByEvent(
                account,
                new DateRange(eventDate, DateOnly.MaxValue))
                .Select(accountBalanceByEvent => accountBalanceByEvent.BalanceEvent).ToList());
}