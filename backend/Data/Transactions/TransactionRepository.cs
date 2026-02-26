using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;

namespace Data.Transactions;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository(DatabaseContext databaseContext) : ITransactionRepository
{
    #region ITransactionRepository

    /// <inheritdoc/>
    public int GetNextSequenceForDate(DateOnly transactionDate)
    {
        var historiesOnDate = databaseContext.Transactions
            .Where(transaction => transaction.Date == transactionDate)
            .ToList();
        return historiesOnDate.Count == 0 ? 1 : historiesOnDate.Max(transaction => transaction.Sequence) + 1;
    }

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForAccount(Account account) =>
        databaseContext.Transactions.Any(transaction =>
            transaction.Id != account.InitialTransaction &&
            ((transaction.DebitAccount != null && transaction.DebitAccount.AccountId == account.Id) ||
            (transaction.CreditAccount != null && transaction.CreditAccount.AccountId == account.Id)));

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        GetManyByAccountingPeriod(accountingPeriodId, new GetAccountingPeriodTransactionsRequest()).Items;

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        databaseContext.Transactions.Any(transaction =>
            (transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)) ||
            (transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == fundId)));

    /// <inheritdoc/>
    public Transaction GetById(TransactionId id) => databaseContext.Transactions.Single(transaction => transaction.Id == id);

    /// <inheritdoc/>
    public void Add(Transaction transaction) => databaseContext.Add(transaction);

    /// <inheritdoc/>
    public void Delete(Transaction transaction) => databaseContext.Remove(transaction);

    #endregion

    /// <summary>
    /// Gets the Transactions within the specified Accounting Period that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByAccountingPeriod(AccountingPeriodId accountingPeriodId, GetAccountingPeriodTransactionsRequest request)
    {
        List<AccountingPeriodTransactionSortModel> filteredTransactions = new AccountingPeriodTransactionFilterer(databaseContext).Get(accountingPeriodId, request);
        filteredTransactions.Sort(new AccountingPeriodTransactionComparer(request.SortBy));
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(filteredTransactions.Select(t => t.Transaction).ToList(), request.Limit, request.Offset),
            TotalCount = filteredTransactions.Count,
        };
    }

    /// <summary>
    /// Gets the Transactions within the specified Account that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByAccount(AccountId accountId, GetAccountTransactionsRequest request)
    {
        List<AccountTransactionSortModel> filteredTransactions = new AccountTransactionFilterer(databaseContext).Get(accountId, request);
        filteredTransactions.Sort(new AccountTransactionComparer(request.SortBy));
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(filteredTransactions.Select(t => t.Transaction).ToList(), request.Limit, request.Offset),
            TotalCount = filteredTransactions.Count,
        };
    }

    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByFund(FundId fundId, GetFundTransactionsRequest request)
    {
        List<FundTransactionSortModel> filteredTransactions = new FundTransactionFilterer(databaseContext).Get(fundId, request);
        filteredTransactions.Sort(new FundTransactionComparer(request.SortBy));
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(filteredTransactions.Select(t => t.Transaction).ToList(), request.Limit, request.Offset),
            TotalCount = filteredTransactions.Count,
        };
    }

    /// <summary>
    /// Attempts to get the Transaction with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = databaseContext.Transactions.FirstOrDefault(transaction => ((Guid)(object)transaction.Id) == id);
        return transaction != null;
    }

    /// <summary>
    /// Gets the paged collection of Transactions based on the provided request
    /// </summary>
    private static List<Transaction> GetPagedTransactions(List<Transaction> sortedTransactions, int? limit, int? offset)
    {
        if (offset != null)
        {
            sortedTransactions = sortedTransactions.Skip(offset.Value).ToList();
        }
        if (limit != null)
        {
            sortedTransactions = sortedTransactions.Take(limit.Value).ToList();
        }
        return sortedTransactions;
    }
}