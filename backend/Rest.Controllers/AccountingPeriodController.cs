using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.AccountingPeriods;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accountingPeriods")]
public sealed class AccountingPeriodController(
    UnitOfWork unitOfWork,
    AddAccountingPeriodAction addAccountingPeriodAction,
    CloseAccountingPeriodAction closeAccountingPeriodAction,
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