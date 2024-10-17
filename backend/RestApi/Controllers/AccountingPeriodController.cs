using Data;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
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
    private readonly IAccountStartingBalanceRepository _accountStartingBalanceRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="accountingPeriodService">Service used to create or modify Accounting Periods</param>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    /// <param name="accountStartingBalanceRepository">Repository of Account Starting Balances</param>
    public AccountingPeriodController(IUnitOfWork unitOfWork,
        IAccountingPeriodService accountingPeriodService,
        IAccountingPeriodRepository accountingPeriodRepository,
        IAccountStartingBalanceRepository accountStartingBalanceRepository)
    {
        _unitOfWork = unitOfWork;
        _accountingPeriodService = accountingPeriodService;
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountStartingBalanceRepository = accountStartingBalanceRepository;
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
        _accountingPeriodService.CreateNewAccountingPeriod(createAccountingPeriodModel.Year,
            createAccountingPeriodModel.Month,
            out AccountingPeriod newAccountingPeriod,
            out ICollection<AccountStartingBalance> newAccountStartingBalances);
        _accountingPeriodRepository.Add(newAccountingPeriod);
        foreach (AccountStartingBalance startingBalance in newAccountStartingBalances)
        {
            _accountStartingBalanceRepository.Add(startingBalance);
        }
        await _unitOfWork.SaveChangesAsync();
        return Ok(ConvertToModel(newAccountingPeriod));
    }

    /// <summary>
    /// Closes the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("close/{accountingPeriodId}")]
    public async Task<IActionResult> CloseAccountingPeriod(Guid accountingPeriodId)
    {
        AccountingPeriod? accountingPeriod = _accountingPeriodRepository.FindOrNull(accountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        _accountingPeriodService.ClosePeriod(
            accountingPeriod,
            out ICollection<AccountStartingBalance> newAccountStartingBalances);
        _accountingPeriodRepository.Update(accountingPeriod);
        foreach (AccountStartingBalance startingBalance in newAccountStartingBalances)
        {
            _accountStartingBalanceRepository.Add(startingBalance);
        }
        await _unitOfWork.SaveChangesAsync();
        return Ok(ConvertToModel(accountingPeriod));
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