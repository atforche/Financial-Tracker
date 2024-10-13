using Data;
using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.Account;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts.
/// </summary>
[ApiController]
[Route("/accounts")]
public class AccountController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountService _accountService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountStartingBalanceRepository _accountStartingBalanceRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="accountService">Service that constructs accounts</param>
    /// <param name="accountRepository">Repository of accounts</param>
    /// <param name="accountingPeriodRepository">Repository of accounting periods</param>
    /// <param name="accountStartingBalanceRepository">Repository of account starting balances</param>
    public AccountController(IUnitOfWork unitOfWork,
        IAccountService accountService,
        IAccountRepository accountRepository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IAccountStartingBalanceRepository accountStartingBalanceRepository)
    {
        _unitOfWork = unitOfWork;
        _accountService = accountService;
        _accountRepository = accountRepository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _accountStartingBalanceRepository = accountStartingBalanceRepository;
    }

    /// <summary>
    /// Retrieves all the accounts from the database
    /// </summary>
    /// <returns>A collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAllAccounts() =>
        _accountRepository.FindAll().Select(ConvertToModel).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult GetAccount(Guid accountId)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        return account != null ? Ok(ConvertToModel(account)) : NotFound();
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
            Type = createAccountModel.Type
        };
        var createAccountStartingBalanceRequest = new CreateAccountStartingBalanceRequest
        {
            AccountingPeriod = _accountingPeriodRepository.FindOpenPeriod() ?? throw new InvalidOperationException(),
            StartingBalance = createAccountModel.StartingBalance
        };
        _accountService.CreateNewAccount(createAccountRequest,
            createAccountStartingBalanceRequest,
            out Account newAccount,
            out AccountStartingBalance? newAccountStartingBalance);
        _accountRepository.Add(newAccount);
        _accountStartingBalanceRepository.Add(newAccountStartingBalance);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ConvertToModel(newAccount));
    }

    /// <summary>
    /// Updates an existing Account with the provided properties
    /// </summary>
    /// <param name="accountId">ID of the Account to update</param>
    /// <param name="updateAccountModel">Request to update an Account</param>
    /// <returns>The newly updated Account</returns>
    [HttpPost("{accountId}")]
    public async Task<IActionResult> UpdateAccountAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        if (account == null)
        {
            return NotFound();
        }
        if (updateAccountModel.Name != null)
        {
            _accountService.RenameAccount(account, updateAccountModel.Name);
        }
        if (updateAccountModel.IsActive != null)
        {
            account.IsActive = updateAccountModel.IsActive.Value;
        }
        _accountRepository.Update(account);
        account = _accountRepository.Find(account.Id);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ConvertToModel(account));
    }

    /// <summary>
    /// Deletes an existing Account with the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to delete</param>
    [HttpDelete("{accountId}")]
    public async Task<IActionResult> DeleteAccountAsync(Guid accountId)
    {
        _accountRepository.Delete(accountId);
        await _unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Converts the Account domain entity into an Account REST model
    /// </summary>
    /// <param name="account">Account domain entity to be converted</param>
    /// <returns>The converted Account REST model</returns>
    private AccountModel ConvertToModel(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        IsActive = account.IsActive,
    };
}