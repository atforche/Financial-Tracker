using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
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
    public bool DoAnyTransactionsExistForAccount(Account account)
    {
        AccountId accountId = account.Id;
        return databaseContext.Transactions.OfType<SpendingTransaction>()
                   .Any(t => t.AccountId == accountId) ||
               databaseContext.Transactions.OfType<SpendingTransferTransaction>()
                   .Any(t => t.CreditAccountId == accountId) ||
               databaseContext.Transactions.OfType<IncomeTransaction>()
                   .Any(t => t.AccountId == accountId && t.Id != account.InitialTransaction) ||
               databaseContext.Transactions.OfType<IncomeTransferTransaction>()
                   .Any(t => t.DebitAccountId == accountId) ||
               databaseContext.Transactions.OfType<AccountTransferTransaction>()
                   .Any(t => t.DebitAccountId == accountId || t.CreditAccountId == accountId);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        GetManyByAccountingPeriod(accountingPeriodId, new GetAccountingPeriodTransactionsRequest()).Items;

    /// <inheritdoc/>
    public bool DoAnyTransactionsExistForFund(FundId fundId) =>
        databaseContext.Transactions.OfType<SpendingTransaction>()
            .Any(t => t.FundAssignments.Any(f => f.FundId == fundId));

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
                        left join Accounts as Account1 on COALESCE(Transactions.SpendingTransaction_AccountId, Transactions.IncomeTransaction_AccountId) = Account1.Id
                        left join Accounts as Account2 on COALESCE(Transactions.SpendingTransferTransaction_CreditAccountId, Transactions.TransferTransaction_CreditAccountId) = Account2.Id
                        left join Accounts as Account3 on COALESCE(Transactions.IncomeTransferTransaction_DebitAccountId, Transactions.TransferTransaction_DebitAccountId) = Account3.Id
                        where Transactions.AccountingPeriodId = '{accountingPeriodId.Value.ToString().ToUpperInvariant()}'
                        """;
        if (request.Search is not null and not "")
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%' or
                            Account1.Name like '%{request.Search}%' or 
                            Account2.Name like '%{request.Search}%' or
                            Account3.Name like '%{request.Search}%' or
                            Transactions.Amount like '%{request.Search}%')
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
                        where (Transactions.SpendingTransaction_AccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                               Transactions.IncomeTransaction_AccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                               Transactions.SpendingTransferTransaction_CreditAccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                               Transactions.TransferTransaction_CreditAccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                               Transactions.IncomeTransferTransaction_DebitAccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or
                               Transactions.TransferTransaction_DebitAccountId = '{account.Id.Value.ToString().ToUpperInvariant()}')
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
            bool isDebt = account.Type == AccountType.Debt;
            AccountId accountId = account.Id;
            if (transaction is SpendingTransferTransaction stt)
            {
                if (stt.AccountId == accountId && stt.CreditAccountId == accountId)
                {
                    return 0;
                }
                if (stt.AccountId == accountId)
                {
                    return isDebt ? transaction.Amount : -transaction.Amount;
                }
                if (stt.CreditAccountId == accountId)
                {
                    return isDebt ? -transaction.Amount : transaction.Amount;
                }
            }
            else if (transaction is SpendingTransaction st)
            {
                if (st.AccountId == accountId)
                {
                    return isDebt ? transaction.Amount : -transaction.Amount;
                }
            }
            else if (transaction is IncomeTransferTransaction itt)
            {
                if (itt.AccountId == accountId && itt.DebitAccountId == accountId)
                {
                    return 0;
                }
                if (itt.AccountId == accountId)
                {
                    return isDebt ? -transaction.Amount : transaction.Amount;
                }
                if (itt.DebitAccountId == accountId)
                {
                    return isDebt ? transaction.Amount : -transaction.Amount;
                }
            }
            else if (transaction is IncomeTransaction it)
            {
                if (it.AccountId == accountId)
                {
                    return isDebt ? -transaction.Amount : transaction.Amount;
                }
            }
            else if (transaction is AccountTransferTransaction trt)
            {
                if (trt.DebitAccountId == accountId && trt.CreditAccountId == accountId)
                {
                    return 0;
                }
                if (trt.DebitAccountId == accountId)
                {
                    return isDebt ? transaction.Amount : -transaction.Amount;
                }
                if (trt.CreditAccountId == accountId)
                {
                    return isDebt ? -transaction.Amount : transaction.Amount;
                }
            }
            return 0;
        }
    }

    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByFund(FundId fundId, GetFundTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        where exists (
                            select 1 from SpendingTransactionFundAmounts
                            where SpendingTransactionFundAmounts.SpendingTransactionId = Transactions.Id
                            and SpendingTransactionFundAmounts.FundId = '{fundId.Value.ToString().ToUpperInvariant()}')
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
            if (transaction is SpendingTransaction st)
            {
                return -(st.FundAssignments.FirstOrDefault(f => f.FundId == fundId)?.Amount ?? 0);
            }
            return 0;
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