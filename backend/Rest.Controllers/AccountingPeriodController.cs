using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.AccountingPeriod;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accountingPeriods")]
internal sealed class AccountingPeriodController(
    UnitOfWork unitOfWork,
    AddAccountingPeriodAction addAccountingPeriodAction,
    CloseAccountingPeriodAction closeAccountingPeriodAction,
    AddTransactionAction addTransactionAction,
    PostTransactionAction postTransactionAction,
    AddFundConversionAction addFundConversionAction,
    AddChangeInValueAction addChangeInValueAction,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    AccountIdFactory accountIdFactory,
    FundIdFactory fundIdFactory,
    AccountingPeriodIdFactory accountingPeriodIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounting Periods from the database
    /// </summary>
    /// <returns>A collection of all Accounting Periods</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAccountingPeriods() =>
        accountingPeriodRepository.FindAll().Select(accountingPeriod => new AccountingPeriodModel(accountingPeriod)).ToList();

    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to retrieve</param>
    /// <returns>The Accounting Period that matches the provided ID</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriodId id = accountingPeriodIdFactory.Create(accountingPeriodId);
        return Ok(new AccountingPeriodModel(accountingPeriodRepository.FindById(id)));
    }

    /// <summary>
    /// Retrieves all the Transactions for the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period</param>
    /// <returns>A collection of Transactions that fall within the provided Accounting Period</returns>
    [HttpGet("{accountingPeriodId}/Transactions")]
    public IActionResult GetTransactions(Guid accountingPeriodId)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
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
        AccountingPeriod newAccountingPeriod = addAccountingPeriodAction.Run(
            createAccountingPeriodModel.Year,
            createAccountingPeriodModel.Month);
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));

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
        await unitOfWork.SaveChangesAsync();
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
        Transaction? transaction = accountingPeriod.Transactions
            .SingleOrDefault(transaction => transaction.Id.Value == transactionId);
        if (transaction == null)
        {
            return NotFound();
        }
        postTransactionAction.Run(transaction, postTransactionModel.AccountToPost, postTransactionModel.PostedStatementDate);
        await unitOfWork.SaveChangesAsync();
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
        Account account = accountRepository.FindById(accountIdFactory.Create(createFundConversionModel.AccountId));
        Fund fromFund = fundRepository.FindById(fundIdFactory.Create(createFundConversionModel.FromFundId));
        Fund toFund = fundRepository.FindById(fundIdFactory.Create(createFundConversionModel.ToFundId));
        FundConversion newFundConversion = addFundConversionAction.Run(accountingPeriod,
            createFundConversionModel.EventDate,
            account,
            fromFund,
            toFund,
            createFundConversionModel.Amount);
        await unitOfWork.SaveChangesAsync();
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
        Account account = accountRepository.FindById(accountIdFactory.Create(createChangeInValueModel.AccountId));
        var funds = fundRepository.FindAll().ToDictionary(fund => fund.Id.Value, fund => fund);
        ChangeInValue newChangeInValue = addChangeInValueAction.Run(accountingPeriod,
            createChangeInValueModel.EventDate,
            account,
            new FundAmount
            {
                FundId = fundIdFactory.Create(createChangeInValueModel.AccountingEntry.FundId),
                Amount = createChangeInValueModel.AccountingEntry.Amount
            });
        await unitOfWork.SaveChangesAsync();
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
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
        closeAccountingPeriodAction.Run(accountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(accountingPeriod));
    }
}