using Domain;
using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository(DatabaseContext databaseContext) : ITransactionRepository
{
    /// <inheritdoc/>
    public bool DoesTransactionWithIdExist(Guid id) => databaseContext.Transactions.Any(transaction => ((Guid)(object)transaction.Id) == id);

    /// <inheritdoc/>
    public Transaction FindById(TransactionId id) => databaseContext.Transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return databaseContext.Transactions
            .Where(transaction => (transaction.Date >= dates.First() && transaction.Date <= dates.Last()) ||
                transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.EventDate >= dates.First() && balanceEvent.EventDate <= dates.Last()))
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);
}