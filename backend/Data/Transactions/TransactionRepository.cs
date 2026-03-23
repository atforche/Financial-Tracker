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
        string query = $"""
                        select Transactions.* from Transactions
                        left join Accounts as DebitAccount on Transactions.DebitAccount_AccountId = DebitAccount.Id
                        left join Accounts as CreditAccount on Transactions.CreditAccount_AccountId = CreditAccount.Id
                        where Transactions.AccountingPeriodId = '{accountingPeriodId.Value.ToString().ToUpperInvariant()}'
                        """;
        if (request.Search is not null and not "")
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%' or
                            DebitAccount.Name like '%{request.Search}%' or 
                            CreditAccount.Name like '%{request.Search}%' or 
                            Transactions.Amount like '%{request.Search}%')
                        """;
        }

        var transactionsQuery = databaseContext.Transactions.FromSqlRaw(query)
            .LeftJoin(databaseContext.Accounts,
                transaction => transaction.DebitAccount != null ? transaction.DebitAccount.AccountId : null,
                account => account.Id,
                (transaction, debitAccount) => new { Transaction = transaction, DebitAccountName = debitAccount != null ? debitAccount.Name : null })
            .LeftJoin(databaseContext.Accounts,
                transaction => transaction.Transaction.CreditAccount != null ? transaction.Transaction.CreditAccount.AccountId : null,
                account => account.Id,
                (transaction, creditAccount) => new { transaction.Transaction, transaction.DebitAccountName, CreditAccountName = creditAccount != null ? creditAccount.Name : null });
        if (request.Sort is null or AccountingPeriodTransactionSortOrder.DateDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Date)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Transaction.Date).ThenBy(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Location)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Transaction.Location).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.LocationDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Transaction.Location).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.DebitAccount)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.DebitAccountName).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.DebitAccountDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.DebitAccountName).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.CreditAccount)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.CreditAccountName).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.CreditAccountDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.CreditAccountName).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Amount)
        {
            transactionsQuery = transactionsQuery.OrderBy(t => t.Transaction.Amount).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.AmountDescending)
        {
            transactionsQuery = transactionsQuery.OrderByDescending(t => t.Transaction.Amount).ThenByDescending(t => t.Transaction.Date).ThenByDescending(t => t.Transaction.Sequence);
        }

        var transactions = transactionsQuery.ToList().Select(transaction => transaction.Transaction).ToList();
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
                        where (Transactions.DebitAccount_AccountId = '{account.Id.Value.ToString().ToUpperInvariant()}' or Transactions.CreditAccount_AccountId = '{account.Id.Value.ToString().ToUpperInvariant()}') 
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
            if (transaction.DebitAccount?.AccountId == account.Id && transaction.CreditAccount?.AccountId == account.Id)
            {
                return 0;
            }
            if (transaction.DebitAccount?.AccountId == account.Id)
            {
                return account.Type == AccountType.Debt ? transaction.Amount : -transaction.Amount;
            }
            return account.Type == AccountType.Debt ? -transaction.Amount : transaction.Amount;
        }
    }

    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByFund(FundId fundId, GetFundTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        where (exists (select 1 from TransactionDebitAccountFundAmounts where TransactionDebitAccountFundAmounts.TransactionAccountTransactionId = Transactions.Id and TransactionDebitAccountFundAmounts.FundId = '{fundId.Value.ToString().ToUpperInvariant()}')
                            or exists (select 1 from TransactionCreditAccountFundAmounts where TransactionCreditAccountFundAmounts.TransactionAccountTransactionId = Transactions.Id and TransactionCreditAccountFundAmounts.FundId = '{fundId.Value.ToString().ToUpperInvariant()}'))
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
            decimal debitFundAmount = transaction.DebitAccount?.FundAmounts.FirstOrDefault(fundAmount => fundAmount.FundId == fundId)?.Amount ?? 0;
            decimal creditFundAmount = transaction.CreditAccount?.FundAmounts.FirstOrDefault(fundAmount => fundAmount.FundId == fundId)?.Amount ?? 0;
            return creditFundAmount - debitFundAmount;
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