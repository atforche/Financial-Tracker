using Domain.AccountingPeriods;

namespace Data.Transactions;

/// <summary>
/// Filter class responsible for filtering Transactions within an Accounting Period based on the specified criteria
/// </summary>
internal sealed class AccountingPeriodTransactionFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Transactions within an Accounting Period that match the specified criteria
    /// </summary>
    public List<AccountingPeriodTransactionSortModel> Get(AccountingPeriodId accountingPeriodId, GetAccountingPeriodTransactionsRequest request)
    {
        var transactionsWithAccounts = databaseContext.Transactions
            .Join(databaseContext.Accounts,
                transaction => transaction.DebitAccount != null ? transaction.DebitAccount.AccountId : null,
                account => account.Id,
                (transaction, account) => new { transaction, account })
            .Join(databaseContext.Accounts,
                pair => pair.transaction.CreditAccount != null ? pair.transaction.CreditAccount.AccountId : null,
                account => account.Id,
                (pair, account) => new { pair.transaction, debitAccount = pair.account, creditAccount = account })
            .Where(pair => pair.transaction.AccountingPeriod == accountingPeriodId)
            .AsQueryable();
        if (request.MinDate != null)
        {
            transactionsWithAccounts = transactionsWithAccounts.Where(pair => pair.transaction.Date >= request.MinDate);
        }
        if (request.MaxDate != null)
        {
            transactionsWithAccounts = transactionsWithAccounts.Where(pair => pair.transaction.Date <= request.MaxDate);
        }
        if (request.Locations != null && request.Locations.Count > 0)
        {
            transactionsWithAccounts = transactionsWithAccounts.Where(pair => request.Locations.Contains(pair.transaction.Location));
        }
        if (request.Accounts != null && request.Accounts.Count > 0)
        {
            transactionsWithAccounts = transactionsWithAccounts.Where(pair =>
                request.Accounts.Contains(pair.debitAccount.Id) ||
                request.Accounts.Contains(pair.creditAccount.Id));
        }
        return transactionsWithAccounts.ToList().Select(pair => new AccountingPeriodTransactionSortModel
        {
            Transaction = pair.transaction,
            DebitAccountName = pair.debitAccount.Name,
            CreditAccountName = pair.creditAccount.Name
        }).ToList();
    }
}