using Data;
using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;
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
    private readonly IAccountingPeriodFactory _accountingPeriodFactory;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountingPeriodController(IUnitOfWork unitOfWork,
        IAccountingPeriodFactory accountingPeriodFactory,
        IAccountingPeriodRepository accountingPeriodRepository)
    {
        _unitOfWork = unitOfWork;
        _accountingPeriodFactory = accountingPeriodFactory;
        _accountingPeriodRepository = accountingPeriodRepository;
    }

    /// <summary>
    /// Retrieves all the Accounting Periods from the database
    /// </summary>
    /// <returns>A collection of all Accounting Periods</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAccountingPeriods() =>
        _accountingPeriodRepository.FindAll().Select(ConvertToModel).ToList();

    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to retrieve</param>
    /// <returns>The Accounting Period that matches the provided ID</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult GetAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindOrNull(accountingPeriodId);
        return accountingPeriod != null ? Ok(ConvertToModel(accountingPeriod)) : NotFound();
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountingPeriodAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        AccountingPeriod newAccountingPeriod = _accountingPeriodFactory.Create(createAccountingPeriodModel.Year,
            createAccountingPeriodModel.Month);
        _accountingPeriodRepository.Add(newAccountingPeriod);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return Ok(ConvertToModel(newAccountingPeriod));
    }

    /// <summary>
    /// Closes the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("/close/{accountingPeriodId}")]
    public async Task<IActionResult> CloseAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        accountingPeriod.CloseAccountingPeriod();
        _accountingPeriodRepository.Update(accountingPeriod);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return Ok(ConvertToModel(accountingPeriod));
    }

    /// <summary>
    /// Deletes an existing Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to delete</param>
    [HttpDelete("{accountingPeriodId}")]
    public async Task<IActionResult> DeleteAccountingPeriodAsync(Guid accountingPeriodId)
    {
        _accountingPeriodRepository.Delete(accountingPeriodId);
        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        return Ok();
    }

    /// <summary>
    /// Converts the Accounting Period domain entity into an Accounting Period REST model
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period domain entity to be converted</param>
    /// <returns>The converted Accounting Period REST model</returns>
    private AccountingPeriodModel ConvertToModel(AccountingPeriod accountingPeriod) => new AccountingPeriodModel()
    {
        Id = accountingPeriod.Id,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen,
    };
}