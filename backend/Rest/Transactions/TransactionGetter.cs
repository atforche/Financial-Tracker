using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
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
        List<string> accountNames = [];
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
                accountNames.Add(account.Name);
            }
        }
        List<string> fundNames = [];
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
                fundNames.Add(fund.Name);
            }
        }

        List<TransactionModel> transactions = [];
        if (accountingPeriodIds.Count == 0)
        {
            transactions = transactionRepository.GetAll().Select(transactionConverter.ToModel).ToList();
        }
        else
        {
            foreach (AccountingPeriodId accountingPeriodId in accountingPeriodIds)
            {
                transactions.AddRange(transactionRepository.GetAllByAccountingPeriod(accountingPeriodId).Select(transactionConverter.ToModel));
            }
        }
        if (accountNames.Count > 0)
        {
            transactions = transactions.Where(transaction => GetAccountNamesForTransaction(transaction).Any(accountNames.Contains)).ToList();
        }
        if (fundNames.Count > 0)
        {
            transactions = transactions.Where(transaction => GetFundNamesForTransaction(transaction).Any(fundNames.Contains)).ToList();
        }
        var filteredResults = transactions.ToList();
        if (request.Sort == TransactionSortOrderModel.Date)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort is null or TransactionSortOrderModel.DateDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.AccountingPeriod)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.AccountingPeriodName).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.AccountingPeriodDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.AccountingPeriodName).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Description)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DescriptionDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Source)
        {
            filteredResults = filteredResults.OrderBy(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.SourceDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Destination)
        {
            filteredResults = filteredResults.OrderBy(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DestinationDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
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

    private static IEnumerable<string> GetAccountNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(spendingTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        IncomeTransactionModel incomeTransaction => incomeTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(incomeTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        AccountTransactionModel accountTransaction => accountTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(accountTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        _ => [],
    };

    private static IEnumerable<string> GetFundNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Destinations.SelectMany(destination => destination.FundAssignments).Select(fundAssignment => fundAssignment.FundName),
        IncomeTransactionModel incomeTransaction => incomeTransaction.Destinations.SelectMany(destination => destination.FundAssignments).Select(fundAssignment => fundAssignment.FundName),
        FundTransactionModel fundTransaction => fundTransaction.Destinations.Select(destination => destination.Fund.FundName).Append(fundTransaction.Source.Fund.FundName),
        _ => [],
    };

    private static string? GetSource(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Source.Account.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.Source.Account?.AccountName ?? incomeTransaction.Source.Location,
        AccountTransactionModel accountTransaction => accountTransaction.Source.Account?.AccountName ?? accountTransaction.Source.Location,
        FundTransactionModel fundTransaction => fundTransaction.Source.Fund?.FundName,
        _ => null,
    };

    private static string? GetDestination(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => string.Join(", ", spendingTransaction.Destinations.Select(destination => destination.Account?.AccountName ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        IncomeTransactionModel incomeTransaction => string.Join(", ", incomeTransaction.Destinations.Select(destination => destination.Account?.AccountName).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransactionModel accountTransaction => string.Join(", ", accountTransaction.Destinations.Select(destination => destination.Account?.AccountName ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        FundTransactionModel fundTransaction => string.Join(", ", fundTransaction.Destinations.Select(destination => destination.Fund?.FundName).Distinct(StringComparer.OrdinalIgnoreCase)),
        _ => null,
    };
}