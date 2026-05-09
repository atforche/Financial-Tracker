using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Data.Transactions;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository(DatabaseContext databaseContext) : ITransactionRepository
{
    #region ITransactionRepository

    /// <inheritdoc/>
    public int GetNextSequenceForDate(DateOnly transactionDate)
    {
        var historiesOnDate = databaseContext.Transactions
            .Where(transaction => transaction.Date == transactionDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForAccount(Account account) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => account.InitialTransaction != t.Id && (t.DebitAccountId == account.Id || t.CreditAccountId == account.Id)) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => account.InitialTransaction != t.Id && (t.DebitAccountId == account.Id || t.CreditAccountId == account.Id)) ||
        databaseContext.Transactions.OfType<AccountTransaction>()
            .Any(t => account.InitialTransaction != t.Id && (t.DebitAccountId == account.Id || t.CreditAccountId == account.Id));

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.FundAssignments.Any(f => f.FundId == fundId)) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => t.FundAssignments.Any(f => f.FundId == fundId)) ||
        databaseContext.Transactions.OfType<FundTransaction>()
            .Any(t => (t.DebitFundId == fundId) || (t.CreditFundId == fundId));

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => databaseContext.Transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => databaseContext.Remove(transaction);

    #endregion

    /// <summary>
    /// Attempts to get the Transaction with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = databaseContext.Transactions.FirstOrDefault(transaction => ((Guid)(object)transaction.Id) == id);
        return transaction != null;
    }

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Account, optionally filtered by Accounting Period
    /// </summary>
    public IReadOnlyCollection<Transaction> GetAllByAccount(AccountId accountId, AccountingPeriodId? accountingPeriodId = null)
    {
        IQueryable<Transaction> transactions = databaseContext.Transactions;
        if (accountingPeriodId != null)
        {
            transactions = transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId);
        }
        var transactionsList = transactions.ToList();
        return transactionsList.Where(transaction =>
            (transaction is SpendingTransaction spendingTransaction && (spendingTransaction.DebitAccountId == accountId || spendingTransaction.CreditAccountId == accountId)) ||
            (transaction is IncomeTransaction incomeTransaction && (incomeTransaction.DebitAccountId == accountId || incomeTransaction.CreditAccountId == accountId)) ||
            (transaction is AccountTransaction accountTransaction && (accountTransaction.DebitAccountId == accountId || accountTransaction.CreditAccountId == accountId)))
            .ToList();
    }

    /// <summary>
    /// Gets all the Transactions that are associated with the specified Fund, optionally filtered by Accounting Period
    /// </summary>
    public IReadOnlyCollection<Transaction> GetAllByFund(FundId fundId, AccountingPeriodId? accountingPeriodId = null)
    {
        IQueryable<Transaction> transactions = databaseContext.Transactions;
        if (accountingPeriodId != null)
        {
            transactions = transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId);
        }
        var transactionsList = transactions.ToList();
        return transactionsList.Where(transaction =>
            (transaction is SpendingTransaction spendingTransaction && spendingTransaction.FundAssignments.Any(f => f.FundId == fundId)) ||
            (transaction is IncomeTransaction incomeTransaction && incomeTransaction.FundAssignments.Any(f => f.FundId == fundId)) ||
            (transaction is FundTransaction fundTransaction && (fundTransaction.DebitFundId == fundId || fundTransaction.CreditFundId == fundId)))
            .ToList();
    }
}