using Data;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.Account;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public class AccountController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountService _accountService;
    private readonly IAccountRepository _accountRepository;
    private readonly IFundRepository _fundRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="accountService">Service that constructs Accounts</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    /// <param name="fundRepository">Repository of Funds</param>
    public AccountController(IUnitOfWork unitOfWork,
        IAccountService accountService,
        IAccountRepository accountRepository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IFundRepository fundRepository)
    {
        _unitOfWork = unitOfWork;
        _accountService = accountService;
        _accountRepository = accountRepository;
        _fundRepository = fundRepository;
    }

    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>A collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAllAccounts() =>
        _accountRepository.FindAll().Select(account => new AccountModel(account)).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult GetAccount(Guid accountId)
    {
        Account? account = _accountRepository.FindByExternalIdOrNull(accountId);
        return account != null ? Ok(new AccountModel(account)) : NotFound();
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountAsync(CreateAccountModel createAccountModel)
    {
        Dictionary<Guid, Fund> funds = _fundRepository.FindAll().ToDictionary(fund => fund.Id.ExternalId, fund => fund);
        Account newAccount = _accountService.CreateNewAccount(createAccountModel.Name, createAccountModel.Type,
            createAccountModel.StartingFundBalances.Select(fundBalance => new FundAmount
            {
                Fund = funds[fundBalance.FundId],
                Amount = fundBalance.Amount,
            }));
        _accountRepository.Add(newAccount);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new AccountModel(newAccount));
    }
}