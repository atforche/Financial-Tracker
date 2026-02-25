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
    public bool DoAnyTransactionsExistForAccount(Account account) =>
        _transactions.Values.Any(transaction => (transaction.DebitAccount?.AccountId == account.Id || transaction.CreditAccount?.AccountId == account.Id) &&
        transaction.Id != account.InitialTransaction);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Values.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Sequence)
            .ToList();

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        _transactions.Values.Any(transaction => (transaction.DebitAccount?.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId) ?? false) ||
                                (transaction.CreditAccount?.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId) ?? false));

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => _transactions[id.Value];

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction.Id.Value, transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => _transactions.Remove(transaction.Id.Value);
}