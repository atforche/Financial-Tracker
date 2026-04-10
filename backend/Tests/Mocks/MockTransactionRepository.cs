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
    private readonly Dictionary<Guid, Transaction> _transactions = [];

    /// <inheritdoc/>
    public int GetNextSequenceForDate(DateOnly transactionDate)
    {
        var transactionsOnDate = _transactions.Values.Where(transaction => transaction.Date == transactionDate).ToList();
        return transactionsOnDate.Count == 0 ? 1 : transactionsOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForAccount(Account account) =>
        _transactions.Values.Any(transaction => GetAccountIds(transaction).Contains(account.Id) &&
        transaction.Id != account.InitialTransaction);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Values.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Sequence)
            .ToList();

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        _transactions.Values.Any(transaction => GetFundIds(transaction).Contains(fundId));

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => _transactions[id.Value];

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction.Id.Value, transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => _transactions.Remove(transaction.Id.Value);

    private static IEnumerable<AccountId> GetAccountIds(Transaction transaction) => transaction switch
    {
        SpendingTransferTransaction st => [st.AccountId, st.CreditAccountId],
        SpendingTransaction s => [s.AccountId],
        IncomeTransferTransaction it => [it.DebitAccountId, it.AccountId],
        IncomeTransaction i => [i.AccountId],
        TransferTransaction t => [t.DebitAccountId, t.CreditAccountId],
        RefundTransaction r => GetAccountIds(r.Transaction),
        _ => [],
    };

    private static IEnumerable<FundId> GetFundIds(Transaction transaction) => transaction switch
    {
        SpendingTransaction s => s.FundAmounts.Select(fa => fa.FundId),
        RefundTransaction r => GetFundIds(r.Transaction),
        _ => [],
    };
}