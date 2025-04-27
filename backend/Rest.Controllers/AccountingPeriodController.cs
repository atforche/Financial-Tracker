using Data;
using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.AccountingPeriod;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accountingPeriods")]
internal sealed class AccountingPeriodController(
    IUnitOfWork unitOfWork,
    AddAccountingPeriodAction addAccountingPeriodAction,
    CloseAccountingPeriodAction closeAccountingPeriodAction,
    IAccountingPeriodService accountingPeriodService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly AddAccountingPeriodAction _addAccountingPeriodAction = addAccountingPeriodAction;
    private readonly CloseAccountingPeriodAction _closeAccountingPeriodAction = closeAccountingPeriodAction;
    private readonly IAccountingPeriodService _accountingPeriodService = accountingPeriodService;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository = accountingPeriodRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IFundRepository _fundRepository = fundRepository;

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
    /// Retrieves all the Fund Conversions for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Fund Conversions that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}/FundConversions")]
    public IActionResult GetFundConversions(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        return Ok(accountingPeriod.FundConversions.Select(fundConversion => new FundConversionModel(fundConversion)));
    }

    /// <summary>
    /// Retrieves all the Change In Values for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Change In Values that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}/ChangeInValues")]
    public IActionResult GetChangeInValues(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        return Ok(accountingPeriod.ChangeInValues.Select(changeInValue => new ChangeInValueModel(changeInValue)));
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountingPeriodAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        AccountingPeriod newAccountingPeriod = _addAccountingPeriodAction.Run(
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
        if (createTransactionModel.DebitAccountId != null)
        {
            debitAccount = _accountRepository.FindByExternalIdOrNull(createTransactionModel.DebitAccountId.Value);
            if (debitAccount == null)
            {
                return NotFound();
            }
        }
        Account? creditAccount = null;
        if (createTransactionModel.CreditAccountId != null)
        {
            creditAccount = _accountRepository.FindByExternalIdOrNull(createTransactionModel.CreditAccountId.Value);
            if (creditAccount == null)
            {
                return NotFound();
            }
        }
        var funds = _fundRepository.FindAll().ToDictionary(fund => fund.Id.ExternalId, fund => fund);
        Transaction newTransaction = _accountingPeriodService.AddTransaction(accountingPeriod,
            createTransactionModel.TransactionDate,
            debitAccount,
            creditAccount,
            createTransactionModel.AccountingEntries.Select(entry => new FundAmount
            {
                Fund = funds[entry.FundId],
                Amount = entry.Amount,
            }));
        await _unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(newTransaction));
    }

    /// <summary>
    /// Posts the provided Transaction in the provided Account
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <param name="transactionId">ID of the Transaction</param>
    /// <param name="postTransactionModel">Request to post a Transaction</param>
    /// <returns>The posted Transaction</returns>
    [HttpPost("{accountingPeriodId}/Transactions/{transactionId}")]
    public async Task<IActionResult> PostTransactionAsync(Guid accountingPeriodId, Guid transactionId, PostTransactionModel postTransactionModel)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        Transaction? transaction = accountingPeriod.Transactions
            .SingleOrDefault(transaction => transaction.Id.ExternalId == transactionId);
        if (transaction == null)
        {
            return NotFound();
        }
        Account? accountToPostIn = _accountRepository.FindByExternalIdOrNull(postTransactionModel.AccountId);
        if (accountToPostIn == null)
        {
            return NotFound();
        }
        _accountingPeriodService.PostTransaction(transaction, accountToPostIn, postTransactionModel.PostedStatementDate);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new TransactionModel(transaction));
    }

    /// <summary>
    /// Creates a new Fund Conversion with the provided properties
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <param name="createFundConversionModel">Request to create a Fund Conversion</param>
    /// <returns>The created Fund Conversion</returns>
    [HttpPost("{accountingPeriodId}/FundConversions")]
    public async Task<IActionResult> CreateFundConversionAsync(Guid accountingPeriodId, CreateFundConversionModel createFundConversionModel)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        Account? account = _accountRepository.FindByExternalIdOrNull(createFundConversionModel.AccountId);
        if (account == null)
        {
            return NotFound();
        }
        Fund? fromFund = _fundRepository.FindByExternalIdOrNull(createFundConversionModel.FromFundId);
        if (fromFund == null)
        {
            return NotFound();
        }
        Fund? toFund = _fundRepository.FindByExternalIdOrNull(createFundConversionModel.ToFundId);
        if (toFund == null)
        {
            return NotFound();
        }
        FundConversion newFundConversion = _accountingPeriodService.AddFundConversion(accountingPeriod,
            createFundConversionModel.EventDate,
            account,
            fromFund,
            toFund,
            createFundConversionModel.Amount);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new FundConversionModel(newFundConversion));
    }

    /// <summary>
    /// Creates a new Change In Value with the provided properties
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <param name="createChangeInValueModel">Request to create a Change In Value</param>
    /// <returns>The created Change In Value</returns>
    [HttpPost("{accountingPeriodId}/ChangeInValues")]
    public async Task<IActionResult> CreateChangeInValueAsync(Guid accountingPeriodId, CreateChangeInValueModel createChangeInValueModel)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindByExternalIdOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        Account? account = _accountRepository.FindByExternalIdOrNull(createChangeInValueModel.AccountId);
        if (account == null)
        {
            return NotFound();
        }
        Fund? fund = _fundRepository.FindByExternalIdOrNull(createChangeInValueModel.AccountingEntry.FundId);
        if (fund == null)
        {
            return NotFound();
        }
        var funds = _fundRepository.FindAll().ToDictionary(fund => fund.Id.ExternalId, fund => fund);
        ChangeInValue newChangeInValue = _accountingPeriodService.AddChangeInValue(accountingPeriod,
            createChangeInValueModel.EventDate,
            account,
            new FundAmount
            {
                Fund = fund,
                Amount = createChangeInValueModel.AccountingEntry.Amount
            });
        await _unitOfWork.SaveChangesAsync();
        return Ok(new ChangeInValueModel(newChangeInValue));
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
        _closeAccountingPeriodAction.Run(accountingPeriod);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(accountingPeriod));
    }
}