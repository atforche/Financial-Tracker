using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository(DatabaseContext databaseContext) : ITransactionRepository
{
    /// <inheritdoc/>
    public int GetNextSequenceForDate(DateOnly transactionDate)
    {
        var historiesOnDate = databaseContext.Transactions
            .Where(transaction => transaction.Date == transactionDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccount(AccountId accountId)
    {
        var transferTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.DebitAccount != null && transaction.DebitAccount.AccountId == accountId &&
                transaction.CreditAccount != null && transaction.CreditAccount.AccountId == accountId)
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.DebitAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .ToList();
        var debitTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.DebitAccount != null && transaction.DebitAccount.AccountId == accountId &&
                (transaction.CreditAccount == null || transaction.CreditAccount.AccountId != accountId))
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.DebitAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .ToList();
        var creditTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.CreditAccount != null && transaction.CreditAccount.AccountId == accountId &&
                (transaction.DebitAccount == null || transaction.DebitAccount.AccountId != accountId))
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.CreditAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .ToList();
        return transferTransactions.Union(debitTransactions).Union(creditTransactions)
            .OrderBy(item => item.history?.Date ?? DateOnly.MaxValue)
            .ThenBy(item => item.history?.Sequence ?? int.MaxValue)
            .ThenBy(item => item.transaction.Date)
            .ThenBy(item => item.transaction.Sequence)
            .Select(item => item.transaction)
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Transactions.Where(transaction => transaction.AccountingPeriod == accountingPeriodId)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Sequence)
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByFund(FundId fundId)
    {
        IQueryable<Transaction> transferTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId) &&
                transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId));
        IQueryable<Transaction> debitTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId) &&
                (transaction.CreditAccount == null || !transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)));
        IQueryable<Transaction> creditTransactions = databaseContext.Transactions
            .Where(transaction =>
                transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId) &&
                (transaction.DebitAccount == null || !transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)));
        return transferTransactions.Union(debitTransactions).Union(creditTransactions)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Sequence)
            .ToList();
    }

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => databaseContext.Transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = databaseContext.Transactions.FirstOrDefault(transaction => ((Guid)(object)transaction.Id) == id);
        return transaction != null;
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => databaseContext.Remove(transaction);
}