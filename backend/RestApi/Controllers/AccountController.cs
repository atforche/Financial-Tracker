using Application.Services;
using Domain.Entities;
using Domain.Repositories;
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
    private readonly IAccountService _accountService;
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountService">Service for manipulating accounts</param>
    /// <param name="accountRepository">Repository of accounts</param>
    public AccountController(IAccountService accountService, IAccountRepository accountRepository)
    {
        _accountService = accountService;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Test endpoint to ensure API is working
    /// </summary>
    /// <returns>A collection of Accounts that match the provided filter criteria.</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> RetrieveAccounts() =>
        _accountRepository.FindAll().Select(ConvertToModel).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID.
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID.</returns>
    [HttpGet("{accountId}")]
    public IActionResult RetrieveAccount(Guid accountId)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        return account != null ? Ok(ConvertToModel(account)) : NotFound();
    }

    /// <summary>
    /// Creates a new Account with the provided properties.
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public IActionResult CreateAccount(CreateAccountModel createAccountModel)
    {
        Account newAccount = _accountService.CreateAccount(createAccountModel.Name,
            createAccountModel.Type,
            createAccountModel.IsActive);
        return Ok(ConvertToModel(newAccount));
    }

    /// <summary>
    /// Updates an existing Account with the provided properties.
    /// </summary>
    /// <param name="accountId">ID of the Account to update</param>
    /// <param name="updateAccountModel">Request to update an Account</param>
    /// <returns>The newly updated Account</returns>
    [HttpPost("{accountId}")]
    public IActionResult UpdateAccount(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        if (account == null)
        {
            return NotFound();
        }
        if (updateAccountModel.Name != null)
        {
            account.Name = updateAccountModel.Name;
        }
        if (updateAccountModel.IsActive != null)
        {
            account.IsActive = updateAccountModel.IsActive.Value;
        }
        _accountRepository.Update(account);
        account = _accountRepository.Find(account.Id);
        return Ok(ConvertToModel(account));
    }

    /// <summary>
    /// Deletes an existing Account with the provided ID.
    /// </summary>
    /// <param name="accountId">ID of the Account to delete</param>
    [HttpDelete("{accountId}")]
    public IActionResult DeleteAccount(Guid accountId)
    {
        _accountRepository.Delete(accountId);
        return Ok();
    }

    /// <summary>
    /// Converst the Account domain entity into an Account model
    /// </summary>
    /// <param name="account">Account domain entity to be converted</param>
    /// <returns>The converted Account model</returns>
    private AccountModel ConvertToModel(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        IsActive = account.IsActive,
    };
}