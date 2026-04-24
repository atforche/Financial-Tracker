using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

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
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.DebitAccountId == account.Id || t.CreditAccountId == account.Id) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => t.DebitAccountId == account.Id || t.CreditAccountId == account.Id) ||
        databaseContext.Transactions.OfType<AccountTransaction>()
            .Any(t => t.DebitAccountId == account.Id || t.CreditAccountId == account.Id);

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        GetManyByAccountingPeriod(accountingPeriodId, new GetAccountingPeriodTransactionsRequest()).Items;

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.FundAssignments.Any(f => f.FundId == fundId)) ||
        databaseContext.Transactions.OfType<IncomeTransaction>()
            .Any(t => t.FundAssignments.Any(f => f.FundId == fundId)) ||
        databaseContext.Transactions.OfType<FundTransaction>()
            .Any(t => (t.DebitFundId == fundId) || (t.CreditFundId == fundId));

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
        string query = $"""
                        select Transactions.* from Transactions
                        left join Accounts as DebitAccount on COALESCE(Transactions.SpendingTransaction_DebitAccountId, Transactions.IncomeTransaction_DebitAccountId, Transactions.AccountTransaction_DebitAccountId) = DebitAccount.Id
                        left join Accounts as CreditAccount on COALESCE(Transactions.SpendingTransaction_CreditAccountId, Transactions.IncomeTransaction_CreditAccountId, Transactions.AccountTransaction_CreditAccountId) = CreditAccount.Id
                        where Transactions.AccountingPeriodId = '{accountingPeriodId.Value.ToString().ToUpperInvariant()}'
                        """;
        if (request.Search is not null and not "")
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%' or
                            DebitAccount.Name like '%{request.Search}%' or 
                            CreditAccount.Name like '%{request.Search}%')
                        """;
        }

        IQueryable<Transaction> transactionsQuery = databaseContext.Transactions.FromSqlRaw(query);
        if (request.Sort is null or AccountingPeriodTransactionSortOrder.DateDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Date).ThenByDescending(t => t.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Date)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Date).ThenBy(t => t.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Location)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.LocationDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Amount)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Amount).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.AmountDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Amount).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence);
        }

        var transactions = transactionsQuery.ToList();
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
        };
    }

    /// <summary>
    /// Gets the Transactions within the specified Account that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByAccount(Account account, GetAccountTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        where (
                            COALESCE(Transactions.SpendingTransaction_DebitAccountId, Transactions.IncomeTransaction_DebitAccountId, Transactions.AccountTransaction_DebitAccountId) = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                            COALESCE(Transactions.SpendingTransaction_CreditAccountId, Transactions.IncomeTransaction_CreditAccountId, Transactions.AccountTransaction_CreditAccountId) = '{account.Id.Value.ToString().ToUpperInvariant()}'
                        )
                        """;
        if (request.AccountingPeriodId != null)
        {
            query += $" and Transactions.AccountingPeriodId = '{request.AccountingPeriodId.Value.ToString().ToUpperInvariant()}'";
        }
        if (request.Search != null)
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%')
                        """;
        }

        var transactions = databaseContext.Transactions.FromSqlRaw(query).ToList();
        if (request.Sort is null or AccountTransactionSortOrder.DateDescending)
        {
            transactions = transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == AccountTransactionSortOrder.Date)
        {
            transactions = transactions.OrderBy(t => t.Date).ThenBy(t => t.Sequence).ToList();
        }
        else if (request.Sort == AccountTransactionSortOrder.Location)
        {
            transactions = transactions.OrderBy(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == AccountTransactionSortOrder.LocationDescending)
        {
            transactions = transactions.OrderByDescending(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == AccountTransactionSortOrder.ChangeInBalance)
        {
            transactions = transactions.OrderBy(GetChangeInBalance).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == AccountTransactionSortOrder.ChangeInBalanceDescending)
        {
            transactions = transactions.OrderByDescending(GetChangeInBalance).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
        };

        decimal GetChangeInBalance(Transaction transaction)
        {
            var oldAccountBalance = new AccountBalance(account, 0, 0, 0);
            AccountBalance newAccountBalance = transaction.ApplyToAccountBalance(oldAccountBalance);
            return newAccountBalance.PostedBalance - oldAccountBalance.PostedBalance;
        }
    }

    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByFund(FundId fundId, GetFundTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        where 
                            exists (
                                select 1 from SpendingTransactionFundAmounts
                                where SpendingTransactionFundAmounts.SpendingTransactionId = Transactions.Id
                                and SpendingTransactionFundAmounts.FundId = '{fundId.Value.ToString().ToUpperInvariant()}')
                            or exists (
                                select 1 from IncomeTransactionFundAmounts
                                where IncomeTransactionFundAmounts.IncomeTransactionId = Transactions.Id
                                and IncomeTransactionFundAmounts.FundId = '{fundId.Value.ToString().ToUpperInvariant()}')
                            or Transactions.FundTransaction_DebitFundId = '{fundId.Value.ToString().ToUpperInvariant()}'
                            or Transactions.FundTransaction_CreditFundId = '{fundId.Value.ToString().ToUpperInvariant()}'
                        )
                        """;
        if (request.AccountingPeriodId != null)
        {
            query += $" and Transactions.AccountingPeriodId = '{request.AccountingPeriodId.Value.ToString().ToUpperInvariant()}'";
        }
        if (request.Search != null)
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%')
                        """;
        }

        var transactions = databaseContext.Transactions.FromSqlRaw(query).ToList();
        if (request.Sort is null or FundTransactionSortOrder.DateDescending)
        {
            transactions = transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrder.Date)
        {
            transactions = transactions.OrderBy(t => t.Date).ThenBy(t => t.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrder.Location)
        {
            transactions = transactions.OrderBy(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrder.LocationDescending)
        {
            transactions = transactions.OrderByDescending(t => t.Location).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrder.ChangeInBalance)
        {
            transactions = transactions.OrderBy(GetChangeInBalance).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrder.ChangeInBalanceDescending)
        {
            transactions = transactions.OrderByDescending(GetChangeInBalance).ThenByDescending(t => t.Date).ThenByDescending(t => t.Sequence).ToList();
        }
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
        };

        decimal GetChangeInBalance(Transaction transaction)
        {
            var oldFundBalance = new FundBalance(fundId, 0, 0, 0);
            FundBalance newFundBalance = transaction.ApplyToFundBalance(oldFundBalance);
            return newFundBalance.PostedBalance - oldFundBalance.PostedBalance;
        }
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