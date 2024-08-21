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
    private static int accountId = 1;
    private static readonly List<AccountModel> accounts = [
        new AccountModel
        {
            Id = accountId++,
            Name = "Checking",
            Type = AccountType.Standard,
            IsActive = true,
        },
        new AccountModel
        {
            Id = accountId++,
            Name = "Savings",
            Type = AccountType.Standard,
            IsActive = true,
        },
        new AccountModel
        {
            Id = accountId++,
            Name = "Retirement",
            Type = AccountType.Investment,
            IsActive = true,
        },
        new AccountModel
        {
            Id = accountId++,
            Name = "Loan",
            Type = AccountType.Debt,
            IsActive = true,
        },
        new AccountModel
        {
            Id = accountId++,
            Name = "Old",
            Type = AccountType.Standard,
            IsActive = false,
        }
    ];

    /// <summary>
    /// Retrieves a group of accounts from the database that match the provided filter criteria.
    /// </summary>
    /// <returns>A collection of Accounts that match the provided filter criteria.</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> RetrieveAccounts() => accounts;

    /// <summary>
    /// Retrieves the Account that matches the provided ID.
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID.</returns>
    [HttpGet("{accountId}")]
    public IActionResult RetrieveAccount(long accountId)
    {
        AccountModel? account = accounts.FirstOrDefault(account => account.Id == accountId);
        return account != null ? Ok(account) : NotFound();
    }

    /// <summary>
    /// Creates a new Account with the provided properties.
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public IActionResult CreateAccount(CreateAccountModel createAccountModel)
    {
        if (createAccountModel == null)
        {
            return BadRequest();
        }
        var newAccount = new AccountModel
        {
            Id = accountId++,
            Name = createAccountModel.Name,
            Type = createAccountModel.Type,
            IsActive = createAccountModel.IsActive,
        };
        accounts.Add(newAccount);
        return Ok(newAccount);
    }

    /// <summary>
    /// Updates an existing Account with the provided properties.
    /// </summary>
    /// <param name="accountId">ID of the Account to update</param>
    /// <param name="updateAccountModel">Request to update an Account</param>
    /// <returns>THe newly updated Account</returns>
    [HttpPost("{accountId}")]
    public IActionResult UpdateAccount(long accountId, UpdateAccountModel updateAccountModel)
    {
        AccountModel? account = accounts.FirstOrDefault(account => account.Id == accountId);
        if (account == null)
        {
            return NotFound();
        }
        if (updateAccountModel == null)
        {
            return BadRequest();
        }
        if (updateAccountModel.Name != null)
        {
            account.Name = updateAccountModel.Name;
        }
        if (updateAccountModel.IsActive != null)
        {
            account.IsActive = updateAccountModel.IsActive.Value;
        }
        return Ok(account);
    }

    /// <summary>
    /// Deletes an existing Account with the provided ID.
    /// </summary>
    /// <param name="accountId">ID of the Account to delete</param>
    [HttpDelete("{accountId}")]
    public IActionResult DeleteAccount(long accountId)
    {
        int index = accounts.FindIndex(account => account.Id == accountId);
        if (index == -1)
        {
            return NotFound();
        }
        accounts.RemoveAt(index);
        return Ok();
    }
}