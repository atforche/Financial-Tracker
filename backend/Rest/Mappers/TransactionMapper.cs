using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transactions to Transaction Models
/// </summary>
public sealed class TransactionMapper(
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService,
    AccountBalanceMapper accountBalanceMapper,
    FundAmountMapper fundAmountMapper,
    FundBalanceMapper fundBalanceMapper,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Maps the provided Transaction to a Transaction Model
    /// </summary>
    public TransactionModel ToModel(Transaction transaction) => new()
    {
        Id = transaction.Id.Value,
        AccountingPeriodId = transaction.AccountingPeriod.Value,
        AccountingPeriodName = accountingPeriodRepository.FindById(transaction.AccountingPeriod).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
        Date = transaction.Date,
        Location = transaction.Location,
        Description = transaction.Description,
        Amount = transaction.Amount,
        DebitAccount = ToModel(transaction.DebitAccount),
        CreditAccount = ToModel(transaction.CreditAccount),
        PreviousFundBalances = fundBalanceService.GetPreviousBalancesForTransaction(transaction)
            .Select(fundBalanceMapper.ToModel)
            .ToList(),
        NewFundBalances = fundBalanceService.GetNewBalanceForTransaction(transaction)
            .Select(fundBalanceMapper.ToModel)
            .ToList(),
    };

    /// <summary>
    /// Attempts to map the provided ID to a Transaction
    /// </summary>
    public bool TryToDomain(
        Guid transactionId,
        [NotNullWhen(true)] out Transaction? transaction,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!transactionRepository.TryFindById(transactionId, out transaction))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Transaction with ID {transactionId} was not found.", []));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Maps the provided Transaction Account to a Transaction Account Model
    /// </summary>
    private TransactionAccountModel? ToModel(TransactionAccount? transactionAccount)
    {
        if (transactionAccount == null)
        {
            return null;
        }
        return new TransactionAccountModel
        {
            AccountId = transactionAccount.AccountId.Value,
            AccountName = accountRepository.FindById(transactionAccount.AccountId).Name,
            PostedDate = transactionAccount.PostedDate,
            FundAmounts = transactionAccount.FundAmounts.Select(fundAmountMapper.ToModel).ToList(),
            PreviousAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetPreviousBalanceForTransaction(transactionAccount)),
            NewAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetNewBalanceForTransaction(transactionAccount)),
        };
    }
}