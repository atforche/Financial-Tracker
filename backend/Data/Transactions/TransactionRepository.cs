using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Transactions;

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
            .AsSplitQuery()
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForAccount(Account account) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.Source.Account.Id == account.Id || t.Destinations.Any(d => d.Account != null && d.Account.Id == account.Id)) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => (t.Source.Account != null && t.Source.Account.Id == account.Id) || t.Destinations.Any(d => d.Account.Id == account.Id)) ||
        databaseContext.Transactions.OfType<RefundTransaction>()
            .Any(t => t.Sources.Any(s => s.Account != null && s.Account.Id == account.Id) || t.Destination.Account.Id == account.Id) ||
        databaseContext.Transactions.OfType<AccountTransaction>()
            .Any(t => (t.Source.Account != null && t.Source.Account.Id == account.Id) || t.Destinations.Any(d => d.Account != null && d.Account.Id == account.Id));

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Transactions.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId).AsSplitQuery().ToList();

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.Destinations.Any(d => d.FundAssignments.Any(f => f.FundId == fundId))) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => t.Destinations.Any(d => d.FundAssignments.Any(f => f.FundId == fundId))) ||
        databaseContext.Transactions.OfType<RefundTransaction>()
            .Any(t => t.Sources.Any(s => s.FundAssignments.Any(f => f.FundId == fundId))) ||
        databaseContext.Transactions.OfType<FundTransaction>()
            .Any(t => t.Source.Fund.Id == fundId || t.Destinations.Any(d => d.Fund.Id == fundId));

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => databaseContext.Transactions.AsSplitQuery().Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = databaseContext.Transactions.AsSplitQuery().SingleOrDefault(candidate => candidate.Id == new TransactionId(id))
            ?? databaseContext.Transactions.Local.SingleOrDefault(candidate => candidate.Id == new TransactionId(id));
        return transaction != null;
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => databaseContext.Remove(transaction);
}
