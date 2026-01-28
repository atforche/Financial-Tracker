using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transactions to Transaction Models
/// </summary>
public sealed class TransactionMapper(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    FundAmountMapper fundAmountMapper)
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
        DebitAccountId = transaction.DebitAccount?.Account.Value,
        DebitAccountName = transaction.DebitAccount != null ? accountRepository.FindById(transaction.DebitAccount.Account).Name : null,
        DebitFundAmounts = transaction.DebitAccount?.FundAmounts.Select(fundAmountMapper.ToModel),
        CreditAccountId = transaction.CreditAccount?.Account.Value,
        CreditAccountName = transaction.CreditAccount != null ? accountRepository.FindById(transaction.CreditAccount.Account).Name : null,
        CreditFundAmounts = transaction.CreditAccount?.FundAmounts.Select(fundAmountMapper.ToModel)
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
}