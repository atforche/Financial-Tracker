using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;

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
        var debitTransactionAccounts = databaseContext.Transactions
            .Where(transaction => transaction.DebitAccount != null && transaction.AccountingPeriodId == accountingPeriodId)
            .Join(databaseContext.Accounts,
                transaction => transaction.DebitAccount!.AccountId,
                account => account.Id,
                (transaction, account) => new { Type = "debit", TransactionAccount = transaction.DebitAccount, Account = account })
            .AsNoTracking()
            .AsQueryable();
        var creditTransactionAccounts = databaseContext.Transactions
            .Where(transaction => transaction.CreditAccount != null && transaction.AccountingPeriodId == accountingPeriodId)
            .Join(databaseContext.Accounts,
                transaction => transaction.CreditAccount!.AccountId,
                account => account.Id,
                (transaction, account) => new { Type = "credit", TransactionAccount = transaction.CreditAccount, Account = account })
            .AsNoTracking()
            .AsQueryable();
        if (request.MinDate != null)
        {
            debitTransactionAccounts = debitTransactionAccounts.Where(pair => pair.TransactionAccount!.Transaction.Date >= request.MinDate);
            creditTransactionAccounts = creditTransactionAccounts.Where(pair => pair.TransactionAccount!.Transaction.Date >= request.MinDate);
        }
        if (request.MaxDate != null)
        {
            debitTransactionAccounts = debitTransactionAccounts.Where(pair => pair.TransactionAccount!.Transaction.Date <= request.MaxDate);
            creditTransactionAccounts = creditTransactionAccounts.Where(pair => pair.TransactionAccount!.Transaction.Date <= request.MaxDate);
        }
        if (request.Locations != null && request.Locations.Count > 0)
        {
            debitTransactionAccounts = debitTransactionAccounts.Where(pair => request.Locations.Contains(pair.TransactionAccount!.Transaction.Location));
            creditTransactionAccounts = creditTransactionAccounts.Where(pair => request.Locations.Contains(pair.TransactionAccount!.Transaction.Location));
        }
        if (request.Accounts != null && request.Accounts.Count > 0)
        {
            debitTransactionAccounts = debitTransactionAccounts.Where(pair => request.Accounts.Contains(pair.TransactionAccount!.AccountId));
            creditTransactionAccounts = creditTransactionAccounts.Where(pair => request.Accounts.Contains(pair.TransactionAccount!.AccountId));
        }
        return debitTransactionAccounts.ToList().Concat(creditTransactionAccounts.ToList())
            .GroupBy(pair => pair.TransactionAccount!.Transaction.Id)
            .Select(pair => new AccountingPeriodTransactionSortModel
            {
                Transaction = pair.First().TransactionAccount!.Transaction,
                DebitAccountName = pair.FirstOrDefault(account => account.Type == "debit")?.Account.Name,
                CreditAccountName = pair.FirstOrDefault(account => account.Type == "credit")?.Account.Name
            }).ToList();
    }
}