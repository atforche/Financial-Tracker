using Domain.Entities;
using Domain.Repositories;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountBalanceService : IAccountBalanceService
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountStartingBalanceRepository _accountStartingBalanceRepository;
    private readonly ITransactionRepository _transactionRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodRepository">Accounting Period repository</param>
    /// <param name="accountStartingBalanceRepository">Account Starting Balance repository</param>
    /// <param name="transactionRepository">Transaction repository</param>
    public AccountBalanceService(IAccountingPeriodRepository accountingPeriodRepository,
        IAccountStartingBalanceRepository accountStartingBalanceRepository,
        ITransactionRepository transactionRepository)
    {
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountStartingBalanceRepository = accountStartingBalanceRepository;
        _transactionRepository = transactionRepository;
    }

    /// <inheritdoc/>
    public AccountBalance GetAccountBalanceAsOfDate(Account account, DateOnly asOfDate)
    {
        AccountingPeriod effectiveAccountingPeriod = _accountingPeriodRepository.FindEffectiveAccountingPeriodForBalances(asOfDate);
        AccountStartingBalance? accountStartingBalance = _accountStartingBalanceRepository.FindOrNull(account.Id,
            effectiveAccountingPeriod.Id);
        IReadOnlyCollection<Transaction> transactions = _transactionRepository.FindAllByAccountOverDateRange(account.Id,
            new DateOnly(effectiveAccountingPeriod.Year, effectiveAccountingPeriod.Month, 1),
            asOfDate,
            DateToCompare.Accounting | DateToCompare.Statement);

        decimal balance = accountStartingBalance?.StartingBalance ?? 0.0m;
        decimal balanceIncludingPendingTransactions = balance;
        foreach (Transaction transaction in transactions)
        {
            balanceIncludingPendingTransactions = ApplyTransactionToAccountBalance(balance, transaction, account);
            if (!IsTransactionPending(transaction, account, asOfDate))
            {
                balance = ApplyTransactionToAccountBalance(balance, transaction, account);
            }
        }
        return new AccountBalance(balance, balanceIncludingPendingTransactions);
    }

    /// <summary>
    /// Determines if the Transaction is pending for an Account as of the provided date
    /// </summary>
    /// <param name="transaction">Transaction to be determined if it's pending</param>
    /// <param name="account">Account the Transaction may be pending for</param>
    /// <param name="asOfDate">Date to determine if the Transaction is pending as of</param>
    /// <returns>True if the Transaction is pending for the provided Account as of the provided date, false otherwise</returns>
    private static bool IsTransactionPending(Transaction transaction, Account account, DateOnly asOfDate)
    {
        TransactionDetail transactionDetail = transaction.DebitDetail?.AccountId == account.Id
            ? transaction.DebitDetail
            : transaction.CreditDetail ?? throw new InvalidOperationException();
        if (!transactionDetail.IsPosted || transactionDetail.StatementDate == null)
        {
            return true;
        }
        return transaction.AccountingDate <= asOfDate && asOfDate < transactionDetail.StatementDate;
    }

    /// <summary>
    /// Calculates the new balance for an Account given a Transaction.
    /// </summary>
    /// <param name="currentBalance">The current balance of the Account</param>
    /// <param name="transaction">Transaction to apply to the Account</param>
    /// <param name="account">Account the Transaction should be applied against</param>
    /// <returns>The new balance for an Account after this Transaction is applied</returns>
    private static decimal ApplyTransactionToAccountBalance(decimal currentBalance, Transaction transaction, Account account)
    {
        decimal transactionTotal = transaction.AccountingEntries.Sum(entry => entry.Amount);
        bool isDebit = transaction.DebitDetail?.AccountId == account.Id;
        if (isDebit && account.Type == AccountType.Debt)
        {
            return currentBalance + transactionTotal;
        }
        if (!isDebit && account.Type != AccountType.Debt)
        {
            return currentBalance + transactionTotal;
        }
        return currentBalance - transactionTotal;
    }
}