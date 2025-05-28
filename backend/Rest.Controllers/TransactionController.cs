using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Transactions;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Transactions
/// </summary>
[ApiController]
[Route("/transactions")]
public sealed class TransactionController(
    UnitOfWork unitOfWork,
    AddTransactionAction addTransactionAction,
    PostTransactionAction postTransactionAction,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    AccountIdFactory accountIdFactory,
    FundIdFactory fundIdFactory,
    TransactionIdFactory transactionIdFactory) : ControllerBase
{
    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="createTransactionModel">Request to create a Transaction</param>
    /// <returns>The created Transaction</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateTransactionAsync(CreateTransactionModel createTransactionModel)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(createTransactionModel.AccountingPeriodId));

        Account? debitAccount = null;
        if (createTransactionModel.DebitAccountId != null)
        {
            debitAccount = accountRepository.FindById(accountIdFactory.Create(createTransactionModel.DebitAccountId.Value));
        }
        Account? creditAccount = null;
        if (createTransactionModel.CreditAccountId != null)
        {
            creditAccount = accountRepository.FindById(accountIdFactory.Create(createTransactionModel.CreditAccountId.Value));
        }
        Transaction newTransaction = addTransactionAction.Run(accountingPeriod,
            createTransactionModel.Date,
            debitAccount,
            creditAccount,
            createTransactionModel.AccountingEntries.Select(entry => new FundAmount
            {
                FundId = fundIdFactory.Create(entry.FundId),
                Amount = entry.Amount,
            }));
        transactionRepository.Add(newTransaction);
        await unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(newTransaction));
    }

    /// <summary>
    /// Posts the provided Transaction in the provided Account
    /// </summary>
    /// <param name="transactionId">ID of the Transaction</param>
    /// <param name="postTransactionModel">Request to post a Transaction</param>
    /// <returns>The posted Transaction</returns>
    [HttpPost("/post/{transactionId}")]
    public async Task<IActionResult> PostTransactionAsync(Guid transactionId, PostTransactionModel postTransactionModel)
    {
        Transaction transaction = transactionRepository.FindById(transactionIdFactory.Create(transactionId));
        postTransactionAction.Run(transaction, postTransactionModel.AccountToPost, postTransactionModel.PostedStatementDate);
        await unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(transaction));
    }
}