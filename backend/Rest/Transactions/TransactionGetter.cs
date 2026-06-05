using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Transactions;
using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Transactions;

/// <summary>
/// Class that handles retrieving top-level Transactions based on specified criteria
/// </summary>
public class TransactionGetter(
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    AccountConverter accountConverter,
    FundConverter fundConverter,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Gets the Transactions that match the specified criteria
    /// </summary>
    public bool TryGet(
        TransactionQueryParameterModel request,
        out CollectionModel<TransactionModel> results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        List<AccountingPeriodId> accountingPeriodIds = [];
        foreach (Guid accountingPeriodId in request.AccountingPeriodIds ?? [])
        {
            if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
            {
                errors.Add(
                    nameof(request.AccountingPeriodIds),
                    [$"Accounting Period with ID {accountingPeriodId} was not found."]);
            }
            else
            {
                accountingPeriodIds.Add(accountingPeriod.Id);
            }
        }
        List<AccountId> accountIds = [];
        foreach (Guid accountId in request.AccountIds ?? [])
        {
            if (!accountConverter.TryToDomain(accountId, out Account? account))
            {
                errors.Add(
                    nameof(request.AccountIds),
                    [$"Account with ID {accountId} was not found."]);
            }
            else
            {
                accountIds.Add(account.Id);
            }
        }
        List<FundId> fundIds = [];
        foreach (Guid fundId in request.FundIds ?? [])
        {
            if (!fundConverter.TryToDomain(fundId, out Fund? fund))
            {
                errors.Add(
                    nameof(request.FundIds),
                    [$"Fund with ID {fundId} was not found."]);
            }
            else
            {
                fundIds.Add(fund.Id);
            }
        }

        List<Transaction> transactions = [];
        if (accountingPeriodIds.Count == 0)
        {
            transactions = transactionRepository.GetAll().ToList();
        }
        else
        {
            foreach (AccountingPeriodId accountingPeriodId in accountingPeriodIds)
            {
                transactions.AddRange(transactionRepository.GetAllByAccountingPeriod(accountingPeriodId));
            }
        }
        if (accountIds.Count > 0)
        {
            transactions = transactions.Where(transaction =>
                (transaction is SpendingTransaction spendingTransaction &&
                    (accountIds.Contains(spendingTransaction.DebitAccountId) ||
                    (spendingTransaction.CreditAccountId != null && accountIds.Contains(spendingTransaction.CreditAccountId)))) ||
                (transaction is IncomeTransaction incomeTransaction &&
                    ((incomeTransaction.DebitAccountId != null && accountIds.Contains(incomeTransaction.DebitAccountId)) ||
                    accountIds.Contains(incomeTransaction.CreditAccountId))) ||
                (transaction is AccountTransaction accountTransaction &&
                    ((accountTransaction.DebitAccountId != null && accountIds.Contains(accountTransaction.DebitAccountId)) ||
                    (accountTransaction.CreditAccountId != null && accountIds.Contains(accountTransaction.CreditAccountId)))))
                .ToList();
        }
        if (fundIds.Count > 0)
        {
            transactions = transactions.Where(transaction =>
                (transaction is FundTransaction fundTransaction &&
                    ((fundTransaction.DebitFundId != null && fundIds.Contains(fundTransaction.DebitFundId)) ||
                    (fundTransaction.CreditFundId != null && fundIds.Contains(fundTransaction.CreditFundId)))) ||
                (transaction is SpendingTransaction spendingTransaction && spendingTransaction.FundAssignments.Any(fundAssignment => fundIds.Contains(fundAssignment.FundId))) ||
                (transaction is IncomeTransaction incomeTransaction && incomeTransaction.FundAssignments.Any(fundAssignment => fundIds.Contains(fundAssignment.FundId))))
                .ToList();
        }
        var filteredResults = transactions.Select(transactionConverter.ToModel).ToList();
        if (request.Sort is null or TransactionSortOrderModel.Date)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DateDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Location)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.LocationDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DebitFrom)
        {
            filteredResults = filteredResults.OrderBy(GetDebitFrom).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DebitFromDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetDebitFrom).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.CreditTo)
        {
            filteredResults = filteredResults.OrderBy(GetCreditTo).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.CreditToDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetCreditTo).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Amount)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.AmountDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }

        results = new CollectionModel<TransactionModel>
        {
            Items = filteredResults.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = filteredResults.Count,
        };
        return true;
    }

    private static string? GetDebitFrom(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.DebitAccount?.AccountName,
        AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
        _ => null,
    };

    private static string? GetCreditTo(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.CreditAccount.AccountName,
        AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
        _ => null,
    };
}