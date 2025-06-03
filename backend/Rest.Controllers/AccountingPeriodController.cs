using Data;
using Domain.AccountingPeriods;
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
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodFactory accountingPeriodFactory,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    CloseAccountingPeriodAction closeAccountingPeriodAction) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounting Periods from the database
    /// </summary>
    /// <returns>A collection of all Accounting Periods</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAll() =>
        accountingPeriodRepository.FindAll().Select(accountingPeriod => new AccountingPeriodModel(accountingPeriod)).ToList();

    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to retrieve</param>
    /// <returns>The Accounting Period that matches the provided ID</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult Get(Guid accountingPeriodId)
    {
        AccountingPeriodId id = accountingPeriodIdFactory.Create(accountingPeriodId);
        return Ok(new AccountingPeriodModel(accountingPeriodRepository.FindById(id)));
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        AccountingPeriod newAccountingPeriod = accountingPeriodFactory.Create(
            createAccountingPeriodModel.Year,
            createAccountingPeriodModel.Month);
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(newAccountingPeriod));
    }

    /// <summary>
    /// Closes the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("close/{accountingPeriodId}")]
    public async Task<IActionResult> CloseAsync(Guid accountingPeriodId)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.FindById(accountingPeriodIdFactory.Create(accountingPeriodId));
        closeAccountingPeriodAction.Run(accountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(new AccountingPeriodModel(accountingPeriod));
    }
}