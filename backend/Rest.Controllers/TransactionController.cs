using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
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
    ITransactionRepository transactionRepository,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    AccountIdFactory accountIdFactory,
    FundIdFactory fundIdFactory,
    TransactionFactory transactionFactory,
    TransactionIdFactory transactionIdFactory,
    PostTransactionAction postTransactionAction) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Transactions for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Transactions that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetByAccountingPeriod(Guid accountingPeriodId)
    {
        IEnumerable<Transaction> transactions = transactionRepository.FindAllByAccountingPeriod(accountingPeriodIdFactory.Create(accountingPeriodId));
        return Ok(transactions.Select(transaction => new TransactionModel(transaction)));
    }

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="createTransactionModel">Request to create a Transaction</param>
    /// <returns>The created Transaction</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateTransactionModel createTransactionModel)
    {
        Transaction newTransaction = transactionFactory.Create(
            accountingPeriodIdFactory.Create(createTransactionModel.AccountingPeriodId),
            createTransactionModel.Date,
            createTransactionModel.DebitAccountId != null ? accountIdFactory.Create(createTransactionModel.DebitAccountId.Value) : null,
            createTransactionModel.CreditAccountId != null ? accountIdFactory.Create(createTransactionModel.CreditAccountId.Value) : null,
            createTransactionModel.FundAmounts.Select(entry => new FundAmount
            {
                FundId = fundIdFactory.Create(entry.FundId),
                Amount = entry.Amount,
            }).ToList());
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
    public async Task<IActionResult> PostAsync(Guid transactionId, PostTransactionModel postTransactionModel)
    {
        Transaction transaction = transactionRepository.FindById(transactionIdFactory.Create(transactionId));
        postTransactionAction.Run(transaction, postTransactionModel.AccountToPost, postTransactionModel.PostedStatementDate);
        await unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(transaction));
    }
}