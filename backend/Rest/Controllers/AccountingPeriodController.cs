using Data;
using Domain.AccountingPeriods;
using Microsoft.AspNetCore.Mvc;
using Models.AccountingPeriods;
using Models.Errors;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accounting-periods")]
public sealed class AccountingPeriodController(UnitOfWork unitOfWork,
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodService accountingPeriodService,
    AccountingPeriodMapper accountingPeriodMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounting Periods from the database
    /// </summary>
    /// <returns>The collection of all Accounting Periods</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAll() => accountingPeriodRepository.FindAll()
        .OrderByDescending(accountingPeriod => accountingPeriod.PeriodStartDate)
        .Select(AccountingPeriodMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves all the open Accounting Periods from the database
    /// </summary>
    /// <returns>The collection of all open Accounting Periods</returns>
    [HttpGet("open")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAllOpen() => accountingPeriodRepository.FindAllOpenPeriods()
        .OrderByDescending(accountingPeriod => accountingPeriod.PeriodStartDate)
        .Select(AccountingPeriodMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to retrieve</param>
    /// <returns>The Accounting Period that matches the provided ID</returns>
    [HttpGet("{accountingPeriodId}")]
    public IActionResult Get(Guid accountingPeriodId)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(AccountingPeriodMapper.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        if (!accountingPeriodService.TryCreate(createAccountingPeriodModel.Year, createAccountingPeriodModel.Month, out AccountingPeriod? newAccountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Accounting Period.", exceptions));
        }
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountingPeriodMapper.ToModel(newAccountingPeriod));
    }

    /// <summary>
    /// Closes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("{accountingPeriodId}/close")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid accountingPeriodId)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountingPeriodService.TryClose(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to close Accounting Period.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountingPeriodMapper.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Deletes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to delete</param>
    [HttpDelete("{accountingPeriodId}")]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountingPeriodId)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountingPeriodService.TryDelete(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Accounting Period.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}