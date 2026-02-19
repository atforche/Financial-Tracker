using Domain.Accounts;

namespace Data.Transactions;

/// <summary>
/// Filter class responsible for filtering Transactions within an Account based on the specified criteria
/// </summary>
internal sealed class AccountTransactionFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Transactions within an Account that match the specified criteria
    /// </summary>
    public List<AccountTransactionSortModel> Get(AccountId accountId, GetAccountTransactionsRequest request)
    {
        var transfers = databaseContext.Transactions
            .Where(transaction => (transaction.DebitAccount != null && transaction.DebitAccount.AccountId == accountId) ||
                                  (transaction.CreditAccount != null && transaction.CreditAccount.AccountId == accountId))
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.DebitAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .AsQueryable();
        var debits = databaseContext.Transactions
            .Where(transaction => transaction.DebitAccount != null && transaction.DebitAccount.AccountId == accountId &&
                (transaction.CreditAccount == null || transaction.CreditAccount.AccountId != accountId))
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.DebitAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .AsQueryable();
        var credits = databaseContext.Transactions
            .Where(transaction => transaction.CreditAccount != null && transaction.CreditAccount.AccountId == accountId &&
                (transaction.DebitAccount == null || transaction.DebitAccount.AccountId != accountId))
            .LeftJoin(databaseContext.AccountBalanceHistories,
                transaction => new { transactionId = transaction.Id, date = transaction.CreditAccount!.PostedDate },
                history => new { transactionId = history.TransactionId, date = (DateOnly?)history.Date },
                (transaction, history) => new { transaction, history })
            .AsQueryable();
        if (request.MinDate != null)
        {
            transfers = transfers.Where(pair => pair.transaction.Date >= request.MinDate);
            debits = debits.Where(pair => pair.transaction.Date >= request.MinDate);
            credits = credits.Where(pair => pair.transaction.Date >= request.MinDate);
        }
        if (request.MaxDate != null)
        {
            transfers = transfers.Where(pair => pair.transaction.Date <= request.MaxDate);
            debits = debits.Where(pair => pair.transaction.Date <= request.MaxDate);
            credits = credits.Where(pair => pair.transaction.Date <= request.MaxDate);
        }
        if (request.Locations != null && request.Locations.Count > 0)
        {
            transfers = transfers.Where(pair => request.Locations.Contains(pair.transaction.Location));
            debits = debits.Where(pair => request.Locations.Contains(pair.transaction.Location));
            credits = credits.Where(pair => request.Locations.Contains(pair.transaction.Location));
        }
        List<AccountTransactionSortModel> results = [];
        if (request.Types == null || request.Types.Contains(TransactionType.Transfer))
        {
            results.AddRange(transfers.ToList().Select(pair => new AccountTransactionSortModel
            {
                Transaction = pair.transaction,
                AccountPostedDate = pair.history?.Date,
                AccountPostedSequence = pair.history?.Sequence,
                TransactionType = TransactionType.Transfer
            }));
        }
        if (request.Types == null || request.Types.Contains(TransactionType.Debit))
        {
            results.AddRange(debits.ToList().Select(pair => new AccountTransactionSortModel
            {
                Transaction = pair.transaction,
                AccountPostedDate = pair.history?.Date,
                AccountPostedSequence = pair.history?.Sequence,
                TransactionType = TransactionType.Debit
            }));
        }
        if (request.Types == null || request.Types.Contains(TransactionType.Credit))
        {
            results.AddRange(credits.ToList().Select(pair => new AccountTransactionSortModel
            {
                Transaction = pair.transaction,
                AccountPostedDate = pair.history?.Date,
                AccountPostedSequence = pair.history?.Sequence,
                TransactionType = TransactionType.Credit
            }));
        }
        return results;
    }
}