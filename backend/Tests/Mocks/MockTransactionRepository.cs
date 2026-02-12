using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
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
    public int GetNextSequenceForDate(DateOnly transactionDate)
    {
        var transactionsOnDate = _transactions.Values.Where(transaction => transaction.Date == transactionDate).ToList();
        return transactionsOnDate.Count == 0 ? 1 : transactionsOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccount(AccountId accountId) =>
        _transactions.Values.Where(transaction =>
            (transaction.DebitAccount != null && transaction.DebitAccount.AccountId == accountId) ||
            (transaction.CreditAccount != null && transaction.CreditAccount.AccountId == accountId)).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Values.Where(transaction => transaction.AccountingPeriod == accountingPeriodId)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Sequence)
            .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByFund(FundId fundId) =>
        _transactions.Values.Where(transaction =>
            (transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)) ||
            (transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId))).ToList();

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => _transactions[id.Value];

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Transaction? transaction) => _transactions.TryGetValue(id, out transaction);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAll() => _transactions.Values;

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction.Id.Value, transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => _transactions.Remove(transaction.Id.Value);
}