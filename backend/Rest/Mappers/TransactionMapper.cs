using System.Diagnostics.CodeAnalysis;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transactions to Transaction Models
/// </summary>
public sealed class TransactionMapper(ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Maps the provided Transaction to a Transaction Model
    /// </summary>
    public static TransactionModel ToModel(Transaction transaction) => new()
    {
        Id = transaction.Id.Value,
        Date = transaction.Date,
        Location = transaction.Location,
        Description = transaction.Description,
        DebitAccountId = transaction.DebitAccount?.Account.Value,
        DebitFundAmounts = transaction.DebitAccount?.FundAmounts.Select(FundAmountMapper.ToModel),
        CreditAccountId = transaction.CreditAccount?.Account.Value,
        CreditFundAmounts = transaction.CreditAccount?.FundAmounts.Select(FundAmountMapper.ToModel)
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