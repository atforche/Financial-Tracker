using System.Globalization;
using Data;
using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.Transaction;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Transactions
/// </summary>
[ApiController]
[Route("/transactions")]
public class TransactionController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionFactory _transactionFactory;
    private readonly ITransactionRepository _transactionRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="transactionFactory">Factory that constructs Transactions</param>
    /// <param name="transactionRepository">Repository of Transactions</param>
    public TransactionController(IUnitOfWork unitOfWork, ITransactionFactory transactionFactory, ITransactionRepository transactionRepository)
    {
        _unitOfWork = unitOfWork;
        _transactionFactory = transactionFactory;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Retrieves all the Transactions that fall within the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Date representing the Accounting Period</param>
    /// <returns>A collection of Transactions that fall within the provided Accounting Period</returns>
    [HttpGet("/ByAccountingPeriod/{accountingPeriod}")]
    public IReadOnlyCollection<TransactionModel> GetTransactionsByAccountingPeriod(string accountingPeriod) =>
        _transactionRepository.FindAllByAccountingPeriod(DateOnly.Parse(accountingPeriod, CultureInfo.InvariantCulture))
            .Select(ConvertToModel).ToList();

    /// <summary>
    /// Retrieves all the Transactions made against the provided Account
    /// </summary>
    /// <param name="accountId">Id of the Account</param>
    /// <returns>A collection of Transactions that were made against the provided Account</returns>
    [HttpGet("/ByAccount/{accountId}")]
    public IReadOnlyCollection<TransactionModel> GetTransactionsByAccount(Guid accountId) =>
        _transactionRepository.FindAllByAccount(accountId).Select(ConvertToModel).ToList();

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="createTransactionModel">Request to create a Transaction</param>
    /// <returns>The created Transaction</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateTransactionAsync(CreateTransactionModel createTransactionModel)
    {
        Transaction newTransaction = _transactionFactory.Create(new CreateTransactionRequest
        {
            AccountingDate = DateOnly.Parse(createTransactionModel.AccountingDate, CultureInfo.InvariantCulture),
            StatementDate = createTransactionModel.StatementDate != null
                ? DateOnly.Parse(createTransactionModel.StatementDate, CultureInfo.InvariantCulture)
                : null,
            Type = createTransactionModel.Type,
            IsPosted = createTransactionModel.IsPosted,
            AccountId = createTransactionModel.AccountId,
            AccountingEntries = createTransactionModel.AccountingEntries.Select(entry => new CreateAccountingEntryRequest
            {
                Type = entry.Type,
                Amount = entry.Amount
            })
        });
        _transactionRepository.Add(newTransaction);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return Ok(ConvertToModel(newTransaction));
    }

    /// <summary>
    /// Deletes an existing transaction with the provided ID
    /// </summary>
    /// <param name="transactionId">ID of the Transaction to delete</param>
    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> DeleteAccountAsync(Guid transactionId)
    {
        _transactionRepository.Delete(transactionId);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    /// <summary>
    /// Converts a Transaction domain entity into a Transaction REST model
    /// </summary>
    /// <param name="transaction">Transaction domain entity to be converted</param>
    /// <returns>The converted Transaction REST model</returns>
    private TransactionModel ConvertToModel(Transaction transaction) => new()
    {
        Id = transaction.Id,
        AccountingDate = transaction.AccountingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        StatementDate = transaction.StatementDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? null,
        Type = transaction.Type,
        IsPosted = transaction.IsPosted,
        AccountId = transaction.AccountId,
        AccountingEntries = transaction.AccountingEntries.Select(ConvertToModel).ToList(),
    };

    /// <summary>
    /// Converts an AccountingEntry domain entity into an AccountingEntry REST model
    /// </summary>
    /// <param name="accountingEntry">AccountingEntry domain entity to be converted</param>
    /// <returns>The converted AccountingEntry REST model</returns>
    private AccountingEntryModel ConvertToModel(AccountingEntry accountingEntry) => new()
    {
        Type = accountingEntry.Type,
        Amount = accountingEntry.Amount,
    };
}