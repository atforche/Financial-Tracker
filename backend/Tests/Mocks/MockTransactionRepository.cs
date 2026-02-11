using System.Diagnostics.CodeAnalysis;
using Domain.Transactions;

namespace Tests.Mocks;

/// <summary>
/// Mock repository of Transactions for testing
/// </summary>
internal sealed class MockTransactionRepository : ITransactionRepository
{
    private readonly Dictionary<Guid, Transaction> _transactions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MockTransactionRepository() => _transactions = new Dictionary<Guid, Transaction>();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAll(TransactionFilter? filter = null)
    {
        IEnumerable<Transaction> results = _transactions.Values;
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
    public Transaction FindById(TransactionId id) => _transactions[id.Value];

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out Transaction? transaction) => _transactions.TryGetValue(id, out transaction);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAll() => _transactions.Values;

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction.Id.Value, transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => _transactions.Remove(transaction.Id.Value);
}