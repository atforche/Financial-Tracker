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
    public IReadOnlyCollection<Transaction> FindAllByAccount(AccountId accountId) => _transactions.Values.Where(transaction =>
        (transaction.DebitAccount != null && transaction.DebitAccount.Account == accountId) ||
        (transaction.CreditAccount != null && transaction.CreditAccount.Account == accountId)).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Values.Where(transaction => transaction.AccountingPeriod == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByFund(FundId fundId) => _transactions.Values.Where(transaction =>
        (transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)) ||
        (transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId))).ToList();

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