using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
                        where Transactions.AccountingPeriodId = '{accountingPeriodId.Value.ToString().ToUpper(CultureInfo.InvariantCulture)}'
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
        if (request.Sort is null or AccountingPeriodTransactionSortOrder.DateDescending)
        {
            query += $" order by Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Date)
        {
            query += $" order by Transactions.Date asc, Transactions.Sequence asc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Location)
        {
            query += $" order by Transactions.Location asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.LocationDescending)
        {
            query += $" order by Transactions.Location desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.DebitAccount)
        {
            query += $" order by DebitAccount.Name asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.DebitAccountDescending)
        {
            query += $" order by DebitAccount.Name desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.CreditAccount)
        {
            query += $" order by CreditAccount.Name asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.CreditAccountDescending)
        {
            query += $" order by CreditAccount.Name desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.Amount)
        {
            query += $" order by Transactions.Amount asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrder.AmountDescending)
        {
            query += $" order by Transactions.Amount desc, Transactions.Date desc, Transactions.Sequence desc";
        }

        var transactions = databaseContext.Transactions.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
        };
    }

    /// <summary>
    /// Gets the Transactions within the specified Account that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByAccount(AccountId accountId, GetAccountTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        left join Accounts as DebitAccount on Transactions.DebitAccount_AccountId = DebitAccount.Id
                        left join Accounts as CreditAccount on Transactions.CreditAccount_AccountId = CreditAccount.Id
                        where (DebitAccount.Id = '{accountId.Value}' or CreditAccount.Id = '{accountId.Value}')
                        """;
        if (request.Search != null)
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%' or
                            Transactions.Amount like '%{request.Search}%')";
                        """;
        }
        if (request.Sort is null or AccountTransactionSortOrder.DateDescending)
        {
            query += $" order by Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.Date)
        {
            query += $" order by Transactions.Date asc, Transactions.Sequence asc";
        }
        else if (request.Sort == AccountTransactionSortOrder.Location)
        {
            query += $" order by Transactions.Location asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.LocationDescending)
        {
            query += $" order by Transactions.Location desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.Type)
        {
            query += $" order by case when DebitAccount.Id = {accountId} then DebitAccount.Name else CreditAccount.Name end asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.TypeDescending)
        {
            query += $" order by case when DebitAccount.Id = {accountId} then DebitAccount.Name else CreditAccount.Name end desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.Amount)
        {
            query += $" order by Transactions.Amount asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == AccountTransactionSortOrder.AmountDescending)
        {
            query += $" order by Transactions.Amount desc, Transactions.Date desc, Transactions.Sequence desc";
        }

        var transactions = databaseContext.Transactions.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
        };
    }

    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<Transaction> GetManyByFund(FundId fundId, GetFundTransactionsRequest request)
    {
        string query = $"""
                        select Transactions.* from Transactions
                        left join Accounts as DebitAccount on Transactions.DebitAccount_AccountId = DebitAccount.Id
                        left join Accounts as CreditAccount on Transactions.CreditAccount_AccountId = CreditAccount.Id
                        where (DebitFundAmounts.FundId = '{fundId.Value}' or CreditFundAmounts.FundId = '{fundId.Value}')
                        """;
        if (request.Search != null)
        {
            query += $"""
                        and (Transactions.Date like '%{request.Search}%' or
                            Transactions.Description like '%{request.Search}%' or 
                            Transactions.Location like '%{request.Search}%' or
                            Transactions.Amount like '%{request.Search}%')";
                        """;
        }
        if (request.Sort is null or FundTransactionSortOrder.DateDescending)
        {
            query += $" order by Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.Date)
        {
            query += $" order by Transactions.Date asc, Transactions.Sequence asc";
        }
        else if (request.Sort == FundTransactionSortOrder.Location)
        {
            query += $" order by Transactions.Location asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.LocationDescending)
        {
            query += $" order by Transactions.Location desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.Type)
        {
            query += $" order by case when DebitFundAmounts.FundId = {fundId} then 0 else 1 end asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.TypeDescending)
        {
            query += $" order by case when DebitFundAmounts.FundId = {fundId} then 0 else 1 end desc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.Amount)
        {
            query += $" order by Transactions.Amount asc, Transactions.Date desc, Transactions.Sequence desc";
        }
        else if (request.Sort == FundTransactionSortOrder.AmountDescending)
        {
            query += $" order by Transactions.Amount desc, Transactions.Date desc, Transactions.Sequence desc";
        }

        var transactions = databaseContext.Transactions.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Transaction>
        {
            Items = GetPagedTransactions(transactions, request.Limit, request.Offset),
            TotalCount = transactions.Count,
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