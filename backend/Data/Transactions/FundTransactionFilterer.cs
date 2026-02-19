using Domain.Funds;
using Domain.Transactions;

namespace Data.Transactions;

/// <summary>
/// Filter class responsible for filtering Transactions within a Fund based on the specified criteria
/// </summary>
internal sealed class FundTransactionFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Transactions within a Fund that match the specified criteria
    /// </summary>
    public List<FundTransactionSortModel> Get(FundId fundId, GetFundTransactionsRequest request)
    {
        IQueryable<Transaction> transfers = databaseContext.Transactions
            .Where(transaction => (transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(amount => amount.FundId == fundId)) ||
                                  (transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(amount => amount.FundId == fundId)))
            .AsQueryable();
        IQueryable<Transaction> debits = databaseContext.Transactions
            .Where(transaction => transaction.DebitAccount != null && transaction.DebitAccount.FundAmounts.Any(amount => amount.FundId == fundId) &&
                (transaction.CreditAccount == null || transaction.CreditAccount.FundAmounts.All(amount => amount.FundId != fundId)))
            .AsQueryable();
        IQueryable<Transaction> credits = databaseContext.Transactions
            .Where(transaction => transaction.CreditAccount != null && transaction.CreditAccount.FundAmounts.Any(amount => amount.FundId == fundId) &&
                (transaction.DebitAccount == null || transaction.DebitAccount.FundAmounts.All(amount => amount.FundId != fundId)))
            .AsQueryable();
        if (request.MinDate != null)
        {
            transfers = transfers.Where(transaction => transaction.Date >= request.MinDate.Value);
            debits = debits.Where(transaction => transaction.Date >= request.MinDate.Value);
            credits = credits.Where(transaction => transaction.Date >= request.MinDate.Value);
        }
        if (request.MaxDate != null)
        {
            transfers = transfers.Where(transaction => transaction.Date <= request.MaxDate.Value);
            debits = debits.Where(transaction => transaction.Date <= request.MaxDate.Value);
            credits = credits.Where(transaction => transaction.Date <= request.MaxDate.Value);
        }
        if (request.Locations != null && request.Locations.Count > 0)
        {
            transfers = transfers.Where(transaction => request.Locations.Contains(transaction.Location));
            debits = debits.Where(transaction => request.Locations.Contains(transaction.Location));
            credits = credits.Where(transaction => request.Locations.Contains(transaction.Location));
        }
        List<FundTransactionSortModel> results = [];
        if (request.Types == null || request.Types.Contains(TransactionType.Transfer))
        {
            results.AddRange(transfers.ToList().Select(transaction => new FundTransactionSortModel
            {
                Transaction = transaction,
                TransactionType = TransactionType.Transfer,
            }));
        }
        if (request.Types == null || request.Types.Contains(TransactionType.Debit))
        {
            results.AddRange(debits.ToList().Select(transaction => new FundTransactionSortModel
            {
                Transaction = transaction,
                TransactionType = TransactionType.Debit,
            }));
        }
        if (request.Types == null || request.Types.Contains(TransactionType.Credit))
        {
            results.AddRange(credits.ToList().Select(transaction => new FundTransactionSortModel
            {
                Transaction = transaction,
                TransactionType = TransactionType.Credit,
            }));
        }
        return results;
    }
}