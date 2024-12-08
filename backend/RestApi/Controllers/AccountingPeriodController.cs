using Data;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.AccountingPeriod;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accountingPeriod")]
public class AccountingPeriodController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IFundRepository _fundRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="accountingPeriodService">Service used to create or modify Accounting Periods</param>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="fundRepository">Repository of Funds</param>
    public AccountingPeriodController(IUnitOfWork unitOfWork,
        IAccountingPeriodService accountingPeriodService,
        IAccountingPeriodRepository accountingPeriodRepository,
        IAccountRepository accountRepository,
        IFundRepository fundRepository)
    {
        _unitOfWork = unitOfWork;
        _accountingPeriodService = accountingPeriodService;
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountRepository = accountRepository;
        _fundRepository = fundRepository;
    }

    /// <summary>
    /// Retrieves all the Accounting Periods from the database
    /// </summary>
    /// <returns>A collection of all Accounting Periods</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAccountingPeriods() =>
        _accountingPeriodRepository.FindAll().Select(accountingPeriod => new AccountingPeriodModel(accountingPeriod)).ToList();

    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to retrieve</param>
    /// <returns>The Accounting Period that matches the provided ID</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        return accountingPeriod != null ? Ok(new AccountingPeriodModel(accountingPeriod)) : NotFound();
    }

    /// <summary>
    /// Retrieves all the Transactions for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Transactions that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}/Transactions")]
    public IActionResult GetTransactions(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        return Ok(accountingPeriod.Transactions.Select(transaction => new TransactionModel(transaction)));
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountingPeriodAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        AccountingPeriod newAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(
            createAccountingPeriodModel.Year,
            createAccountingPeriodModel.Month);
        _accountingPeriodRepository.Add(newAccountingPeriod);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(newAccountingPeriod));
    }

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <param name="createTransactionModel">Request to create a Transaction</param>
    /// <returns>The created Transaction</returns>
    [HttpPost("{accountingPeriodId}/Transactions")]
    public async Task<IActionResult> CreateTransactionAsync(Guid accountingPeriodId, CreateTransactionModel createTransactionModel)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }

        Account? debitAccount = null;
        if (createTransactionModel.DebitDetail != null)
        {
            debitAccount = _accountRepository.FindByExternalIdOrNull(createTransactionModel.DebitDetail.AccountId);
            if (debitAccount == null)
            {
                return NotFound();
            }
        }
        Account? creditAccount = null;
        if (createTransactionModel.CreditDetail != null)
        {
            creditAccount = _accountRepository.FindByExternalIdOrNull(createTransactionModel.CreditDetail.AccountId);
            if (creditAccount == null)
            {
                return NotFound();
            }
        }
        Dictionary<Guid, Fund> funds = _fundRepository.FindAll().ToDictionary(fund => fund.Id.ExternalId, fund => fund);
        Transaction newTransaction = _accountingPeriodService.AddTransaction(accountingPeriod,
            createTransactionModel.TransactionDate,
            debitAccount,
            creditAccount,
            createTransactionModel.AccountingEntries.Select(entry => new FundAmount
            {
                Fund = funds[entry.FundId],
                Amount = entry.Amount,
            }));
        if (debitAccount != null && createTransactionModel.DebitDetail?.PostedStatementDate != null)
        {
            newTransaction.Post(debitAccount, createTransactionModel.DebitDetail.PostedStatementDate.Value);
        }
        if (creditAccount != null && createTransactionModel.CreditDetail?.PostedStatementDate != null)
        {
            newTransaction.Post(creditAccount, createTransactionModel.CreditDetail.PostedStatementDate.Value);
        }
        await _unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(newTransaction));
    }

    /// <summary>
    /// Closes the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("close/{accountingPeriodId}")]
    public async Task<IActionResult> CloseAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        _accountingPeriodService.ClosePeriod(accountingPeriod);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(accountingPeriod));
    }
}