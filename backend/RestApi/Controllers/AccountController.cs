using Data;
using Domain.Entities;
using Domain.Repositories;
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
    private readonly IAccountStartingBalanceRepository _accountStartingBalanceRepository;
    private readonly IFundRepository _fundRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="accountService">Service that constructs Accounts</param>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountStartingBalanceRepository">Repository of Account Starting Balances</param>
    /// <param name="fundRepository">Repository of Funds</param>
    public AccountController(IUnitOfWork unitOfWork,
        IAccountService accountService,
        IAccountRepository accountRepository,
        IAccountStartingBalanceRepository accountStartingBalanceRepository,
        IFundRepository fundRepository)
    {
        _unitOfWork = unitOfWork;
        _accountService = accountService;
        _accountRepository = accountRepository;
        _accountStartingBalanceRepository = accountStartingBalanceRepository;
        _fundRepository = fundRepository;
    }

    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>A collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAllAccounts() =>
        _accountRepository.FindAll().Select(AccountModel.ConvertEntityToModel).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult GetAccount(Guid accountId)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        return account != null ? Ok(AccountModel.ConvertEntityToModel(account)) : NotFound();
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountAsync(CreateAccountModel createAccountModel)
    {
        var createAccountRequest = new CreateAccountRequest
        {
            Name = createAccountModel.Name,
            Type = createAccountModel.Type,
            StartingFundBalances = createAccountModel.StartingFundBalances
                .Select(startingBalance => new CreateFundAmountRequest
                {
                    Fund = _fundRepository.FindOrNull(startingBalance.FundId) ?? throw new InvalidOperationException(),
                    Amount = startingBalance.Amount,
                })
        };
        _accountService.CreateNewAccount(createAccountRequest,
            out Account newAccount,
            out AccountStartingBalance? newAccountStartingBalance);
        _accountRepository.Add(newAccount);
        _accountStartingBalanceRepository.Add(newAccountStartingBalance);
        await _unitOfWork.SaveChangesAsync();
        return Ok(AccountModel.ConvertEntityToModel(newAccount));
    }
}