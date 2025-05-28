using Domain;
using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Transactions for testing
/// </summary>
public class MockTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = [];

    /// <inheritdoc/>
    public bool DoesTransactionWithIdExist(Guid id) => _transactions.Any(transaction => transaction.Id.Value == id);

    /// <inheritdoc/>
    public Transaction FindById(TransactionId id) => _transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return _transactions
            .Where(transaction => (transaction.Date >= dates.First() && transaction.Date <= dates.Last()) ||
                transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.EventDate >= dates.First() && balanceEvent.EventDate <= dates.Last()))
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction);
}