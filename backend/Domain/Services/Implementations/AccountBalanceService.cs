using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;

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

        Dictionary<Guid, decimal> balances = accountStartingBalance?.StartingFundBalances.ToDictionary(
            startingFundBalance => startingFundBalance.FundId,
            startingFundBalance => startingFundBalance.Amount) ?? [];
        Dictionary<Guid, decimal> balancesIncludingPendingTransactions = accountStartingBalance?.StartingFundBalances.ToDictionary(
            startingFundBalance => startingFundBalance.FundId,
            startingFundBalance => startingFundBalance.Amount) ?? [];
        foreach (Transaction transaction in transactions)
        {
            ApplyTransactionToAccountBalance(balances, transaction, account);
            if (!IsTransactionPending(transaction, account, asOfDate))
            {
                ApplyTransactionToAccountBalance(balancesIncludingPendingTransactions, transaction, account);
            }
        }
        IEnumerable<FundAmount> result = balances.Select(pair => new FundAmount(pair.Key, pair.Value));
        IEnumerable<FundAmount> resultIncludingPendingTransactions = balancesIncludingPendingTransactions
            .Select(pair => new FundAmount(pair.Key, pair.Value));
        return new AccountBalance(result, resultIncludingPendingTransactions);
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
    /// Updates the balances for an Account after applying a Transaction.
    /// </summary>
    /// <param name="currentBalance">The current balances of the Account</param>
    /// <param name="transaction">Transaction to apply to the Account</param>
    /// <param name="account">Account the Transaction should be applied against</param>
    private static void ApplyTransactionToAccountBalance(Dictionary<Guid, decimal> currentBalance,
        Transaction transaction,
        Account account)
    {
        BalanceChangeFromTransaction balanceChange = DetermineBalanceChangeFromTransaction(transaction, account);
        int adjustmentFactor = balanceChange switch
        {
            BalanceChangeFromTransaction.Increase => 1,
            BalanceChangeFromTransaction.Decrease => -1,
            _ => 1
        };
        foreach (FundAmount accountingEntry in transaction.AccountingEntries)
        {
            if (!currentBalance.TryAdd(accountingEntry.FundId, accountingEntry.Amount))
            {
                currentBalance[accountingEntry.FundId] = currentBalance[accountingEntry.FundId] +
                    (adjustmentFactor * accountingEntry.Amount);
            }
        }
    }

    /// <summary>
    /// Determines how the balance of an Account will change when a Transaction is applied
    /// </summary>
    /// <param name="transaction">Transaction to be applied to an Account</param>
    /// <param name="account">Account to have a Transaction applied</param>
    /// <returns>An enum representing how the Transaction will change the Account's balance</returns>
    private static BalanceChangeFromTransaction DetermineBalanceChangeFromTransaction(Transaction transaction, Account account)
    {
        bool isDebit = transaction.DebitDetail?.AccountId == account.Id;
        if (isDebit && account.Type == AccountType.Debt)
        {
            return BalanceChangeFromTransaction.Increase;
        }
        if (!isDebit && account.Type != AccountType.Debt)
        {
            return BalanceChangeFromTransaction.Increase;
        }
        return BalanceChangeFromTransaction.Decrease;
    }

    /// <summary>
    /// Enum that represents the ways that a Transaction can affect the balance of an Account
    /// </summary>
    private enum BalanceChangeFromTransaction
    {
        Increase,
        Decrease,
    }
}