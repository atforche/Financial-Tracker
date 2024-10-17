using System.Globalization;
using Data;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
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
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="transactionRepository">Repository of Transactions</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    public TransactionController(IUnitOfWork unitOfWork,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository)
    {
        _unitOfWork = unitOfWork;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Retrieves all the Transactions that fall within the Accounting Period that contains the provided date
    /// </summary>
    /// <param name="accountingPeriod">Date representing the Accounting Period</param>
    /// <returns>A collection of Transactions that fall within the provided Accounting Period</returns>
    [HttpGet("/ByAccountingPeriod/{accountingPeriod}")]
    public IReadOnlyCollection<TransactionModel> GetTransactionsByAccountingPeriod(DateOnly accountingPeriod) =>
        _transactionRepository.FindAllByAccountingPeriod(accountingPeriod).Select(ConvertToModel).ToList();

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
        Account? debitAccount = null;
        if (createTransactionModel.DebitDetail != null)
        {
            debitAccount = _accountRepository.FindOrNull(createTransactionModel.DebitDetail.AccountId);
            if (debitAccount == null)
            {
                return NotFound();
            }
        }
        Account? creditAccount = null;
        if (createTransactionModel.CreditDetail != null)
        {
            creditAccount = _accountRepository.FindOrNull(createTransactionModel.CreditDetail.AccountId);
            if (creditAccount == null)
            {
                return NotFound();
            }
        }

        Transaction newTransaction = new Transaction(new CreateTransactionRequest
        {
            AccountingDate = createTransactionModel.AccountingDate,
            DebitDetail = createTransactionModel.DebitDetail != null && debitAccount != null
                ? new CreateTransactionDetailRequest
                {
                    Account = debitAccount,
                    StatementDate = createTransactionModel.DebitDetail.StatementDate,
                    IsPosted = createTransactionModel.DebitDetail.IsPosted,
                }
                : null,
            CreditDetail = createTransactionModel.CreditDetail != null && creditAccount != null
                ? new CreateTransactionDetailRequest
                {
                    Account = creditAccount,
                    StatementDate = createTransactionModel.CreditDetail.StatementDate,
                    IsPosted = createTransactionModel.CreditDetail.IsPosted,
                }
                : null,
            AccountingEntries = createTransactionModel.AccountingEntries.Select(entry => entry.Amount)
        });
        _transactionRepository.Add(newTransaction);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ConvertToModel(newTransaction));
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
        DebitDetail = transaction.DebitDetail != null ? ConvertToModel(transaction.DebitDetail) : null,
        CreditDetail = transaction.CreditDetail != null ? ConvertToModel(transaction.CreditDetail) : null,
        AccountingEntries = transaction.AccountingEntries.Select(ConvertToModel).ToList(),
    };

    /// <summary>
    /// Converts a Transaction Detail domain entity into a Transaction Detail REST model
    /// </summary>
    /// <param name="transactionDetail">Transaction Detail domain entity to be converted</param>
    /// <returns>The converted Transaction Detail REST model</returns>
    private static TransactionDetailModel ConvertToModel(TransactionDetail transactionDetail) => new TransactionDetailModel
    {
        AccountId = transactionDetail.AccountId,
        StatementDate = transactionDetail.StatementDate,
        IsPosted = transactionDetail.IsPosted,
    };

    /// <summary>
    /// Converts an Accounting Entry value object into an AccountingEntry REST model
    /// </summary>
    /// <param name="accountingEntry">Accounting Entry value object to be converted</param>
    /// <returns>The converted Accounting Entry REST model</returns>
    private AccountingEntryModel ConvertToModel(AccountingEntry accountingEntry) => new AccountingEntryModel
    {
        Amount = accountingEntry.Amount,
    };
}