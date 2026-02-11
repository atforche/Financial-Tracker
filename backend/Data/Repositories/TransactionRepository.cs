using System.Diagnostics.CodeAnalysis;
using Domain.Transactions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository(DatabaseContext databaseContext) : ITransactionRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAll(TransactionFilter? filter = null)
    {
        IQueryable<Transaction> results = databaseContext.Transactions;
        if (filter?.AccountId != null)
        {
            results = results.Where(transaction =>
                (transaction.DebitAccount != null && transaction.DebitAccount.AccountId == filter.AccountId) ||
                (transaction.CreditAccount != null && transaction.CreditAccount.AccountId == filter.AccountId));
        }
        if (filter?.AccountingPeriodId != null)
        {
            results = results.Where(transaction => transaction.AccountingPeriod == filter.AccountingPeriodId);
        }
        if (filter?.FundId != null)
        {
            results = results.Where(transaction =>
                (transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == filter.FundId)) ||
                (transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == filter.FundId)));
        }
        return results.ToList();
    }

    /// <inheritdoc/>
    public Transaction FindById(TransactionId id) => databaseContext.Transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = databaseContext.Transactions.FirstOrDefault(transaction => ((Guid)(object)transaction.Id) == id);
        return transaction != null;
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => databaseContext.Remove(transaction);
}